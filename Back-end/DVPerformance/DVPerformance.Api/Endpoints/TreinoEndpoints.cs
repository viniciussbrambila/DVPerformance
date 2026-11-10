using System.Globalization;
using System.Text.RegularExpressions;
using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public record AtualizarStatusTreinoRequest(string Status);

public static class TreinoEndpoints
{
    private const int MinSeries = 1;
    private const int MaxSeries = 10;
    private const int MinRepeticoes = 6;
    private const int MaxRepeticoes = 30;
    private const int MinDescansoSegundos = 30;
    private const int MaxDescansoSegundos = 300;
    private const int MinAerobicoMinutos = 10;
    private const int MaxAerobicoMinutos = 100;

    private static readonly HashSet<string> StatusPermitidos = new()
    {
        "Rascunho", "Validado", "Exportado"
    };

    private static readonly HashSet<string> ObjetivosComDicas = new()
    {
        "Hipertrofia", "Emagrecimento", "Força", "Resistência"
    };

    public static void MapTreinoEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/treinos");

        // GET /treinos/divisoes?dias=4 -> Opções compatíveis com a frequência do aluno.
        grupo.MapGet("/divisoes", (int? dias) =>
        {
            if (dias is null or < 2 or > 6)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Informe uma frequência semanal entre 2 e 6 dias."
                });
            }

            return Results.Ok(DivisoesTreino.ObterDisponiveis(dias.Value));
        });

        // GET /treinos?usuarioId=1 -> Lista todos os treinos ativos já enriquecidos com dados do aluno, dica sorteada, status e dias agrupados
        grupo.MapGet("/", async (int? usuarioId, int? alunoId, bool? incluirInativos, AppDbContext db) =>
        {
            var query = db.Treinos
                .AsNoTracking()
                .Include(t => t.Aluno)
                .Include(t => t.DicaObjetivo)
                .Include(t => t.SegundaDicaObjetivo)
                .Include(t => t.Itens.OrderBy(i => i.Id))
                    .ThenInclude(i => i.Exercicio)
                        .ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens)
                    .ThenInclude(i => i.Maquina)
                        .ThenInclude(m => m!.GrupoMuscular)
                .AsQueryable();

            if (incluirInativos != true)
            {
                query = query.Where(t => t.Ativo && (t.Aluno == null || t.Aluno.Ativo));
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                query = query.Where(t => t.UsuarioId == usuarioId.Value);
            }

            if (alunoId.HasValue && alunoId.Value > 0)
            {
                query = query.Where(t => t.AlunoId == alunoId.Value);
            }

            var lista = await query
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();

            return Results.Ok(lista.Select(FormatarPlanoResposta));
        });

        // GET /treinos/{id} -> Busca uma ficha de treino específica pelo ID com todos os itens, dica do objetivo e dias estruturados
        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do treino deve ser um número inteiro positivo." });
            }

            var treino = await db.Treinos
                .AsNoTracking()
                .Include(t => t.Aluno)
                .Include(t => t.DicaObjetivo)
                .Include(t => t.SegundaDicaObjetivo)
                .Include(t => t.Itens.OrderBy(i => i.Id))
                    .ThenInclude(i => i.Exercicio)
                        .ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens)
                    .ThenInclude(i => i.Maquina)
                        .ThenInclude(m => m!.GrupoMuscular)
                .FirstOrDefaultAsync(t => t.Id == id && t.Ativo);

            if (treino is null)
            {
                return Results.NotFound(new { mensagem = "Ficha de treino não encontrada." });
            }

            return Results.Ok(FormatarPlanoResposta(treino));
        });

        // GET /treinos/aluno/{alunoId} -> Lista o histórico de fichas de treino de um aluno específico
        grupo.MapGet("/aluno/{alunoId:int}", async (int alunoId, AppDbContext db) =>
        {
            if (alunoId <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do aluno deve ser um número inteiro positivo." });
            }

            var treinos = await db.Treinos
                .AsNoTracking()
                .Where(t => t.AlunoId == alunoId && t.Ativo)
                .Include(t => t.Aluno)
                .Include(t => t.DicaObjetivo)
                .Include(t => t.SegundaDicaObjetivo)
                .Include(t => t.Itens.OrderBy(i => i.Id))
                    .ThenInclude(i => i.Exercicio)
                        .ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens)
                    .ThenInclude(i => i.Maquina)
                        .ThenInclude(m => m!.GrupoMuscular)
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();

            return Results.Ok(treinos.Select(FormatarPlanoResposta));
        });

        // POST /treinos/gerar/{alunoId} -> Motor Inteligente DV Performance: gera e salva a ficha usando os exercícios e máquinas ativos da conta do Personal
        grupo.MapPost("/gerar/{alunoId:int}", async (int alunoId, int? usuarioId, string? divisao, AppDbContext db) =>
        {
            if (alunoId <= 0)
            {
                return Results.BadRequest(new { mensagem = "Informe um ID de aluno válido para gerar o treino." });
            }

            var aluno = await db.Alunos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == alunoId && a.Ativo);
            if (aluno is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado ou inativo no sistema." });
            }

            var errosEtarios = AlunoEndpoints.ValidarDadosEtarios(aluno);
            if (errosEtarios.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Atualize os dados do aluno antes de gerar o treino.",
                    erros = errosEtarios
                });
            }

            var divisaoSelecionada = DivisoesTreino.Obter(divisao, aluno.FrequenciaSemanal);
            if (divisaoSelecionada is null)
            {
                return Results.BadRequest(new
                {
                    mensagem = $"Selecione uma divisão compatível com os {aluno.FrequenciaSemanal} dias de treino do aluno."
                });
            }

            // Identifica o Personal dono da ficha (pelo query param ou pelo vínculo do próprio aluno)
            var donoId = (usuarioId.HasValue && usuarioId.Value > 0)
                ? usuarioId.Value
                : aluno.UsuarioId;

            if (donoId.HasValue && donoId.Value > 0)
            {
                await CatalogoPadraoService.GarantirCatalogoPorUsuarioIdAsync(db, donoId.Value);
            }

            var gruposMusculares = await db.GruposMusculares.AsNoTracking().ToListAsync();

            var exerciciosDisponiveis = await db.Exercicios
                .AsNoTracking()
                .Include(e => e.GrupoMuscular)
                .Where(e => e.Ativo && (donoId == null || e.UsuarioId == donoId))
                .ToListAsync();

            var maquinasDisponiveis = await db.Maquinas
                .AsNoTracking()
                .Include(m => m.GrupoMuscular)
                .Where(m => m.Ativo && (donoId == null || m.UsuarioId == donoId))
                .ToListAsync();

            if (donoId is null)
            {
                exerciciosDisponiveis = exerciciosDisponiveis
                    .GroupBy(e => (e.GrupoMuscularId, Nome: CatalogoPadraoService.NormalizarNome(e.Nome)))
                    .Select(g => g.First())
                    .ToList();

                maquinasDisponiveis = maquinasDisponiveis
                    .GroupBy(m => (m.GrupoMuscularId, Nome: CatalogoPadraoService.NormalizarNome(m.Nome)))
                    .Select(g => g.First())
                    .ToList();
            }

            if (exerciciosDisponiveis.Count == 0 && maquinasDisponiveis.Count == 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Não há exercícios nem máquinas ativos na sua conta para gerar o treino.",
                    erros = new[] { "Cadastre ou restaure exercícios e máquinas no catálogo antes de gerar uma ficha automática." }
                });
            }

            var errosCatalogo = ValidarCatalogoDivisao(
                divisaoSelecionada, gruposMusculares, exerciciosDisponiveis, maquinasDisponiveis);
            if (errosCatalogo.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "O catálogo não possui todos os grupos musculares necessários para esta divisão.",
                    erros = errosCatalogo
                });
            }

            // Busca todo o histórico cronológico de dicas já recebidas por este aluno (ativos e inativos)
            // para garantir que nenhuma dica se repita até que todas as dicas do objetivo tenham aparecido
            var historicoDicasAluno = await ObterHistoricoDicasAlunoAsync(db, aluno.Id);

            // Sorteia duas dicas distintas do objetivo, sem repetir antes de completar o ciclo.
            var objetivoParaDica = NormalizarObjetivoParaDica(aluno.Objetivo);
            var dicasDoObjetivo = await db.DicasObjetivo
                .AsNoTracking()
                .Where(d => d.Objetivo == objetivoParaDica)
                .ToListAsync();

            var dicasSorteadas = SortearDuasDicasSemRepetirCiclo(dicasDoObjetivo, historicoDicasAluno);
            if (dicasSorteadas.Count != 2)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Cadastre pelo menos duas dicas distintas para o objetivo do aluno antes de gerar o treino."
                });
            }

            // Gera a ficha conforme a divisão escolhida e os parâmetros do aluno
            var novoTreino = GerarTreinoPersonalizado(
                aluno,
                donoId,
                dicasSorteadas[0].Id,
                dicasSorteadas[1].Id,
                gruposMusculares,
                exerciciosDisponiveis,
                maquinasDisponiveis,
                divisaoSelecionada);

            if (novoTreino.Itens.Count == 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Não foi possível montar os itens do treino com o catálogo atual.",
                    erros = new[] { "Cadastre exercícios ou máquinas compatíveis com os grupos musculares do treino." }
                });
            }

            // Desativa treinos anteriores ativos desse aluno para que o novo seja o principal (mantendo histórico)
            var treinosAnteriores = await db.Treinos
                .Where(t => t.AlunoId == aluno.Id && t.Ativo)
                .ToListAsync();

            foreach (var antigo in treinosAnteriores)
            {
                antigo.Ativo = false;
            }

            db.Treinos.Add(novoTreino);
            await db.SaveChangesAsync();

            // Recarrega o treino com as navegações completas (Aluno, DicaObjetivo, SegundaDicaObjetivo, Exercicio, Maquina e GrupoMuscular) para retornar ao cliente
            var treinoSalvo = await db.Treinos
                .AsNoTracking()
                .Include(t => t.Aluno)
                .Include(t => t.DicaObjetivo)
                .Include(t => t.SegundaDicaObjetivo)
                .Include(t => t.Itens.OrderBy(i => i.Id))
                    .ThenInclude(i => i.Exercicio)
                        .ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens)
                    .ThenInclude(i => i.Maquina)
                        .ThenInclude(m => m!.GrupoMuscular)
                .FirstAsync(t => t.Id == novoTreino.Id);

            return Results.Created($"/treinos/{treinoSalvo.Id}", FormatarPlanoResposta(treinoSalvo));
        });

        // PATCH /treinos/{id}/status -> Atualiza o status do plano ("Rascunho", "Validado", "Exportado") no banco PostgreSQL
        grupo.MapPatch("/{id:int}/status", async (int id, AtualizarStatusTreinoRequest? dados, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do treino deve ser um número inteiro positivo." });
            }

            var novoStatus = dados?.Status?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(novoStatus) || !StatusPermitidos.Contains(novoStatus))
            {
                return Results.BadRequest(new
                {
                    mensagem = "Status inválido. Os valores permitidos são: 'Rascunho', 'Validado' ou 'Exportado'."
                });
            }

            var treino = await db.Treinos.FirstOrDefaultAsync(t => t.Id == id && t.Ativo);

            if (treino is null)
            {
                return Results.NotFound(new { mensagem = "Ficha de treino não encontrada ou desativada." });
            }

            treino.Status = novoStatus;
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = treino.Id,
                status = treino.Status
            });
        });

        // Salva a edição completa; os itens omitidos são removidos somente deste plano.
        grupo.MapPut("/{id:int}/itens", async (int id, AtualizarItensTreinoRequest? dados, AppDbContext db) =>
        {
            if (id <= 0 || dados?.Itens is not { Count: > 0 } entradas)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Informe um plano válido e mantenha pelo menos um exercício ou máquina."
                });
            }

            var treino = await db.Treinos
                .Include(t => t.Aluno)
                .Include(t => t.DicaObjetivo)
                .Include(t => t.SegundaDicaObjetivo)
                .Include(t => t.Itens).ThenInclude(i => i.Exercicio).ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens).ThenInclude(i => i.Maquina).ThenInclude(m => m!.GrupoMuscular)
                .FirstOrDefaultAsync(t => t.Id == id && t.Ativo);

            if (treino is null)
            {
                return Results.NotFound(new { mensagem = "Ficha de treino não encontrada ou desativada." });
            }

            var originais = treino.Itens.ToDictionary(i => i.Id);
            if (entradas.Any(i => i is null || i.Id < 0 || (i.Id > 0 && !originais.ContainsKey(i.Id)))
                || entradas.Where(i => i!.Id > 0).Select(i => i!.Id).Distinct().Count()
                    != entradas.Count(i => i!.Id > 0))
            {
                return Results.BadRequest(new
                {
                    mensagem = "Os itens existentes devem pertencer a este plano e não podem estar repetidos. Informe zero como ID dos novos itens."
                });
            }

            var diasOriginais = treino.Itens
                .GroupBy(i => i.DivisaoTreino)
                .ToDictionary(g => g.Key, g => g.OrderBy(i => i.OrdemExercicio).ThenBy(i => i.Id).First().TempoAerobico);
            var idsExercicios = entradas.Where(i => i!.ExercicioId.HasValue).Select(i => i!.ExercicioId!.Value).Distinct().ToList();
            var idsMaquinas = entradas.Where(i => i!.MaquinaId.HasValue).Select(i => i!.MaquinaId!.Value).Distinct().ToList();
            var exercicios = await db.Exercicios.Include(e => e.GrupoMuscular)
                .Where(e => idsExercicios.Contains(e.Id)).ToDictionaryAsync(e => e.Id);
            var maquinas = await db.Maquinas.Include(m => m.GrupoMuscular)
                .Where(m => idsMaquinas.Contains(m.Id)).ToDictionaryAsync(m => m.Id);
            var donoId = treino.UsuarioId ?? treino.Aluno?.UsuarioId;
            var ordensPorDia = new Dictionary<string, int>();
            var aerobicosPorDia = new Dictionary<string, int>();
            var itensPorDia = new HashSet<(string Divisao, int? ExercicioId, int? MaquinaId)>();
            var alteracoes = new List<(ItemTreino? Item, AtualizarItemTreinoRequest Dados, string Divisao, int Ordem, string Repeticao, string Descanso)>();

            foreach (var entrada in entradas)
            {
                var atualizacao = entrada!;
                var item = atualizacao.Id == 0 ? null : originais[atualizacao.Id];
                var divisao = item?.DivisaoTreino ?? atualizacao.DivisaoTreino?.Trim();
                if (string.IsNullOrWhiteSpace(divisao) || !diasOriginais.ContainsKey(divisao)
                    || (item is not null && atualizacao.DivisaoTreino is not null
                        && atualizacao.DivisaoTreino.Trim() != item.DivisaoTreino))
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "Adicione itens somente aos dias existentes e mantenha cada item original no seu dia."
                    });
                }

                if (atualizacao.TempoAerobico is int minutos)
                {
                    if (!TempoAerobicoValido(minutos))
                    {
                        return Results.BadRequest(new
                        {
                            mensagem = $"Informe o aeróbico entre {MinAerobicoMinutos} e {MaxAerobicoMinutos} minutos inteiros. Use zero para removê-lo."
                        });
                    }
                    if (aerobicosPorDia.TryGetValue(divisao, out var informado) && informado != minutos)
                    {
                        return Results.BadRequest(new
                        {
                            mensagem = "Informe a mesma quantidade de minutos inteiros para todos os itens do dia. Use zero para remover o aeróbico."
                        });
                    }
                    aerobicosPorDia[divisao] = minutos;
                }

                if (atualizacao.ExercicioId.HasValue == atualizacao.MaquinaId.HasValue)
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "Selecione apenas um exercício ou uma máquina para cada item do plano."
                    });
                }

                if (!itensPorDia.Add((divisao, atualizacao.ExercicioId, atualizacao.MaquinaId)))
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "Não é permitido repetir o mesmo exercício ou máquina no mesmo dia do treino."
                    });
                }

                if (atualizacao.Series < MinSeries || atualizacao.Series > MaxSeries
                    || !TentarNormalizarRepeticoes(atualizacao.Repeticao ?? "", out var repeticao)
                    || !TentarNormalizarDescanso(atualizacao.TempoDescanso ?? "", out var descanso))
                {
                    return Results.BadRequest(new
                    {
                        mensagem = $"Informe séries entre {MinSeries} e {MaxSeries}, repetições entre {MinRepeticoes} e {MaxRepeticoes} e descanso entre {MinDescansoSegundos} e {MaxDescansoSegundos} segundos."
                    });
                }

                if (atualizacao.ExercicioId.HasValue
                    && (item is null || atualizacao.ExercicioId != item.ExercicioId)
                    && (!exercicios.TryGetValue(atualizacao.ExercicioId.Value, out var exercicio)
                        || !exercicio.Ativo || exercicio.UsuarioId != donoId))
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "Selecione um exercício ativo do catálogo da conta responsável pelo plano."
                    });
                }

                if (atualizacao.MaquinaId.HasValue
                    && (item is null || atualizacao.MaquinaId != item.MaquinaId)
                    && (!maquinas.TryGetValue(atualizacao.MaquinaId.Value, out var maquina)
                        || !maquina.Ativo || maquina.UsuarioId != donoId))
                {
                    return Results.BadRequest(new
                    {
                        mensagem = "Selecione uma máquina ativa do catálogo da conta responsável pelo plano."
                    });
                }

                ordensPorDia.TryGetValue(divisao, out var ordem);
                ordensPorDia[divisao] = ++ordem;
                alteracoes.Add((item, atualizacao, divisao, ordem, repeticao, descanso));
            }

            // Só altera entidades após validar toda a edição. SaveChanges grava em uma transação.
            var idsMantidos = entradas.Where(i => i!.Id > 0).Select(i => i!.Id).ToHashSet();
            var removidos = treino.Itens.Where(i => !idsMantidos.Contains(i.Id)).ToList();
            var houveAlteracao = removidos.Count > 0;
            db.ItensTreino.RemoveRange(removidos);
            treino.Itens.RemoveAll(i => !idsMantidos.Contains(i.Id));

            foreach (var (original, atualizacao, divisao, ordem, repeticao, descanso) in alteracoes)
            {
                var minutos = aerobicosPorDia.GetValueOrDefault(divisao, diasOriginais[divisao]);
                var item = original ?? new ItemTreino
                {
                    TreinoId = treino.Id,
                    DivisaoTreino = divisao,
                    TempoAerobico = diasOriginais[divisao]
                };
                houveAlteracao |= original is null
                    || item.ExercicioId != atualizacao.ExercicioId
                    || item.MaquinaId != atualizacao.MaquinaId
                    || item.Series != atualizacao.Series
                    || item.Repeticao != repeticao
                    || item.TempoDescanso != descanso
                    || item.OrdemExercicio != ordem
                    || item.TempoAerobico != minutos;

                if (item.ExercicioId != atualizacao.ExercicioId)
                {
                    item.ExercicioId = atualizacao.ExercicioId;
                    item.Exercicio = atualizacao.ExercicioId.HasValue ? exercicios[atualizacao.ExercicioId.Value] : null;
                }
                if (item.MaquinaId != atualizacao.MaquinaId)
                {
                    item.MaquinaId = atualizacao.MaquinaId;
                    item.Maquina = atualizacao.MaquinaId.HasValue ? maquinas[atualizacao.MaquinaId.Value] : null;
                }
                item.Series = atualizacao.Series;
                item.Repeticao = repeticao;
                item.TempoDescanso = descanso;
                item.OrdemExercicio = ordem;
                item.TempoAerobico = minutos;

                if (original is null)
                {
                    treino.Itens.Add(item);
                    db.ItensTreino.Add(item);
                }
            }

            if (houveAlteracao)
            {
                treino.Status = "Rascunho";
            }
            await db.SaveChangesAsync();
            return Results.Ok(FormatarPlanoResposta(treino));
        });

        // PUT /treinos/itens/{idItem} -> Permite ao Personal editar um item específico do treino gerado (séries, repetições, descanso, aeróbico ou trocar exercício/máquina)
        grupo.MapPut("/itens/{idItem:int}", async (int idItem, ItemTreino dadosAtualizados, AppDbContext db) =>
        {
            if (idItem <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do item de treino deve ser um número inteiro positivo." });
            }

            var item = await db.ItensTreino.FindAsync(idItem);
            if (item is null)
            {
                return Results.NotFound(new { mensagem = "Item de treino não encontrado." });
            }

            if (dadosAtualizados.Series < MinSeries || dadosAtualizados.Series > MaxSeries)
            {
                return Results.BadRequest(new { mensagem = $"O número de séries deve ser entre {MinSeries} e {MaxSeries}." });
            }

            if (!TempoAerobicoValido(dadosAtualizados.TempoAerobico))
            {
                return Results.BadRequest(new
                {
                    mensagem = $"Informe o aeróbico entre {MinAerobicoMinutos} e {MaxAerobicoMinutos} minutos inteiros. Use zero para removê-lo."
                });
            }

            string? repeticaoNormalizada = null;
            if (!string.IsNullOrWhiteSpace(dadosAtualizados.Repeticao))
            {
                if (!TentarNormalizarRepeticoes(dadosAtualizados.Repeticao, out repeticaoNormalizada))
                {
                    return Results.BadRequest(new
                    {
                        mensagem = $"O número de repetições deve ser entre {MinRepeticoes} e {MaxRepeticoes} (ex.: 10 ou 8-12)."
                    });
                }
            }

            string? descansoNormalizado = null;
            if (!string.IsNullOrWhiteSpace(dadosAtualizados.TempoDescanso))
            {
                if (!TentarNormalizarDescanso(dadosAtualizados.TempoDescanso, out descansoNormalizado))
                {
                    return Results.BadRequest(new
                    {
                        mensagem = $"O tempo de descanso deve ser entre {MinDescansoSegundos} e {MaxDescansoSegundos} segundos."
                    });
                }
            }

            if (dadosAtualizados.ExercicioId.HasValue == dadosAtualizados.MaquinaId.HasValue)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Selecione apenas um exercício ou uma máquina para cada item do plano."
                });
            }

            var divisao = string.IsNullOrWhiteSpace(dadosAtualizados.DivisaoTreino)
                ? item.DivisaoTreino
                : dadosAtualizados.DivisaoTreino.Trim();
            var itemRepetido = await db.ItensTreino.AnyAsync(i =>
                i.Id != idItem && i.TreinoId == item.TreinoId && i.DivisaoTreino == divisao
                && ((dadosAtualizados.ExercicioId.HasValue && i.ExercicioId == dadosAtualizados.ExercicioId)
                    || (dadosAtualizados.MaquinaId.HasValue && i.MaquinaId == dadosAtualizados.MaquinaId)));
            if (itemRepetido)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Não é permitido repetir o mesmo exercício ou máquina no mesmo dia do treino."
                });
            }

            item.DivisaoTreino = divisao;

            if (dadosAtualizados.OrdemExercicio > 0)
            {
                item.OrdemExercicio = dadosAtualizados.OrdemExercicio;
            }

            item.ExercicioId = dadosAtualizados.ExercicioId;
            item.MaquinaId = dadosAtualizados.MaquinaId;
            item.Series = dadosAtualizados.Series;

            if (repeticaoNormalizada is not null)
            {
                item.Repeticao = repeticaoNormalizada;
            }

            if (descansoNormalizado is not null)
            {
                item.TempoDescanso = descansoNormalizado;
            }

            var itensDia = await db.ItensTreino
                .Where(i => i.TreinoId == item.TreinoId && i.DivisaoTreino == item.DivisaoTreino)
                .ToListAsync();
            var alterouAerobico = item.TempoAerobico != dadosAtualizados.TempoAerobico
                || itensDia.Any(i => i.TempoAerobico != dadosAtualizados.TempoAerobico);
            item.TempoAerobico = dadosAtualizados.TempoAerobico;
            foreach (var itemDia in itensDia)
            {
                itemDia.TempoAerobico = dadosAtualizados.TempoAerobico;
            }
            if (alterouAerobico)
            {
                var treino = await db.Treinos.FindAsync(item.TreinoId);
                if (treino is not null) treino.Status = "Rascunho";
            }

            await db.SaveChangesAsync();

            var itemAtualizado = await db.ItensTreino
                .Include(i => i.Exercicio)
                    .ThenInclude(e => e!.GrupoMuscular)
                .Include(i => i.Maquina)
                    .ThenInclude(m => m!.GrupoMuscular)
                .FirstAsync(i => i.Id == idItem);

            return Results.Ok(itemAtualizado);
        });

        // DELETE /treinos/{id} -> Desativa uma ficha de treino (Soft Delete)
        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do treino deve ser um número inteiro positivo." });
            }

            var treino = await db.Treinos.FirstOrDefaultAsync(t => t.Id == id && t.Ativo);
            if (treino is null)
            {
                return Results.NotFound(new { mensagem = "Ficha de treino não encontrada ou já desativada." });
            }

            treino.Ativo = false;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    public static string NormalizarObjetivoParaDica(string? objetivo)
    {
        var valor = objetivo?.Trim() ?? string.Empty;
        return ObjetivosComDicas.Contains(valor) ? valor : "Hipertrofia";
    }

    /// <summary>
    /// Sorteia 1 dica aleatória para o aluno garantindo que nenhuma dica se repita
    /// enquanto todas as outras dicas daquele objetivo ainda não tiverem saído para ele.
    /// Quando o aluno já tiver recebido todas as dicas do objetivo, inicia-se um novo ciclo completo.
    /// </summary>
    public static DicaObjetivo? SortearDicaSemRepetirCiclo(
        List<DicaObjetivo> opcoes,
        IReadOnlyList<int> historicoDicasAluno)
    {
        if (opcoes.Count == 0)
        {
            return null;
        }

        if (opcoes.Count == 1)
        {
            return opcoes[0];
        }

        var idsValidos = opcoes.Select(d => d.Id).ToHashSet();
        var vistasNoCicloAtual = new HashSet<int>();
        int? ultimaDicaDoObjetivo = null;

        foreach (var dicaId in historicoDicasAluno)
        {
            if (!idsValidos.Contains(dicaId))
            {
                continue;
            }

            ultimaDicaDoObjetivo = dicaId;
            vistasNoCicloAtual.Add(dicaId);

            // Ao completar todas as dicas disponíveis daquele objetivo, reinicia o ciclo de dicas para o aluno
            if (vistasNoCicloAtual.Count >= idsValidos.Count)
            {
                vistasNoCicloAtual.Clear();
            }
        }

        var disponiveisNesteCiclo = opcoes
            .Where(d => !vistasNoCicloAtual.Contains(d.Id))
            .ToList();

        // Na virada de um ciclo completo para o próximo, evita repetir seguida exatamente a última dica que acabou de sair
        if (vistasNoCicloAtual.Count == 0 && ultimaDicaDoObjetivo.HasValue && disponiveisNesteCiclo.Count > 1)
        {
            var semRepetirUltimaDaVirada = disponiveisNesteCiclo
                .Where(d => d.Id != ultimaDicaDoObjetivo.Value)
                .ToList();

            if (semRepetirUltimaDaVirada.Count > 0)
            {
                disponiveisNesteCiclo = semRepetirUltimaDaVirada;
            }
        }

        return disponiveisNesteCiclo[Random.Shared.Next(disponiveisNesteCiclo.Count)];
    }

    public static async Task<List<int>> ObterHistoricoDicasAlunoAsync(AppDbContext db, int alunoId)
    {
        var historico = await db.Treinos
            .AsNoTracking()
            .Where(t => t.AlunoId == alunoId)
            .OrderBy(t => t.DataCriacao)
            .ThenBy(t => t.Id)
            .Select(t => new { t.DicaObjetivoId, t.SegundaDicaObjetivoId })
            .ToListAsync();

        return historico
            .SelectMany(t => new[] { t.DicaObjetivoId, t.SegundaDicaObjetivoId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }

    public static List<DicaObjetivo> SortearDuasDicasSemRepetirCiclo(
        List<DicaObjetivo> opcoes,
        IReadOnlyList<int> historicoDicasAluno)
    {
        var distintas = opcoes
            .Where(d => !string.IsNullOrWhiteSpace(d.Texto))
            .GroupBy(d => d.Texto.Trim(), StringComparer.Ordinal)
            .Select(grupo => grupo.First())
            .ToList();
        var historico = historicoDicasAluno.ToList();
        var sorteadas = new List<DicaObjetivo>();

        for (var indice = 0; indice < Math.Min(2, distintas.Count); indice++)
        {
            var dica = SortearDicaSemRepetirCiclo(distintas, historico)!;
            sorteadas.Add(dica);
            historico.Add(dica.Id);
        }

        return sorteadas;
    }

    private static bool TempoAerobicoValido(int minutos) =>
        minutos == 0 || (minutos >= MinAerobicoMinutos && minutos <= MaxAerobicoMinutos);

    private static bool TentarNormalizarRepeticoes(string valorBruto, out string repeticaoNormalizada)
    {
        repeticaoNormalizada = string.Empty;
        var texto = valorBruto.Trim();

        if (int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var unico))
        {
            if (unico < MinRepeticoes || unico > MaxRepeticoes)
            {
                return false;
            }

            repeticaoNormalizada = unico.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        var separadores = new[] { '-', '–' };
        var partes = texto.Split(separadores, StringSplitOptions.TrimEntries);
        if (partes.Length == 2
            && int.TryParse(partes[0], NumberStyles.None, CultureInfo.InvariantCulture, out var minimo)
            && int.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out var maximo)
            && minimo >= MinRepeticoes
            && maximo <= MaxRepeticoes
            && minimo <= maximo)
        {
            repeticaoNormalizada = minimo == maximo
                ? minimo.ToString(CultureInfo.InvariantCulture)
                : $"{minimo}-{maximo}";
            return true;
        }

        return false;
    }

    private static bool TentarNormalizarDescanso(string valorBruto, out string descansoNormalizado)
    {
        descansoNormalizado = string.Empty;
        var texto = valorBruto.Trim();

        foreach (var sufixo in new[] { "segundos", "segundo", "segs", "seg", "s" })
        {
            if (texto.EndsWith(sufixo, StringComparison.OrdinalIgnoreCase))
            {
                texto = texto[..^sufixo.Length].Trim();
                break;
            }
        }

        if (!int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out var segundos)
            || segundos < MinDescansoSegundos
            || segundos > MaxDescansoSegundos)
        {
            return false;
        }

        descansoNormalizado = $"{segundos}s";
        return true;
    }

    // =========================================================================
    // MOTOR DE PRESCRIÇÃO INTELIGENTE DV PERFORMANCE
    // (Divisão escolhida pelo Personal, com parâmetros de experiência e idade)
    // =========================================================================
    public static IReadOnlyList<string> ValidarCatalogoDivisao(
        DivisaoTreinoConfiguracao divisao,
        List<GrupoMuscular> gruposMusculares,
        List<Exercicio> exercicios,
        List<Maquina> maquinas)
    {
        var mapaGrupos = gruposMusculares.ToDictionary(g => g.Nome, StringComparer.Ordinal);
        var erros = new List<string>();
        foreach (var sessao in divisao.Sessoes)
        {
            foreach (var nomeGrupo in sessao.Grupos.Distinct(StringComparer.Ordinal))
            {
                if (!mapaGrupos.TryGetValue(nomeGrupo, out var grupo)
                    || (!exercicios.Any(e => e.Ativo && e.GrupoMuscularId == grupo.Id)
                        && !maquinas.Any(m => m.Ativo && m.GrupoMuscularId == grupo.Id)))
                {
                    erros.Add($"{sessao.Titulo}: cadastre ou restaure ao menos um exercício ou máquina para {nomeGrupo}.");
                }
            }
        }
        return erros;
    }

    public static Treino GerarTreinoPersonalizado(
        Aluno aluno,
        int? usuarioId,
        int? dicaObjetivoId,
        int? segundaDicaObjetivoId,
        List<GrupoMuscular> gruposMusculares,
        List<Exercicio> exercicios,
        List<Maquina> maquinas,
        DivisaoTreinoConfiguracao divisao)
    {
        var mapaGrupos = gruposMusculares.ToDictionary(g => g.Nome, StringComparer.Ordinal);
        var (seriesBase, repeticoes, descansoBase, tempoAerobicoGeral, tempoAerobicoDia) =
            CalcularParametrosFisiologicos(aluno);

        double proporcaoMaquinas = aluno.Experiencia switch
        {
            "Iniciante" => 0.75,
            "Intermediário" => 0.50,
            "Avançado" => 0.40,
            _ => 0.50
        };
        int volumeDia = aluno.Experiencia switch
        {
            "Iniciante" => 5,
            "Intermediário" => 6,
            "Avançado" => 7,
            _ => 5
        };
        if (aluno.Idade > 65)
        {
            proporcaoMaquinas = Math.Max(proporcaoMaquinas, 0.80);
            volumeDia = Math.Min(volumeDia, 5);
        }

        var sufixoSexo = aluno.Sexo == "F" ? "Feminino" : "Masculino";
        var treino = new Treino
        {
            AlunoId = aluno.Id,
            UsuarioId = usuarioId,
            DicaObjetivoId = dicaObjetivoId,
            SegundaDicaObjetivoId = segundaDicaObjetivoId,
            NomePlano = $"Protocolo {sufixoSexo} ({aluno.Experiencia}) - {divisao.Nome}",
            Divisao = divisao.Nome,
            TempoAerobico = tempoAerobicoGeral,
            Status = "Rascunho",
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };

        var usadosNoTreinoExercicio = new HashSet<int>();
        var usadosNoTreinoMaquina = new HashSet<int>();
        for (int indiceDia = 0; indiceDia < divisao.Sessoes.Count; indiceDia++)
        {
            var sessao = divisao.Sessoes[indiceDia];
            var gruposObrigatorios = sessao.Grupos
                .Distinct(StringComparer.Ordinal)
                .Where(mapaGrupos.ContainsKey)
                .Select(nome => mapaGrupos[nome])
                .ToList();
            if (gruposObrigatorios.Count == 0)
                continue;

            // A cobertura muscular vem antes dos slots adicionais de volume.
            var slots = gruposObrigatorios.ToList();
            int quantidadeDesejada = Math.Max(volumeDia, gruposObrigatorios.Count);
            while (slots.Count < quantidadeDesejada)
            {
                int indiceGrupo = (slots.Count - gruposObrigatorios.Count + indiceDia)
                    % gruposObrigatorios.Count;
                slots.Add(gruposObrigatorios[indiceGrupo]);
            }

            var usadosNoDiaExercicio = new HashSet<int>();
            var usadosNoDiaMaquina = new HashSet<int>();
            var selecionadosDoDia = new List<(int Prioridade, Exercicio? Exercicio, Maquina? Maquina)>();
            int metaMaquinasDia = (int)Math.Round(slots.Count * proporcaoMaquinas);
            int maquinasEscolhidasNoDia = 0;
            foreach (var grupoMuscular in slots)
            {
                var (exercicioEscolhido, maquinaEscolhida) = SelecionarMovimentoParaGrupo(
                    grupoMuscular, maquinasEscolhidasNoDia < metaMaquinasDia,
                    exercicios, maquinas,
                    usadosNoTreinoExercicio, usadosNoTreinoMaquina,
                    usadosNoDiaExercicio, usadosNoDiaMaquina);
                // O catálogo pode ter menos movimentos distintos que o volume desejado.
                if (exercicioEscolhido is null && maquinaEscolhida is null)
                    continue;
                if (maquinaEscolhida is not null)
                    maquinasEscolhidasNoDia++;
                selecionadosDoDia.Add((grupoMuscular.OrdemPrioridade, exercicioEscolhido, maquinaEscolhida));
            }

            int ordem = 1;
            foreach (var selecionado in selecionadosDoDia.OrderBy(item => item.Prioridade))
            {
                int seriesItem = seriesBase;
                if (seriesBase >= 4 && selecionado.Prioridade >= 3 && aluno.Experiencia != "Avançado")
                    seriesItem--;
                treino.Itens.Add(new ItemTreino
                {
                    DivisaoTreino = sessao.Titulo,
                    OrdemExercicio = ordem++,
                    ExercicioId = selecionado.Exercicio?.Id,
                    MaquinaId = selecionado.Maquina?.Id,
                    Series = seriesItem,
                    Repeticao = repeticoes,
                    TempoDescanso = descansoBase,
                    TempoAerobico = tempoAerobicoDia
                });
            }
        }
        return treino;
    }

    private static (int series, string repeticoes, string descanso, string aerobicoGeral, int aerobicoDia)
        CalcularParametrosFisiologicos(Aluno aluno)
    {
        int series;
        string repeticoes;
        int descansoSegundos;
        string aerobicoGeral;
        int aerobicoDia;

        switch (aluno.Experiencia)
        {
            case "Iniciante":
                series = 3;
                repeticoes = "12-15";
                descansoSegundos = 60;
                aerobicoGeral = "15 a 20 min de cardio leve a moderado pós-treino";
                aerobicoDia = 15;
                break;

            case "Avançado":
                series = 4;
                repeticoes = "8-12";
                descansoSegundos = 75;
                aerobicoGeral = "20 a 25 min de cardio moderado ou intervalado pós-treino";
                aerobicoDia = 20;
                break;

            case "Intermediário":
            default:
                series = 4;
                repeticoes = "10-12";
                descansoSegundos = 60;
                aerobicoGeral = "20 min de cardio moderado pós-treino";
                aerobicoDia = 20;
                break;
        }

        if (aluno.Idade > 65)
        {
            series = Math.Min(series, 3);
            descansoSegundos += 15;
            aerobicoGeral = "15 min de caminhada ou bicicleta ergométrica leve pós-treino";
            aerobicoDia = 15;
        }

        return (series, repeticoes, $"{descansoSegundos}s", aerobicoGeral, aerobicoDia);
    }

    private static (Exercicio? exercicio, Maquina? maquina) SelecionarMovimentoParaGrupo(
        GrupoMuscular grupoAlvo,
        bool preferirMaquina,
        List<Exercicio> exercicios,
        List<Maquina> maquinas,
        HashSet<int> usadosNoTreinoExercicio,
        HashSet<int> usadosNoTreinoMaquina,
        HashSet<int> usadosNoDiaExercicio,
        HashSet<int> usadosNoDiaMaquina)
    {
        // A divisão determina o grupo exato; outro músculo nunca preenche esta vaga.
        var exerciciosDoGrupo = exercicios
            .Where(e => e.Ativo && e.GrupoMuscularId == grupoAlvo.Id
                && !usadosNoDiaExercicio.Contains(e.Id))
            .ToList();
        var maquinasDoGrupo = maquinas
            .Where(m => m.Ativo && m.GrupoMuscularId == grupoAlvo.Id
                && !usadosNoDiaMaquina.Contains(m.Id))
            .ToList();
        var exerciciosIneditos = exerciciosDoGrupo
            .Where(e => !usadosNoTreinoExercicio.Contains(e.Id)).ToList();
        var maquinasIneditas = maquinasDoGrupo
            .Where(m => !usadosNoTreinoMaquina.Contains(m.Id)).ToList();

        // A variação entre sessões tem precedência sobre o tipo de equipamento.
        if (exerciciosIneditos.Count > 0 || maquinasIneditas.Count > 0)
        {
            exerciciosDoGrupo = exerciciosIneditos;
            maquinasDoGrupo = maquinasIneditas;
        }

        Exercicio? exercicio = null;
        Maquina? maquina = null;
        if (preferirMaquina && maquinasDoGrupo.Count > 0)
            maquina = maquinasDoGrupo[Random.Shared.Next(maquinasDoGrupo.Count)];
        else if (exerciciosDoGrupo.Count > 0)
            exercicio = exerciciosDoGrupo[Random.Shared.Next(exerciciosDoGrupo.Count)];
        else if (maquinasDoGrupo.Count > 0)
            maquina = maquinasDoGrupo[Random.Shared.Next(maquinasDoGrupo.Count)];

        if (exercicio is not null)
        {
            usadosNoDiaExercicio.Add(exercicio.Id);
            usadosNoTreinoExercicio.Add(exercicio.Id);
        }
        if (maquina is not null)
        {
            usadosNoDiaMaquina.Add(maquina.Id);
            usadosNoTreinoMaquina.Add(maquina.Id);
        }
        return (exercicio, maquina);
    }

    // Monta o objeto completo do plano já com dados do Aluno, Dica do Objetivo, Status e Dias agrupados/ordenados para o Front-end
    private static object FormatarPlanoResposta(Treino treino)
    {
        var itensOrdenados = (treino.Itens ?? new List<ItemTreino>())
            .OrderBy(i => i.Id)
            .ToList();

        var dias = itensOrdenados
            .GroupBy(i => string.IsNullOrWhiteSpace(i.DivisaoTreino) ? "Treino" : i.DivisaoTreino)
            .OrderBy(grupoDia =>
            {
                // Mantém A/B/C e A1/B1/C1/A2/B2/C2 mesmo após substituir todos os itens de um dia.
                var prefixo = Regex.Match(grupoDia.Key, @"^Treino ([A-Z])([1-9]\d*)?(?:\s+-|$)");
                var ciclo = 0;
                return prefixo.Success
                    && (prefixo.Groups[2].Value.Length == 0 || int.TryParse(prefixo.Groups[2].Value, out ciclo))
                    ? (ciclo, prefixo.Groups[1].Value[0])
                    : (int.MaxValue, char.MaxValue);
            })
            .Select(grupoDia => new
            {
                nome = grupoDia.Key,
                tempoAerobico = grupoDia.FirstOrDefault()?.TempoAerobico ?? 0,
                exercicios = grupoDia
                    .OrderBy(i => i.OrdemExercicio)
                    .ThenBy(i => i.Id)
                    .Select(item =>
                    {
                        var nomeMovimento = item.Exercicio?.Nome ?? item.Maquina?.Nome ?? "Exercício";
                        var grupoMuscular = item.Exercicio?.Grupo ?? item.Maquina?.Grupo ?? string.Empty;
                        return new
                        {
                            id = item.Id,
                            treinoId = item.TreinoId,
                            divisaoTreino = item.DivisaoTreino,
                            ordemExercicio = item.OrdemExercicio,
                            exercicioId = item.ExercicioId,
                            maquinaId = item.MaquinaId,
                            tipo = item.MaquinaId.HasValue ? "Máquina" : "Exercício",
                            grupo = grupoMuscular,
                            nome = nomeMovimento,
                            series = item.Series,
                            repeticoes = item.Repeticao,
                            descanso = item.TempoDescanso,
                            tempoAerobico = item.TempoAerobico
                        };
                    })
                    .ToList()
            })
            .ToList();

        return new
        {
            id = treino.Id,
            alunoId = treino.AlunoId,
            usuarioId = treino.UsuarioId,
            aluno = treino.Aluno?.Nome ?? $"Aluno #{treino.AlunoId}",
            peso = treino.Aluno?.Peso,
            altura = treino.Aluno?.Altura,
            dataCadastro = treino.Aluno?.DataCadastro,
            objetivo = treino.Aluno?.Objetivo ?? "-",
            experiencia = treino.Aluno?.Experiencia ?? "-",
            frequencia = treino.Aluno?.FrequenciaSemanal ?? dias.Count,
            nomePlano = treino.NomePlano,
            divisao = treino.Divisao,
            tempoAerobico = treino.TempoAerobico,
            dicaObjetivoId = treino.DicaObjetivoId,
            segundaDicaObjetivoId = treino.SegundaDicaObjetivoId,
            dicas = new[] { treino.DicaObjetivo?.Texto, treino.SegundaDicaObjetivo?.Texto }
                .Where(texto => !string.IsNullOrWhiteSpace(texto))
                .Select(texto => texto!)
                .Distinct(StringComparer.Ordinal)
                .ToArray(),
            dica = treino.DicaObjetivo?.Texto ?? string.Empty,
            dicaObjetivo = treino.DicaObjetivo is null ? null : new
            {
                id = treino.DicaObjetivo.Id,
                objetivo = treino.DicaObjetivo.Objetivo,
                texto = treino.DicaObjetivo.Texto
            },
            status = string.IsNullOrWhiteSpace(treino.Status) ? "Rascunho" : treino.Status,
            data = treino.DataCriacao,
            dataCriacao = treino.DataCriacao,
            ativo = treino.Ativo,
            dias,
            itens = itensOrdenados
        };
    }
}
