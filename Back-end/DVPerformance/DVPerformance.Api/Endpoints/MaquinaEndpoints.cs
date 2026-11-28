using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using DVPerformance.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class MaquinaEndpoints
{
    public static void MapMaquinaEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/maquinas" (RF03 - Gerenciar Catálogo de Máquinas)
        var grupo = app.MapGroup("/maquinas");

        // GET /maquinas?usuarioId=1 -> Lista as máquinas ativas da conta do Personal (pré-cadastradas + personalizadas)
        grupo.MapGet("/", async (int? usuarioId, bool? incluirInativos, HttpContext httpContext, AppDbContext db) =>
        {
            if (AutorizacaoEndpoints.ValidarMesmoUsuario(httpContext, usuarioId) is IResult erroPropriedade)
            {
                return erroPropriedade;
            }

            var idResolvido = AutorizacaoEndpoints.ResolverUsuarioId(httpContext, usuarioId);

            if (idResolvido.HasValue && idResolvido.Value > 0)
            {
                await CatalogoPadraoService.GarantirCatalogoPorUsuarioIdAsync(db, idResolvido.Value);
            }

            var query = db.Maquinas
                .AsNoTracking()
                .Include(m => m.GrupoMuscular)
                .AsQueryable();

            if (incluirInativos != true)
            {
                query = query.Where(m => m.Ativo);
            }

            if (idResolvido.HasValue && idResolvido.Value > 0)
            {
                // RF03: Retorna exclusivamente as máquinas da conta deste Personal
                query = query.Where(m => m.UsuarioId == idResolvido.Value);

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
        grupo.MapGet("/{id:int}", async (int id, HttpContext httpContext, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID da máquina deve ser um número inteiro positivo." });
            }

            var maquina = await db.Maquinas
                .AsNoTracking()
                .Include(m => m.GrupoMuscular)
                .FirstOrDefaultAsync(m => m.Id == id && m.Ativo);

            if (maquina is null)
            {
                return Results.NotFound(new { mensagem = "Máquina não encontrada ou desativada." });
            }

            if (AutorizacaoEndpoints.ValidarMesmoUsuario(httpContext, maquina.UsuarioId) is IResult erroPropriedade)
            {
                return erroPropriedade;
            }

            return Results.Ok(maquina);
        });

        // POST /maquinas -> Cadastra uma nova máquina na conta do Personal vinculada a grupos_musculares (RF03 / RN18)
        grupo.MapPost("/", async (int? usuarioId, Maquina? novaMaquina, HttpContext httpContext, AppDbContext db) =>
        {
            var idAutenticado = AutorizacaoEndpoints.ResolverUsuarioId(httpContext, usuarioId);

            if (novaMaquina is not null && (!novaMaquina.UsuarioId.HasValue || novaMaquina.UsuarioId.Value <= 0) && idAutenticado.HasValue && idAutenticado.Value > 0)
            {
                novaMaquina.UsuarioId = idAutenticado.Value;
            }

            var (erros, grupoMuscular) = await ValidarMaquinaAsync(novaMaquina, db, exigirUsuarioId: true);
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
            var donoId = novaMaquina.UsuarioId!.Value;

            // RN18: A atualização e manutenção do catálogo de máquinas deve ser restrita ao Administrador
            var (usuario, erroPermissao) = await AutorizacaoEndpoints.ExigirAdministradorAtivoAsync(
                db,
                httpContext,
                donoId,
                "RN18: Apenas usuários com perfil de Administrador podem cadastrar ou manter máquinas no catálogo.");
            if (erroPermissao is not null)
            {
                return erroPermissao;
            }

            await CatalogoPadraoService.GarantirCatalogoDoPersonalAsync(db, usuario!);

            var mesmoNomeExistente = await db.Maquinas
                .Include(m => m.GrupoMuscular)
                .FirstOrDefaultAsync(m =>
                    m.UsuarioId == donoId &&
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
                mesmoNomeExistente.FocoMuscular = novaMaquina.FocoMuscular;
                mesmoNomeExistente.NivelMinimo = novaMaquina.NivelMinimo;
                mesmoNomeExistente.Ativo = true;

                await db.SaveChangesAsync();
                return Results.Created($"/maquinas/{mesmoNomeExistente.Id}", mesmoNomeExistente);
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

        // PUT /maquinas/{id} -> Atualiza uma máquina ativa existente na conta do Personal (RF03 / RN18)
        grupo.MapPut("/{id:int}", async (int id, Maquina? dadosAtualizados, HttpContext httpContext, AppDbContext db) =>
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

            if (AutorizacaoEndpoints.ValidarMesmoUsuario(httpContext, maquinaExistente.UsuarioId) is IResult erroPropriedadeExistente)
            {
                return erroPropriedadeExistente;
            }

            if (AutorizacaoEndpoints.ValidarMesmoUsuario(httpContext, dadosAtualizados?.UsuarioId) is IResult erroPropriedadeNovo)
            {
                return erroPropriedadeNovo;
            }

            var (erros, grupoMuscular) = await ValidarMaquinaAsync(dadosAtualizados, db, exigirUsuarioId: false);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos na atualização da máquina.",
                    erros
                });
            }

            var donoVerificarId = maquinaExistente.UsuarioId
                ?? ((dadosAtualizados!.UsuarioId.HasValue && dadosAtualizados.UsuarioId.Value > 0)
                    ? dadosAtualizados.UsuarioId.Value
                    : AutorizacaoEndpoints.ResolverUsuarioId(httpContext, null));

            if (donoVerificarId.HasValue && donoVerificarId.Value > 0)
            {
                var (_, erroPermissao) = await AutorizacaoEndpoints.ExigirAdministradorAtivoAsync(
                    db,
                    httpContext,
                    donoVerificarId.Value,
                    "RN18: Apenas usuários com perfil de Administrador podem alterar máquinas do catálogo.");
                if (erroPermissao is not null)
                {
                    return erroPermissao;
                }

                if (!maquinaExistente.UsuarioId.HasValue)
                {
                    maquinaExistente.UsuarioId = donoVerificarId.Value;
                }
            }

            maquinaExistente.Nome = dadosAtualizados!.Nome.Trim();
            maquinaExistente.GrupoMuscularId = grupoMuscular!.Id;
            maquinaExistente.GrupoMuscular = grupoMuscular;
            maquinaExistente.FocoMuscular = dadosAtualizados.FocoMuscular;
            maquinaExistente.NivelMinimo = dadosAtualizados.NivelMinimo;
            maquinaExistente.Descricao = dadosAtualizados.Descricao?.Trim() ?? string.Empty;

            await db.SaveChangesAsync();

            return Results.Ok(maquinaExistente);
        });

        // DELETE /maquinas/{id} -> Remove a máquina da conta do Personal (Soft Delete - preserva integridade de planos vigentes conforme RN17 e valida RN18)
        grupo.MapDelete("/{id:int}", async (int id, int? usuarioId, HttpContext httpContext, AppDbContext db) =>
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

            if (AutorizacaoEndpoints.ValidarMesmoUsuario(httpContext, maquina.UsuarioId) is IResult erroPropriedade)
            {
                return erroPropriedade;
            }

            var donoVerificarId = maquina.UsuarioId ?? AutorizacaoEndpoints.ResolverUsuarioId(httpContext, usuarioId);
            if (donoVerificarId.HasValue && donoVerificarId.Value > 0)
            {
                var (_, erroPermissao) = await AutorizacaoEndpoints.ExigirAdministradorAtivoAsync(
                    db,
                    httpContext,
                    donoVerificarId.Value,
                    "RN18: Apenas usuários com perfil de Administrador podem remover máquinas do catálogo.");
                if (erroPermissao is not null)
                {
                    return erroPermissao;
                }
            }

            maquina.Ativo = false;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    // Valida os atributos da Máquina conforme a seção 8.1 (Dicionário de Dados - pág. 21) do Documento de Estágio
    private static async Task<(List<string> erros, GrupoMuscular? grupoMuscular)> ValidarMaquinaAsync(
        Maquina? maquina,
        AppDbContext db,
        bool exigirUsuarioId)
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

        // 4. Vínculo obrigatório com o Personal/Administrador responsável no cadastro
        if (exigirUsuarioId && (!maquina.UsuarioId.HasValue || maquina.UsuarioId.Value <= 0))
        {
            erros.Add("Informe o ID do usuário (usuarioId) responsável pelo cadastro da máquina.");
        }

        if (grupoEncontrado is not null)
        {
            if (!string.IsNullOrWhiteSpace(maquina.FocoMuscular)
                && !FocosMusculares.Valido(grupoEncontrado.Nome, maquina.FocoMuscular))
            {
                erros.Add("Selecione um foco muscular válido para o grupo informado.");
            }
            else
            {
                var focoResolvido = FocosMusculares.Resolver(grupoEncontrado.Nome, nome, maquina.FocoMuscular);
                if (focoResolvido == "geral")
                {
                    erros.Add("Selecione um foco muscular específico (padrão de movimento) para que a máquina se encaixe corretamente na geração de treinos.");
                }
                else
                {
                    maquina.FocoMuscular = focoResolvido;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(maquina.NivelMinimo) && !FocosMusculares.NivelValido(maquina.NivelMinimo))
        {
            erros.Add("Selecione um nível mínimo de experiência válido (Iniciante, Intermediário ou Avançado).");
        }
        else
        {
            maquina.NivelMinimo = FocosMusculares.ResolverNivelMinimo(nome, maquina.NivelMinimo);
        }

        return (erros, grupoEncontrado);
    }
}
