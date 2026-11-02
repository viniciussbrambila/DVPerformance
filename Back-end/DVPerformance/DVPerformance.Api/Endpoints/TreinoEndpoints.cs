using System.Globalization;
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

        // GET /treinos?usuarioId=1 -> Lista todos os treinos ativos já enriquecidos com dados do aluno, dica sorteada, status e dias agrupados
        grupo.MapGet("/", async (int? usuarioId, int? alunoId, bool? incluirInativos, AppDbContext db) =>
        {
            var query = db.Treinos
                .AsNoTracking()
                .Include(t => t.Aluno)
                .Include(t => t.DicaObjetivo)
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
        grupo.MapPost("/gerar/{alunoId:int}", async (int alunoId, int? usuarioId, AppDbContext db) =>
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

            // Busca todo o histórico cronológico de dicas já recebidas por este aluno (ativos e inativos)
            // para garantir que nenhuma dica se repita até que todas as dicas do objetivo tenham aparecido
            var historicoDicasAluno = await db.Treinos
                .AsNoTracking()
                .Where(t => t.AlunoId == aluno.Id && t.DicaObjetivoId != null)
                .OrderBy(t => t.DataCriacao)
                .ThenBy(t => t.Id)
                .Select(t => t.DicaObjetivoId!.Value)
                .ToListAsync();

            // Desativa treinos anteriores ativos desse aluno para que o novo seja o principal (mantendo histórico)
            var treinosAnteriores = await db.Treinos
                .Where(t => t.AlunoId == aluno.Id && t.Ativo)
                .ToListAsync();

            foreach (var antigo in treinosAnteriores)
            {
                antigo.Ativo = false;
            }

            // Sorteia 1 nova dica contextualizada conforme o objetivo do aluno sem repetir no ciclo atual
            var objetivoParaDica = NormalizarObjetivoParaDica(aluno.Objetivo);
            var dicasDoObjetivo = await db.DicasObjetivo
                .AsNoTracking()
                .Where(d => d.Ativo && d.Objetivo == objetivoParaDica)
                .ToListAsync();

            var dicaSorteada = SortearDicaSemRepetirCiclo(dicasDoObjetivo, historicoDicasAluno);

            // Executa o Motor de Prescrição Inteligente baseado exclusivamente em Experiência, Sexo, Frequência Semanal e Idade
            var novoTreino = GerarTreinoPersonalizado(
                aluno,
                donoId,
                dicaSorteada?.Id,
                gruposMusculares,
                exerciciosDisponiveis,
                maquinasDisponiveis);

            if (novoTreino.Itens.Count == 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Não foi possível montar os itens do treino com o catálogo atual.",
                    erros = new[] { "Cadastre exercícios ou máquinas compatíveis com os grupos musculares do treino." }
                });
            }

            db.Treinos.Add(novoTreino);
            await db.SaveChangesAsync();

            // Recarrega o treino com as navegações completas (Aluno, DicaObjetivo, Exercicio, Maquina e GrupoMuscular) para retornar ao cliente
            var treinoSalvo = await db.Treinos
                .AsNoTracking()
                .Include(t => t.Aluno)
                .Include(t => t.DicaObjetivo)
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

            if (!string.IsNullOrWhiteSpace(dadosAtualizados.DivisaoTreino))
            {
                item.DivisaoTreino = dadosAtualizados.DivisaoTreino.Trim();
            }

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

            if (dadosAtualizados.TempoAerobico is not null)
            {
                item.TempoAerobico = dadosAtualizados.TempoAerobico.Trim();
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
    // (Baseado exclusivamente em Experiência, Sexo, Frequência Semanal e Idade)
    // =========================================================================
    private static Treino GerarTreinoPersonalizado(
        Aluno aluno,
        int? usuarioId,
        int? dicaObjetivoId,
        List<GrupoMuscular> gruposMusculares,
        List<Exercicio> exercicios,
        List<Maquina> maquinas)
    {
        var mapaGrupos = gruposMusculares.ToDictionary(g => g.Nome, g => g);

        // 1. Parâmetros base de Séries, Repetições, Descanso e Aeróbico conforme EXPERIÊNCIA e IDADE (sem influência do Objetivo)
        var (seriesBase, repeticoes, descansoBase, tempoAerobicoGeral, tempoAerobicoDia) =
            CalcularParametrosFisiologicos(aluno);

        // 2. Proporção entre Máquinas Guiadas vs Exercícios Livres conforme Nível de Experiência e Idade (RN01)
        double proporcaoMaquinas = aluno.Experiencia switch
        {
            "Iniciante" => 0.75,
            "Intermediário" => 0.50,
            "Avançado" => 0.40,
            _ => 0.50
        };

        if (aluno.Idade > 65)
        {
            proporcaoMaquinas = Math.Max(proporcaoMaquinas, 0.80);
        }

        // 3. Monta a Matriz de Divisão Semanal conforme Sexo, Frequência Semanal, Experiência e Idade
        var (nomeDivisao, diasPlanejados) = ObterDivisaoSemanal(aluno);

        var sufixoSexo = aluno.Sexo == "F" ? "Feminino" : "Masculino";
        var nomePlano = $"Protocolo {sufixoSexo} ({aluno.Experiencia}) - {nomeDivisao}";

        var treino = new Treino
        {
            AlunoId = aluno.Id,
            UsuarioId = usuarioId,
            DicaObjetivoId = dicaObjetivoId,
            NomePlano = nomePlano,
            Divisao = nomeDivisao,
            TempoAerobico = tempoAerobicoGeral,
            Status = "Rascunho",
            DataCriacao = DateTime.UtcNow,
            Ativo = true,
            Itens = new List<ItemTreino>()
        };

        // 4. Para cada Dia de Treino, ordena os grupos musculares pela OrdemPrioridade da tabela grupos_musculares
        // (1 = Músculo Grande -> 2 = Médio -> 3 = Pequeno -> 4 = Core/Finalizador)
        var usadosNoTreinoExercicio = new HashSet<int>();
        var usadosNoTreinoMaquina = new HashSet<int>();

        foreach (var (tituloDia, nomesGruposDoDia) in diasPlanejados)
        {
            var gruposOrdenadosPorPrioridade = nomesGruposDoDia
                .Where(nome => mapaGrupos.ContainsKey(nome))
                .Select(nome => mapaGrupos[nome])
                .OrderBy(g => g.OrdemPrioridade)
                .ToList();

            var selecionadosDoDia = new List<(int prioridade, Exercicio? ex, Maquina? maq)>();
            int totalSlotsDia = gruposOrdenadosPorPrioridade.Count;
            int metaMaquinasDia = (int)Math.Round(totalSlotsDia * proporcaoMaquinas);
            int maquinasEscolhidasNoDia = 0;

            foreach (var grupoMuscular in gruposOrdenadosPorPrioridade)
            {
                bool preferirMaquina = maquinasEscolhidasNoDia < metaMaquinasDia;

                // Busca somente entre os exercícios e máquinas reais cadastrados no banco
                var (exercicioEscolhido, maquinaEscolhida) = SelecionarMovimentoParaGrupo(
                    grupoMuscular,
                    preferirMaquina,
                    exercicios,
                    maquinas,
                    usadosNoTreinoExercicio,
                    usadosNoTreinoMaquina);

                // Se não houver nenhum exercício/máquina cadastrado para aquele grupo nem na mesma região, pula o slot sem criar item vazio
                if (exercicioEscolhido is null && maquinaEscolhida is null)
                {
                    continue;
                }

                if (maquinaEscolhida is not null)
                {
                    maquinasEscolhidasNoDia++;
                }

                int prioridadeReal = exercicioEscolhido?.GrupoMuscular?.OrdemPrioridade
                    ?? maquinaEscolhida?.GrupoMuscular?.OrdemPrioridade
                    ?? grupoMuscular.OrdemPrioridade;

                selecionadosDoDia.Add((prioridadeReal, exercicioEscolhido, maquinaEscolhida));
            }

            // Garante que mesmo em caso de substituição por outro exercício da região, a ordem final respeite estritamente 1 -> 2 -> 3 -> 4
            int ordem = 1;
            foreach (var itemSelecionado in selecionadosDoDia.OrderBy(s => s.prioridade))
            {
                int seriesItem = seriesBase;
                if (seriesBase >= 4 && itemSelecionado.prioridade >= 3 && aluno.Experiencia != "Avançado")
                {
                    seriesItem = seriesBase - 1;
                }

                treino.Itens.Add(new ItemTreino
                {
                    DivisaoTreino = tituloDia,
                    OrdemExercicio = ordem++,
                    ExercicioId = itemSelecionado.ex?.Id,
                    MaquinaId = itemSelecionado.maq?.Id,
                    Series = seriesItem,
                    Repeticao = repeticoes,
                    TempoDescanso = descansoBase,
                    TempoAerobico = tempoAerobicoDia
                });
            }
        }

        return treino;
    }

    private static (int series, string repeticoes, string descanso, string aerobicoGeral, string aerobicoDia)
        CalcularParametrosFisiologicos(Aluno aluno)
    {
        int series;
        string repeticoes;
        int descansoSegundos;
        string aerobicoGeral;
        string aerobicoDia;

        switch (aluno.Experiencia)
        {
            case "Iniciante":
                series = 3;
                repeticoes = "12-15";
                descansoSegundos = 60;
                aerobicoGeral = "15 a 20 min de cardio leve a moderado pós-treino";
                aerobicoDia = "15 min Esteira ou Bike leve/moderada";
                break;

            case "Avançado":
                series = 4;
                repeticoes = "8-12";
                descansoSegundos = 75;
                aerobicoGeral = "20 a 25 min de cardio moderado ou intervalado pós-treino";
                aerobicoDia = "20 min Cardio moderado/intervalado pós-treino";
                break;

            case "Intermediário":
            default:
                series = 4;
                repeticoes = "10-12";
                descansoSegundos = 60;
                aerobicoGeral = "20 min de cardio moderado pós-treino";
                aerobicoDia = "20 min Esteira, Bike ou Elíptico moderado";
                break;
        }

        if (aluno.Idade > 65)
        {
            series = Math.Min(series, 3);
            descansoSegundos += 15;
            aerobicoGeral = "15 min de caminhada ou bicicleta ergométrica leve pós-treino";
            aerobicoDia = "15 min Bike ergométrica / Caminhada leve";
        }

        return (series, repeticoes, $"{descansoSegundos}s", aerobicoGeral, aerobicoDia);
    }

    private static (string nomeDivisao, List<(string tituloDia, List<string> grupos)> dias)
        ObterDivisaoSemanal(Aluno aluno)
    {
        bool feminino = aluno.Sexo == "F";
        int freq = Math.Clamp(aluno.FrequenciaSemanal, 2, 6);

        int volumeDia = aluno.Experiencia switch
        {
            "Iniciante" => 5,
            "Intermediário" => 6,
            "Avançado" => 7,
            _ => 5
        };

        if (aluno.Idade > 65)
        {
            volumeDia = Math.Min(volumeDia, 5);
        }

        var dias = new List<(string tituloDia, List<string> grupos)>();
        string nomeDivisao;

        if (freq == 2)
        {
            nomeDivisao = "Full Body / AB (2x)";
            if (feminino)
            {
                dias.Add(("Treino A - Full Body (Ênfase Quadríceps, Glúteos e Superiores)", AjustarVolume(new[]
                {
                    "Quadríceps", "Glúteos", "Costas", "Peito", "Ombros", "Abdômen", "Panturrilha"
                }, volumeDia)));

                dias.Add(("Treino B - Full Body (Ênfase Posterior, Glúteos e Superiores)", AjustarVolume(new[]
                {
                    "Posterior de Coxa", "Glúteos", "Adutores / Abdutores", "Costas", "Tríceps", "Abdômen", "Lombar"
                }, volumeDia)));
            }
            else
            {
                dias.Add(("Treino A - Full Body (Ênfase Peito, Costas e Quadríceps)", AjustarVolume(new[]
                {
                    "Peito", "Costas", "Quadríceps", "Ombros", "Bíceps", "Abdômen", "Panturrilha"
                }, volumeDia)));

                dias.Add(("Treino B - Full Body (Ênfase Costas, Peito e Posterior)", AjustarVolume(new[]
                {
                    "Costas", "Peito", "Posterior de Coxa", "Ombros", "Tríceps", "Glúteos", "Lombar"
                }, volumeDia)));
            }
        }
        else if (freq == 3)
        {
            if (feminino)
            {
                nomeDivisao = "ABA Ênfase Inferiores (3x)";

                dias.Add(("Treino A - Inferiores (Foco Quadríceps, Glúteos e Core)", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Glúteos", "Adutores / Abdutores", "Panturrilha", "Abdômen", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino B - Membros Superiores e Core", AjustarVolume(new[]
                {
                    "Costas", "Peito", "Ombros", "Tríceps", "Bíceps", "Abdômen", "Lombar"
                }, volumeDia)));

                dias.Add(("Treino C - Inferiores (Foco Posterior, Glúteos e Core)", AjustarVolume(new[]
                {
                    "Posterior de Coxa", "Posterior de Coxa", "Glúteos", "Glúteos", "Adutores / Abdutores", "Abdômen", "Panturrilha"
                }, volumeDia)));
            }
            else
            {
                nomeDivisao = "ABC Clássico (3x)";

                dias.Add(("Treino A - Peito, Ombros e Tríceps", AjustarVolume(new[]
                {
                    "Peito", "Peito", "Ombros", "Ombros", "Tríceps", "Tríceps", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino B - Costas, Trapézio e Bíceps", AjustarVolume(new[]
                {
                    "Costas", "Costas", "Trapézio", "Bíceps", "Bíceps", "Antebraço", "Lombar"
                }, volumeDia)));

                dias.Add(("Treino C - Membros Inferiores e Core", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Posterior de Coxa", "Glúteos", "Adutores / Abdutores", "Panturrilha", "Abdômen"
                }, volumeDia)));
            }
        }
        else if (freq == 4)
        {
            nomeDivisao = feminino ? "ABCD Ênfase Inferiores (4x)" : "AB 2x - Superior / Inferior (4x)";

            if (feminino)
            {
                dias.Add(("Treino A - Quadríceps, Adutores e Panturrilha", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Quadríceps", "Adutores / Abdutores", "Panturrilha", "Abdômen", "Panturrilha"
                }, volumeDia)));

                dias.Add(("Treino B - Costas, Ombros e Bíceps", AjustarVolume(new[]
                {
                    "Costas", "Costas", "Ombros", "Ombros", "Bíceps", "Abdômen", "Lombar"
                }, volumeDia)));

                dias.Add(("Treino C - Glúteos e Posterior de Coxa", AjustarVolume(new[]
                {
                    "Glúteos", "Glúteos", "Posterior de Coxa", "Posterior de Coxa", "Adutores / Abdutores", "Lombar", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino D - Peito, Ombros, Tríceps e Core", AjustarVolume(new[]
                {
                    "Peito", "Ombros", "Tríceps", "Tríceps", "Costas", "Abdômen", "Abdômen"
                }, volumeDia)));
            }
            else
            {
                dias.Add(("Treino A - Superiores (Ênfase Peito, Ombros e Tríceps)", AjustarVolume(new[]
                {
                    "Peito", "Peito", "Costas", "Ombros", "Tríceps", "Bíceps", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino B - Inferiores (Ênfase Quadríceps e Panturrilha)", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Posterior de Coxa", "Glúteos", "Adutores / Abdutores", "Panturrilha", "Lombar"
                }, volumeDia)));

                dias.Add(("Treino C - Superiores (Ênfase Costas, Trapézio e Bíceps)", AjustarVolume(new[]
                {
                    "Costas", "Costas", "Peito", "Trapézio", "Ombros", "Bíceps", "Antebraço"
                }, volumeDia)));

                dias.Add(("Treino D - Inferiores (Ênfase Posterior, Glúteos e Core)", AjustarVolume(new[]
                {
                    "Posterior de Coxa", "Posterior de Coxa", "Quadríceps", "Glúteos", "Panturrilha", "Abdômen", "Abdômen"
                }, volumeDia)));
            }
        }
        else if (freq == 5)
        {
            nomeDivisao = feminino ? "ABCDE Feminino Especializado (5x)" : "ABCDE Masculino Especializado (5x)";

            if (feminino)
            {
                dias.Add(("Treino A - Quadríceps e Panturrilha", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Quadríceps", "Adutores / Abdutores", "Panturrilha", "Panturrilha", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino B - Costas, Ombros e Bíceps", AjustarVolume(new[]
                {
                    "Costas", "Costas", "Ombros", "Ombros", "Bíceps", "Trapézio", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino C - Glúteos e Posterior de Coxa", AjustarVolume(new[]
                {
                    "Glúteos", "Glúteos", "Posterior de Coxa", "Posterior de Coxa", "Adutores / Abdutores", "Lombar", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino D - Peito, Ombros, Tríceps e Core", AjustarVolume(new[]
                {
                    "Peito", "Peito", "Ombros", "Tríceps", "Tríceps", "Abdômen", "Lombar"
                }, volumeDia)));

                dias.Add(("Treino E - Inferiores Completo (Foco Glúteos e Quadríceps)", AjustarVolume(new[]
                {
                    "Glúteos", "Quadríceps", "Posterior de Coxa", "Adutores / Abdutores", "Panturrilha", "Abdômen", "Abdômen"
                }, volumeDia)));
            }
            else
            {
                dias.Add(("Treino A - Peito e Tríceps", AjustarVolume(new[]
                {
                    "Peito", "Peito", "Peito", "Ombros", "Tríceps", "Tríceps", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino B - Costas, Trapézio e Antebraço", AjustarVolume(new[]
                {
                    "Costas", "Costas", "Costas", "Trapézio", "Bíceps", "Antebraço", "Lombar"
                }, volumeDia)));

                dias.Add(("Treino C - Membros Inferiores (Quadríceps e Panturrilha)", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Quadríceps", "Posterior de Coxa", "Adutores / Abdutores", "Panturrilha", "Panturrilha"
                }, volumeDia)));

                dias.Add(("Treino D - Ombros, Trapézio e Abdômen", AjustarVolume(new[]
                {
                    "Peito", "Ombros", "Ombros", "Ombros", "Trapézio", "Abdômen", "Abdômen"
                }, volumeDia)));

                dias.Add(("Treino E - Posterior, Glúteos, Bíceps e Tríceps", AjustarVolume(new[]
                {
                    "Posterior de Coxa", "Glúteos", "Bíceps", "Bíceps", "Tríceps", "Antebraço", "Lombar"
                }, volumeDia)));
            }
        }
        else
        {
            nomeDivisao = feminino ? "ABC 2x Feminino (6x)" : "ABC 2x Masculino (6x)";

            if (feminino)
            {
                dias.Add(("Treino A1 - Quadríceps, Adutores e Panturrilha", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Quadríceps", "Adutores / Abdutores", "Panturrilha", "Panturrilha", "Abdômen"
                }, volumeDia)));
                dias.Add(("Treino B1 - Costas, Ombros e Bíceps", AjustarVolume(new[]
                {
                    "Costas", "Costas", "Ombros", "Ombros", "Bíceps", "Trapézio", "Abdômen"
                }, volumeDia)));
                dias.Add(("Treino C1 - Glúteos, Posterior e Lombar", AjustarVolume(new[]
                {
                    "Glúteos", "Glúteos", "Posterior de Coxa", "Posterior de Coxa", "Adutores / Abdutores", "Lombar", "Abdômen"
                }, volumeDia)));
                dias.Add(("Treino A2 - Quadríceps, Glúteos e Core", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Glúteos", "Adutores / Abdutores", "Panturrilha", "Abdômen", "Abdômen"
                }, volumeDia)));
                dias.Add(("Treino B2 - Peito, Ombros e Tríceps", AjustarVolume(new[]
                {
                    "Peito", "Peito", "Costas", "Ombros", "Tríceps", "Tríceps", "Abdômen"
                }, volumeDia)));
                dias.Add(("Treino C2 - Posterior, Glúteos e Panturrilha", AjustarVolume(new[]
                {
                    "Posterior de Coxa", "Posterior de Coxa", "Glúteos", "Glúteos", "Panturrilha", "Lombar", "Abdômen"
                }, volumeDia)));
            }
            else
            {
                dias.Add(("Treino A1 - Peito, Ombros e Tríceps", AjustarVolume(new[]
                {
                    "Peito", "Peito", "Peito", "Ombros", "Ombros", "Tríceps", "Tríceps"
                }, volumeDia)));
                dias.Add(("Treino B1 - Costas, Trapézio e Bíceps", AjustarVolume(new[]
                {
                    "Costas", "Costas", "Costas", "Trapézio", "Bíceps", "Bíceps", "Antebraço"
                }, volumeDia)));
                dias.Add(("Treino C1 - Pernas Completo (Foco Quadríceps)", AjustarVolume(new[]
                {
                    "Quadríceps", "Quadríceps", "Posterior de Coxa", "Glúteos", "Adutores / Abdutores", "Panturrilha", "Abdômen"
                }, volumeDia)));
                dias.Add(("Treino A2 - Ombros, Peito e Tríceps", AjustarVolume(new[]
                {
                    "Peito", "Peito", "Ombros", "Ombros", "Tríceps", "Tríceps", "Abdômen"
                }, volumeDia)));
                dias.Add(("Treino B2 - Costas, Bíceps e Lombar", AjustarVolume(new[]
                {
                    "Costas", "Costas", "Trapézio", "Bíceps", "Bíceps", "Antebraço", "Lombar"
                }, volumeDia)));
                dias.Add(("Treino C2 - Pernas Completo (Foco Posterior e Glúteos)", AjustarVolume(new[]
                {
                    "Posterior de Coxa", "Posterior de Coxa", "Quadríceps", "Glúteos", "Adutores / Abdutores", "Panturrilha", "Abdômen"
                }, volumeDia)));
            }
        }

        return (nomeDivisao, dias);
    }

    private static List<string> AjustarVolume(string[] sequenciaIdeal, int quantidadeDesejada)
    {
        return sequenciaIdeal.Take(Math.Clamp(quantidadeDesejada, 4, sequenciaIdeal.Length)).ToList();
    }

    private static (Exercicio? exercicio, Maquina? maquina) SelecionarMovimentoParaGrupo(
        GrupoMuscular grupoAlvo,
        bool preferirMaquina,
        List<Exercicio> exercicios,
        List<Maquina> maquinas,
        HashSet<int> usadosExercicio,
        HashSet<int> usadosMaquina)
    {
        // 1º tenta buscar exatamente pelo GrupoMuscularId (ex: "Peito")
        var maquinasDoGrupo = maquinas.Where(m => m.GrupoMuscularId == grupoAlvo.Id).ToList();
        var exerciciosDoGrupo = exercicios.Where(e => e.GrupoMuscularId == grupoAlvo.Id).ToList();

        var resultadoDireto = EscolherEntreMaquinaOuExercicio(
            preferirMaquina,
            exerciciosDoGrupo,
            maquinasDoGrupo,
            usadosExercicio,
            usadosMaquina);

        if (resultadoDireto.exercicio is not null || resultadoDireto.maquina is not null)
        {
            return resultadoDireto;
        }

        // 2º Caso o Personal tenha removido todos os itens daquele músculo específico,
        // busca outro item ainda não usado da mesma Região Corporal (ex: outro músculo de "Membros Superiores")
        var maquinasDaRegiao = maquinas
            .Where(m => m.GrupoMuscular?.Regiao == grupoAlvo.Regiao && !usadosMaquina.Contains(m.Id))
            .ToList();

        var exerciciosDaRegiao = exercicios
            .Where(e => e.GrupoMuscular?.Regiao == grupoAlvo.Regiao && !usadosExercicio.Contains(e.Id))
            .ToList();

        return EscolherEntreMaquinaOuExercicio(
            preferirMaquina,
            exerciciosDaRegiao,
            maquinasDaRegiao,
            usadosExercicio,
            usadosMaquina);
    }

    private static (Exercicio? exercicio, Maquina? maquina) EscolherEntreMaquinaOuExercicio(
        bool preferirMaquina,
        List<Exercicio> opcoesExercicios,
        List<Maquina> opcoesMaquinas,
        HashSet<int> usadosExercicio,
        HashSet<int> usadosMaquina)
    {
        if (preferirMaquina)
        {
            var maq = EscolherItemSemRepetir(opcoesMaquinas, usadosMaquina, m => m.Id);
            if (maq is not null)
            {
                return (null, maq);
            }

            var ex = EscolherItemSemRepetir(opcoesExercicios, usadosExercicio, e => e.Id);
            return (ex, null);
        }
        else
        {
            var ex = EscolherItemSemRepetir(opcoesExercicios, usadosExercicio, e => e.Id);
            if (ex is not null)
            {
                return (ex, null);
            }

            var maq = EscolherItemSemRepetir(opcoesMaquinas, usadosMaquina, m => m.Id);
            return (null, maq);
        }
    }

    private static T? EscolherItemSemRepetir<T>(List<T> opcoes, HashSet<int> jaUsados, Func<T, int> seletorId) where T : class
    {
        if (opcoes.Count == 0)
        {
            return null;
        }

        var aindaNaoUsados = opcoes.Where(o => !jaUsados.Contains(seletorId(o))).ToList();
        var candidato = aindaNaoUsados.Count > 0
            ? aindaNaoUsados[Random.Shared.Next(aindaNaoUsados.Count)]
            : opcoes[Random.Shared.Next(opcoes.Count)];

        jaUsados.Add(seletorId(candidato));
        return candidato;
    }

    // Monta o objeto completo do plano já com dados do Aluno, Dica do Objetivo, Status e Dias agrupados/ordenados para o Front-end
    private static object FormatarPlanoResposta(Treino treino)
    {
        var itensOrdenados = (treino.Itens ?? new List<ItemTreino>())
            .OrderBy(i => i.Id)
            .ToList();

        var dias = itensOrdenados
            .GroupBy(i => string.IsNullOrWhiteSpace(i.DivisaoTreino) ? "Treino" : i.DivisaoTreino)
            .Select(grupoDia => new
            {
                nome = grupoDia.Key,
                tempoAerobico = grupoDia.FirstOrDefault()?.TempoAerobico ?? string.Empty,
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
            objetivo = treino.Aluno?.Objetivo ?? "-",
            experiencia = treino.Aluno?.Experiencia ?? "-",
            frequencia = treino.Aluno?.FrequenciaSemanal ?? dias.Count,
            nomePlano = treino.NomePlano,
            divisao = treino.Divisao,
            tempoAerobico = treino.TempoAerobico,
            dicaObjetivoId = treino.DicaObjetivoId,
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
