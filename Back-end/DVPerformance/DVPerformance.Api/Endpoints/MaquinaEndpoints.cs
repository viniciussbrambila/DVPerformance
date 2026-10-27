using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class MaquinaEndpoints
{
    // Mapa oficial: Grupo Muscular -> Região Corporal (Subtítulo/Categoria)
    private static readonly Dictionary<string, string> RegiaoPorGrupo = new()
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

    public static void MapMaquinaEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/maquinas" (RF03 - Gerenciar Catálogo de Máquinas)
        var grupo = app.MapGroup("/maquinas");

        // GET /maquinas?usuarioId=1 -> Lista as máquinas ativas (padrões do sistema + do usuário logado)
        grupo.MapGet("/", async (int? usuarioId, bool? incluirInativos, AppDbContext db) =>
        {
            var query = db.Maquinas.AsQueryable();

            if (incluirInativos != true)
            {
                query = query.Where(m => m.Ativo);
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                // RF03: Retorna máquinas padrão (UsuarioId == null) e as vinculadas ao usuário
                query = query.Where(m => m.UsuarioId == null || m.UsuarioId == usuarioId.Value);
            }

            var maquinas = await query
                .OrderBy(m => m.Regiao)
                .ThenBy(m => m.Grupo)
                .ThenBy(m => m.Nome)
                .ToListAsync();

            return Results.Ok(maquinas);
        });

        // GET /maquinas/{id} -> Busca uma máquina ativa específica pelo Id no banco
        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID da máquina deve ser um número inteiro positivo." });
            }

            var maquina = await db.Maquinas.FirstOrDefaultAsync(m => m.Id == id && m.Ativo);
            if (maquina is null)
            {
                return Results.NotFound(new { mensagem = "Máquina não encontrada ou desativada." });
            }

            return Results.Ok(maquina);
        });

        // POST /maquinas -> Cadastra uma nova máquina no banco PostgreSQL (RF03 / RN18)
        grupo.MapPost("/", async (Maquina? novaMaquina, AppDbContext db) =>
        {
            var erros = ValidarMaquina(novaMaquina);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos no cadastro da máquina.",
                    erros
                });
            }

            if (novaMaquina!.UsuarioId.HasValue && novaMaquina.UsuarioId.Value > 0)
            {
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == novaMaquina.UsuarioId.Value && u.Ativo);
                if (usuario is null)
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "O usuário informado não foi encontrado.",
                        erros = new[] { "A conta informada (usuarioId) não existe ou está desativada no banco de dados." }
                    });
                }

                // RN18: A atualização e manutenção do catálogo de máquinas deve ser restrita ao Administrador
                if (!string.Equals(usuario.Perfil, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Json(new
                    {
                        mensagem = "Acesso negado.",
                        erros = new[] { "RN18: Apenas usuários com perfil de Administrador podem cadastrar ou manter máquinas no catálogo." }
                    }, statusCode: StatusCodes.Status403Forbidden);
                }
            }

            var grupoSelecionado = novaMaquina.Grupo.Trim();

            novaMaquina.Id = 0; // Garante geração automática de ID pelo PostgreSQL (SERIAL)
            novaMaquina.Nome = novaMaquina.Nome.Trim();
            novaMaquina.Grupo = grupoSelecionado;
            novaMaquina.Regiao = RegiaoPorGrupo[grupoSelecionado];
            novaMaquina.Descricao = novaMaquina.Descricao?.Trim() ?? string.Empty;
            novaMaquina.Ativo = true;

            db.Maquinas.Add(novaMaquina);
            await db.SaveChangesAsync();

            return Results.Created($"/maquinas/{novaMaquina.Id}", novaMaquina);
        });

        // PUT /maquinas/{id} -> Atualiza uma máquina ativa existente no banco (RF03 / RN18)
        grupo.MapPut("/{id:int}", async (int id, Maquina? dadosAtualizados, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID da máquina deve ser um número inteiro positivo." });
            }

            var maquinaExistente = await db.Maquinas.FirstOrDefaultAsync(m => m.Id == id && m.Ativo);
            if (maquinaExistente is null)
            {
                return Results.NotFound(new { mensagem = "Máquina não encontrada ou desativada." });
            }

            var erros = ValidarMaquina(dadosAtualizados);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos na atualização da máquina.",
                    erros
                });
            }

            if (dadosAtualizados!.UsuarioId.HasValue && dadosAtualizados.UsuarioId.Value > 0)
            {
                var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == dadosAtualizados.UsuarioId.Value && u.Ativo);
                if (usuario is null)
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "O usuário informado não foi encontrado.",
                        erros = new[] { "A conta informada (usuarioId) não existe ou está desativada no banco de dados." }
                    });
                }

                // RN18: Restrito ao Administrador
                if (!string.Equals(usuario.Perfil, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Results.Json(new
                    {
                        mensagem = "Acesso negado.",
                        erros = new[] { "RN18: Apenas usuários com perfil de Administrador podem alterar máquinas do catálogo." }
                    }, statusCode: StatusCodes.Status403Forbidden);
                }

                maquinaExistente.UsuarioId = dadosAtualizados.UsuarioId;
            }

            var grupoSelecionado = dadosAtualizados.Grupo.Trim();

            maquinaExistente.Nome = dadosAtualizados.Nome.Trim();
            maquinaExistente.Grupo = grupoSelecionado;
            maquinaExistente.Regiao = RegiaoPorGrupo[grupoSelecionado];
            maquinaExistente.Descricao = dadosAtualizados.Descricao?.Trim() ?? string.Empty;

            await db.SaveChangesAsync();

            return Results.Ok(maquinaExistente);
        });

        // DELETE /maquinas/{id} -> Desativa a máquina (Soft Delete - preserva integridade de planos vigentes conforme RN17)
        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID da máquina deve ser um número inteiro positivo." });
            }

            var maquina = await db.Maquinas.FirstOrDefaultAsync(m => m.Id == id && m.Ativo);
            if (maquina is null)
            {
                return Results.NotFound(new { mensagem = "Máquina não encontrada ou já desativada." });
            }

            maquina.Ativo = false;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // PATCH /maquinas/{id}/reativar -> Reativa uma máquina desativada anteriormente
        grupo.MapPatch("/{id:int}/reativar", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID da máquina deve ser um número inteiro positivo." });
            }

            var maquina = await db.Maquinas.FindAsync(id);
            if (maquina is null)
            {
                return Results.NotFound(new { mensagem = "Máquina não encontrada." });
            }

            maquina.Ativo = true;
            await db.SaveChangesAsync();

            return Results.Ok(maquina);
        });
    }

    // Valida os atributos da Máquina conforme a seção 8.1 (Dicionário de Dados - pág. 21) do Documento de Estágio
    private static List<string> ValidarMaquina(Maquina? maquina)
    {
        var erros = new List<string>();

        if (maquina is null)
        {
            erros.Add("Os dados da máquina não foram enviados.");
            return erros;
        }

        // 1. Validação do Nome (Dicionário de Dados: varchar 100, Não Nulo / Obrigatório)
        var nome = maquina.Nome?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nome))
        {
            erros.Add("O nome da máquina é obrigatório.");
        }
        else if (nome.Length < 2 || nome.Length > 100)
        {
            erros.Add("O nome da máquina deve ter entre 2 e 100 caracteres.");
        }

        // 2. Validação do Grupo Muscular (Dicionário de Dados: grupoMuscular varchar 50, Não Nulo / Obrigatório)
        var grupo = maquina.Grupo?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(grupo) || !RegiaoPorGrupo.ContainsKey(grupo))
        {
            erros.Add("Selecione um grupo muscular válido na lista.");
        }

        // 3. Validação da Descrição (Dicionário de Dados: descricao varchar 255, Nulo = Sim / Opcional)
        var descricao = maquina.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length > 255)
        {
            erros.Add("A descrição da máquina deve ter no máximo 255 caracteres.");
        }

        return erros;
    }
}
