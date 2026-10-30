using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class MaquinaEndpoints
{
    public static void MapMaquinaEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/maquinas" (RF03 - Gerenciar Catálogo de Máquinas)
        var grupo = app.MapGroup("/maquinas");

        // GET /maquinas?usuarioId=1 -> Lista as máquinas ativas da conta do Personal (pré-cadastradas + personalizadas)
        grupo.MapGet("/", async (int? usuarioId, bool? incluirInativos, AppDbContext db) =>
        {
            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                await CatalogoPadraoService.GarantirCatalogoPorUsuarioIdAsync(db, usuarioId.Value);
            }

            var query = db.Maquinas
                .Include(m => m.GrupoMuscular)
                .AsQueryable();

            if (incluirInativos != true)
            {
                query = query.Where(m => m.Ativo);
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                // RF03: Retorna as máquinas da conta deste Personal (e eventuais registros sem vínculo)
                query = query.Where(m => m.UsuarioId == usuarioId.Value || m.UsuarioId == null);

                var maquinasPersonal = await query
                    .OrderBy(m => m.GrupoMuscular!.Regiao)
                    .ThenBy(m => m.GrupoMuscular!.OrdemPrioridade)
                    .ThenBy(m => m.GrupoMuscular!.Nome)
                    .ThenBy(m => m.Nome)
                    .ToListAsync();

                return Results.Ok(maquinasPersonal);
            }

            // Caso a requisição não informe usuarioId (ex.: testes gerais), evita exibir cópias repetidas entre múltiplos Personals
            var todas = await query
                .OrderBy(m => m.Id)
                .ToListAsync();

            var maquinasUnicas = todas
                .GroupBy(m => (m.GrupoMuscularId, Nome: CatalogoPadraoService.NormalizarNome(m.Nome)))
                .Select(g => g.First())
                .OrderBy(m => m.GrupoMuscular!.Regiao)
                .ThenBy(m => m.GrupoMuscular!.OrdemPrioridade)
                .ThenBy(m => m.GrupoMuscular!.Nome)
                .ThenBy(m => m.Nome)
                .ToList();

            return Results.Ok(maquinasUnicas);
        });

        // GET /maquinas/{id} -> Busca uma máquina ativa específica pelo Id no banco
        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID da máquina deve ser um número inteiro positivo." });
            }

            var maquina = await db.Maquinas
                .Include(m => m.GrupoMuscular)
                .FirstOrDefaultAsync(m => m.Id == id && m.Ativo);

            if (maquina is null)
            {
                return Results.NotFound(new { mensagem = "Máquina não encontrada ou desativada." });
            }

            return Results.Ok(maquina);
        });

        // POST /maquinas -> Cadastra uma nova máquina na conta do Personal vinculada a grupos_musculares (RF03 / RN18)
        grupo.MapPost("/", async (Maquina? novaMaquina, AppDbContext db) =>
        {
            var (erros, grupoMuscular) = await ValidarMaquinaAsync(novaMaquina, db);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos no cadastro da máquina.",
                    erros
                });
            }

            var nomeLimpo = novaMaquina!.Nome.Trim();
            var descricaoLimpa = novaMaquina.Descricao?.Trim() ?? string.Empty;

            if (novaMaquina.UsuarioId.HasValue && novaMaquina.UsuarioId.Value > 0)
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

                await CatalogoPadraoService.GarantirCatalogoDoPersonalAsync(db, usuario);

                var mesmoNomeExistente = await db.Maquinas
                    .Include(m => m.GrupoMuscular)
                    .FirstOrDefaultAsync(m =>
                        m.UsuarioId == novaMaquina.UsuarioId.Value &&
                        m.Nome.ToLower() == nomeLimpo.ToLower());

                if (mesmoNomeExistente is not null)
                {
                    if (mesmoNomeExistente.Ativo)
                    {
                        return Results.Conflict(new
                        {
                            mensagem = "Esta máquina já está cadastrada na sua conta.",
                            erros = new[] { "Você já possui uma máquina ativa com este nome no seu catálogo." }
                        });
                    }

                    // Se estava removida (Soft Delete), reativa e atualiza com os dados informados pelo Personal
                    mesmoNomeExistente.Nome = nomeLimpo;
                    mesmoNomeExistente.GrupoMuscularId = grupoMuscular!.Id;
                    mesmoNomeExistente.GrupoMuscular = grupoMuscular;
                    mesmoNomeExistente.Descricao = descricaoLimpa;
                    mesmoNomeExistente.Ativo = true;

                    await db.SaveChangesAsync();
                    return Results.Created($"/maquinas/{mesmoNomeExistente.Id}", mesmoNomeExistente);
                }
            }

            novaMaquina.Id = 0; // Garante geração automática de ID pelo PostgreSQL (SERIAL)
            novaMaquina.Nome = nomeLimpo;
            novaMaquina.GrupoMuscularId = grupoMuscular!.Id;
            novaMaquina.GrupoMuscular = grupoMuscular;
            novaMaquina.Descricao = descricaoLimpa;
            novaMaquina.Ativo = true;

            db.Maquinas.Add(novaMaquina);
            await db.SaveChangesAsync();

            return Results.Created($"/maquinas/{novaMaquina.Id}", novaMaquina);
        });

        // POST /maquinas/restaurar-padrao?usuarioId=1 -> Restaura as máquinas pré-cadastradas que o Personal tenha removido, mantendo as criadas por ele
        grupo.MapPost("/restaurar-padrao", async (int usuarioId, AppDbContext db) =>
        {
            if (usuarioId <= 0)
            {
                return Results.BadRequest(new { mensagem = "Informe um usuarioId válido para restaurar as máquinas padrão." });
            }

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);
            if (usuario is null)
            {
                return Results.NotFound(new { mensagem = "Conta de Personal não encontrada ou desativada." });
            }

            if (!string.Equals(usuario.Perfil, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Json(new
                {
                    mensagem = "Acesso negado.",
                    erros = new[] { "RN18: Apenas usuários com perfil de Administrador podem manter máquinas no catálogo." }
                }, statusCode: StatusCodes.Status403Forbidden);
            }

            var (inseridos, reativados) = await CatalogoPadraoService.RestaurarMaquinasPadraoAsync(db, usuarioId);

            return Results.Ok(new
            {
                usuarioId,
                inseridos,
                reativados,
                totalPadrao = CatalogoPadraoService.MaquinasPadrao.Count,
                mensagem = "Máquinas pré-cadastradas restauradas com sucesso na conta do Personal."
            });
        });

        // PUT /maquinas/{id} -> Atualiza uma máquina ativa existente na conta do Personal (RF03 / RN18)
        grupo.MapPut("/{id:int}", async (int id, Maquina? dadosAtualizados, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID da máquina deve ser um número inteiro positivo." });
            }

            var maquinaExistente = await db.Maquinas
                .Include(m => m.GrupoMuscular)
                .FirstOrDefaultAsync(m => m.Id == id && m.Ativo);

            if (maquinaExistente is null)
            {
                return Results.NotFound(new { mensagem = "Máquina não encontrada ou desativada." });
            }

            var (erros, grupoMuscular) = await ValidarMaquinaAsync(dadosAtualizados, db);
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

            maquinaExistente.Nome = dadosAtualizados.Nome.Trim();
            maquinaExistente.GrupoMuscularId = grupoMuscular!.Id;
            maquinaExistente.GrupoMuscular = grupoMuscular;
            maquinaExistente.Descricao = dadosAtualizados.Descricao?.Trim() ?? string.Empty;

            await db.SaveChangesAsync();

            return Results.Ok(maquinaExistente);
        });

        // DELETE /maquinas/{id} -> Remove a máquina da conta do Personal (Soft Delete - preserva integridade de planos vigentes conforme RN17)
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

            var maquina = await db.Maquinas
                .Include(m => m.GrupoMuscular)
                .FirstOrDefaultAsync(m => m.Id == id);

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
    private static async Task<(List<string> erros, GrupoMuscular? grupoMuscular)> ValidarMaquinaAsync(
        Maquina? maquina,
        AppDbContext db)
    {
        var erros = new List<string>();

        if (maquina is null)
        {
            erros.Add("Os dados da máquina não foram enviados.");
            return (erros, null);
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

        // 2. Validação do Grupo Muscular (aceita tanto o ID da tabela grupos_musculares quanto o nome enviado pelo Front-end)
        GrupoMuscular? grupoEncontrado = null;
        if (maquina.GrupoMuscularId > 0)
        {
            grupoEncontrado = await db.GruposMusculares.FindAsync(maquina.GrupoMuscularId);
        }
        else if (!string.IsNullOrWhiteSpace(maquina.Grupo))
        {
            var nomeGrupo = maquina.Grupo.Trim();
            grupoEncontrado = await db.GruposMusculares
                .FirstOrDefaultAsync(g => g.Nome == nomeGrupo);
        }

        if (grupoEncontrado is null)
        {
            erros.Add("Selecione um grupo muscular válido na lista.");
        }

        // 3. Validação da Descrição (Dicionário de Dados: descricao varchar 255, Nulo = Sim / Opcional)
        var descricao = maquina.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length > 255)
        {
            erros.Add("A descrição da máquina deve ter no máximo 255 caracteres.");
        }

        return (erros, grupoEncontrado);
    }
}
