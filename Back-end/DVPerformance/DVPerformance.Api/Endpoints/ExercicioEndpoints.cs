using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class ExercicioEndpoints
{
    public static void MapExercicioEndpoints(this WebApplication app)
    {
        // Rota auxiliar para consultar a tabela unificada de Grupos Musculares e suas prioridades (1 a 4)
        app.MapGet("/grupos-musculares", async (AppDbContext db) =>
        {
            var grupos = await db.GruposMusculares
                .OrderBy(g => g.OrdemPrioridade)
                .ThenBy(g => g.Nome)
                .ToListAsync();

            return Results.Ok(grupos);
        });

        // Cria um grupo de rotas com o prefixo "/exercicios"
        var grupo = app.MapGroup("/exercicios");

        // GET /exercicios?usuarioId=1 -> Lista os exercícios ativos (ou todos se incluirInativos=true)
        grupo.MapGet("/", async (int? usuarioId, bool? incluirInativos, AppDbContext db) =>
        {
            var query = db.Exercicios
                .Include(e => e.GrupoMuscular)
                .AsQueryable();

            if (incluirInativos != true)
            {
                query = query.Where(e => e.Ativo);
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                // Retorna exercícios globais (UsuarioId == null) e os criados por este Personal
                query = query.Where(e => e.UsuarioId == null || e.UsuarioId == usuarioId.Value);
            }

            var exercicios = await query
                .OrderBy(e => e.GrupoMuscular!.Regiao)
                .ThenBy(e => e.GrupoMuscular!.OrdemPrioridade)
                .ThenBy(e => e.GrupoMuscular!.Nome)
                .ThenBy(e => e.Nome)
                .ToListAsync();

            return Results.Ok(exercicios);
        });

        // GET /exercicios/{id} -> Busca um exercício ativo específico pelo Id no banco
        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do exercício deve ser um número inteiro positivo." });
            }

            var exercicio = await db.Exercicios
                .Include(e => e.GrupoMuscular)
                .FirstOrDefaultAsync(e => e.Id == id && e.Ativo);

            if (exercicio is null)
            {
                return Results.NotFound(new { mensagem = "Exercício não encontrado ou desativado." });
            }

            return Results.Ok(exercicio);
        });

        // POST /exercicios -> Cadastra um novo exercício no banco PostgreSQL vinculado a grupos_musculares
        grupo.MapPost("/", async (Exercicio? novoExercicio, AppDbContext db) =>
        {
            var (erros, grupoMuscular) = await ValidarExercicioAsync(novoExercicio, db);
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
                var personalExiste = await db.Usuarios.AnyAsync(u => u.Id == novoExercicio.UsuarioId.Value && u.Ativo);
                if (!personalExiste)
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "O Personal informado não foi encontrado.",
                        erros = new[] { "A conta do Personal (usuarioId) não existe ou está desativada no banco de dados." }
                    });
                }
            }

            novoExercicio.Id = 0; // Garante geração automática de ID pelo PostgreSQL (SERIAL)
            novoExercicio.Nome = novoExercicio.Nome.Trim();
            novoExercicio.GrupoMuscularId = grupoMuscular!.Id;
            novoExercicio.GrupoMuscular = grupoMuscular;
            novoExercicio.Descricao = novoExercicio.Descricao?.Trim() ?? string.Empty;
            novoExercicio.Ativo = true;

            db.Exercicios.Add(novoExercicio);
            await db.SaveChangesAsync();

            return Results.Created($"/exercicios/{novoExercicio.Id}", novoExercicio);
        });

        // PUT /exercicios/{id} -> Atualiza um exercício ativo existente no banco
        grupo.MapPut("/{id:int}", async (int id, Exercicio? dadosAtualizados, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do exercício deve ser um número inteiro positivo." });
            }

            var exercicioExistente = await db.Exercicios
                .Include(e => e.GrupoMuscular)
                .FirstOrDefaultAsync(e => e.Id == id && e.Ativo);

            if (exercicioExistente is null)
            {
                return Results.NotFound(new { mensagem = "Exercício não encontrado ou desativado." });
            }

            var (erros, grupoMuscular) = await ValidarExercicioAsync(dadosAtualizados, db);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos na atualização do exercício.",
                    erros
                });
            }

            exercicioExistente.Nome = dadosAtualizados!.Nome.Trim();
            exercicioExistente.GrupoMuscularId = grupoMuscular!.Id;
            exercicioExistente.GrupoMuscular = grupoMuscular;
            exercicioExistente.Descricao = dadosAtualizados.Descricao?.Trim() ?? string.Empty;

            if (dadosAtualizados.UsuarioId.HasValue && dadosAtualizados.UsuarioId.Value > 0)
            {
                exercicioExistente.UsuarioId = dadosAtualizados.UsuarioId;
            }

            await db.SaveChangesAsync();

            return Results.Ok(exercicioExistente);
        });

        // DELETE /exercicios/{id} -> Desativa o exercício (Soft Delete) mantendo o histórico no banco
        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do exercício deve ser um número inteiro positivo." });
            }

            var exercicio = await db.Exercicios.FirstOrDefaultAsync(e => e.Id == id && e.Ativo);
            if (exercicio is null)
            {
                return Results.NotFound(new { mensagem = "Exercício não encontrado ou já desativado." });
            }

            exercicio.Ativo = false;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // PATCH /exercicios/{id}/reativar -> Reativa um exercício desativado anteriormente
        grupo.MapPatch("/{id:int}/reativar", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do exercício deve ser um número inteiro positivo." });
            }

            var exercicio = await db.Exercicios
                .Include(e => e.GrupoMuscular)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exercicio is null)
            {
                return Results.NotFound(new { mensagem = "Exercício não encontrado." });
            }

            exercicio.Ativo = true;
            await db.SaveChangesAsync();

            return Results.Ok(exercicio);
        });
    }

    private static async Task<(List<string> erros, GrupoMuscular? grupoMuscular)> ValidarExercicioAsync(
        Exercicio? exercicio,
        AppDbContext db)
    {
        var erros = new List<string>();

        if (exercicio is null)
        {
            erros.Add("Os dados do exercício não foram enviados.");
            return (erros, null);
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

        // 2. Validação do Grupo Muscular (aceita tanto o ID da tabela grupos_musculares quanto o nome enviado pelo Front-end)
        GrupoMuscular? grupoEncontrado = null;
        if (exercicio.GrupoMuscularId > 0)
        {
            grupoEncontrado = await db.GruposMusculares.FindAsync(exercicio.GrupoMuscularId);
        }
        else if (!string.IsNullOrWhiteSpace(exercicio.Grupo))
        {
            var nomeGrupo = exercicio.Grupo.Trim();
            grupoEncontrado = await db.GruposMusculares
                .FirstOrDefaultAsync(g => g.Nome == nomeGrupo);
        }

        if (grupoEncontrado is null)
        {
            erros.Add("Selecione um grupo muscular válido na lista.");
        }

        // 3. Validação da Descrição (opcional, máximo 255 caracteres)
        var descricao = exercicio.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length > 255)
        {
            erros.Add("A descrição do exercício deve ter no máximo 255 caracteres.");
        }

        return (erros, grupoEncontrado);
    }
}
