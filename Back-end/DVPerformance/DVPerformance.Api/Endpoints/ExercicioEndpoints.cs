using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class ExercicioEndpoints
{
    // Mapa oficial: Grupo Muscular -> Região Corporal (Subtítulo/Categoria)
    private static readonly Dictionary<string, string>RegiaoPorGrupo = new()
    {
        // Membros Superiores (Parte de Cima)
        ["Peito"] = "Membros Superiores",
        ["Costas"] = "Membros Superiores",
        ["Trapézio"] = "Membros Superiores",
        ["Ombros"] = "Membros Superiores",
        ["Bíceps"] = "Membros Superiores",
        ["Tríceps"] = "Membros Superiores",
        ["Antebraço"] = "Membros Superiores",

        // Membros Inferiores (Parte de Baixo)
        ["Quadríceps"] = "Membros Inferiores",
        ["Posterior de Coxa"] = "Membros Inferiores",
        ["Glúteos"] = "Membros Inferiores",
        ["Adutores / Abdutores"] = "Membros Inferiores",
        ["Panturrilha"] = "Membros Inferiores",

        // Core / Tronco
        ["Abdômen"] = "Core / Tronco",
        ["Lombar"] = "Core / Tronco"
    };

    public static void MapExercicioEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/exercicios"
        var grupo = app.MapGroup("/exercicios");

        // GET /exercicios?usuarioId=1 -> Lista os exercícios cadastrados (padrões + do Personal logado)
        grupo.MapGet("/", async (int? usuarioId, AppDbContext db) =>
        {
            var query = db.Exercicios.AsQueryable();

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                // Retorna exercícios globais (UsuarioId == null) e os criados por este Personal
                query = query.Where(e => e.UsuarioId == null || e.UsuarioId == usuarioId.Value);
            }

            var exercicios = await query
                .OrderBy(e => e.Regiao)
                .ThenBy(e => e.Grupo)
                .ThenBy(e => e.Nome)
                .ToListAsync();

            return Results.Ok(exercicios);
        });

        // GET /exercicios/{id} -> Busca um exercício específico pelo Id no banco
        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do exercício deve ser um número inteiro positivo." });
            }

            var exercicio = await db.Exercicios.FindAsync(id);
            if (exercicio is null)
            {
                return Results.NotFound(new { mensagem = "Exercício não encontrado." });
            }

            return Results.Ok(exercicio);
        });

        // POST /exercicios -> Cadastra um novo exercício no banco PostgreSQL
        grupo.MapPost("/", async (Exercicio? novoExercicio, AppDbContext db) =>
        {
            var erros = ValidarExercicio(novoExercicio);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos no cadastro do exercício.",
                    erros
                });
            }

            if (novoExercicio!.UsuarioId.HasValue && novoExercicio.UsuarioId.Value > 0)
            {
                var personalExiste = await db.Usuarios.AnyAsync(u => u.Id == novoExercicio.UsuarioId.Value);
                if (!personalExiste)
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "O Personal informado não foi encontrado.",
                        erros = new[] { "A conta do Personal (usuarioId) não existe no banco de dados." }
                    });
                }
            }

            var grupoSelecionado = novoExercicio.Grupo.Trim();

            novoExercicio.Id = 0; // Garante geração automática de ID pelo PostgreSQL (SERIAL)
            novoExercicio.Nome = novoExercicio.Nome.Trim();
            novoExercicio.Grupo = grupoSelecionado;
            novoExercicio.Regiao = RegiaoPorGrupo[grupoSelecionado];
            novoExercicio.Descricao = novoExercicio.Descricao?.Trim() ?? string.Empty;

            db.Exercicios.Add(novoExercicio);
            await db.SaveChangesAsync();

            return Results.Created($"/exercicios/{novoExercicio.Id}", novoExercicio);
        });

        // PUT /exercicios/{id} -> Atualiza um exercício existente no banco
        grupo.MapPut("/{id:int}", async (int id, Exercicio? dadosAtualizados, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do exercício deve ser um número inteiro positivo." });
            }

            var exercicioExistente = await db.Exercicios.FindAsync(id);
            if (exercicioExistente is null)
            {
                return Results.NotFound(new { mensagem = "Exercício não encontrado." });
            }

            var erros = ValidarExercicio(dadosAtualizados);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos na atualização do exercício.",
                    erros
                });
            }

            var grupoSelecionado = dadosAtualizados!.Grupo.Trim();

            exercicioExistente.Nome = dadosAtualizados.Nome.Trim();
            exercicioExistente.Grupo = grupoSelecionado;
            exercicioExistente.Regiao = RegiaoPorGrupo[grupoSelecionado];
            exercicioExistente.Descricao = dadosAtualizados.Descricao?.Trim() ?? string.Empty;

            if (dadosAtualizados.UsuarioId.HasValue && dadosAtualizados.UsuarioId.Value > 0)
            {
                exercicioExistente.UsuarioId = dadosAtualizados.UsuarioId;
            }

            await db.SaveChangesAsync();

            return Results.Ok(exercicioExistente);
        });

        // DELETE /exercicios/{id} -> Remove um exercício pelo Id do banco
        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do exercício deve ser um número inteiro positivo." });
            }

            var exercicio = await db.Exercicios.FindAsync(id);
            if (exercicio is null)
            {
                return Results.NotFound(new { mensagem = "Exercício não encontrado." });
            }

            db.Exercicios.Remove(exercicio);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    private static List<string> ValidarExercicio(Exercicio? exercicio)
    {
        var erros = new List<string>();

        if (exercicio is null)
        {
            erros.Add("Os dados do exercício não foram enviados.");
            return erros;
        }

        // 1. Validação do Nome (obrigatório, entre 2 e 100 caracteres)
        var nome = exercicio.Nome?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nome))
        {
            erros.Add("O nome do exercício é obrigatório.");
        }
        else if (nome.Length < 2 || nome.Length > 100)
        {
            erros.Add("O nome do exercício deve ter entre 2 e 100 caracteres.");
        }

        // 2. Validação do Grupo Muscular (Select obrigatório entre os grupos padronizados)
        var grupo = exercicio.Grupo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(grupo) || !RegiaoPorGrupo.ContainsKey(grupo))
        {
            erros.Add("Selecione um grupo muscular válido na lista.");
        }

        // 3. Validação da Descrição (opcional, máximo 255 caracteres)
        var descricao = exercicio.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length > 255)
        {
            erros.Add("A descrição do exercício deve ter no máximo 255 caracteres.");
        }

        return erros;
    }
}
