using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class TreinoEndpoints
{
    public static void MapTreinoEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/treinos");

        // GET /treinos?usuarioId=1 -> Lista todos os treinos ativos (com os itens, exercicio/maquina e grupo muscular unificado)
        grupo.MapGet("/", async (int? usuarioId, int? alunoId, bool? incluirInativos, AppDbContext db) =>
        {
            var query = db.Treinos
                .Include(t => t.Itens.OrderBy(i => i.DivisaoTreino).ThenBy(i => i.OrdemExercicio))
                    .ThenInclude(i => i.Exercicio)
                        .ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens)
                    .ThenInclude(i => i.Maquina)
                        .ThenInclude(m => m!.GrupoMuscular)
                .AsQueryable();

            if (incluirInativos != true)
            {
                query = query.Where(t => t.Ativo);
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                query = query.Where(t => t.UsuarioId == usuarioId.Value || t.UsuarioId == null);
            }

            if (alunoId.HasValue && alunoId.Value > 0)
            {
                query = query.Where(t => t.AlunoId == alunoId.Value);
            }

            var lista = await query
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();

            return Results.Ok(lista);
        });

        // GET /treinos/{id} -> Busca uma ficha de treino específica pelo ID com todos os itens
        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do treino deve ser um número inteiro positivo." });
            }

            var treino = await db.Treinos
                .Include(t => t.Itens.OrderBy(i => i.DivisaoTreino).ThenBy(i => i.OrdemExercicio))
                    .ThenInclude(i => i.Exercicio)
                        .ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens)
                    .ThenInclude(i => i.Maquina)
                        .ThenInclude(m => m!.GrupoMuscular)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (treino is null)
            {
                return Results.NotFound(new { mensagem = "Ficha de treino não encontrada." });
            }

            return Results.Ok(treino);
        });

        // GET /treinos/aluno/{alunoId} -> Lista o histórico de fichas de treino de um aluno específico
        grupo.MapGet("/aluno/{alunoId:int}", async (int alunoId, AppDbContext db) =>
        {
            if (alunoId <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do aluno deve ser um número inteiro positivo." });
            }

            var treinos = await db.Treinos
                .Where(t => t.AlunoId == alunoId && t.Ativo)
                .Include(t => t.Itens.OrderBy(i => i.DivisaoTreino).ThenBy(i => i.OrdemExercicio))
                    .ThenInclude(i => i.Exercicio)
                        .ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens)
                    .ThenInclude(i => i.Maquina)
                        .ThenInclude(m => m!.GrupoMuscular)
                .OrderByDescending(t => t.DataCriacao)
                .ToListAsync();

            return Results.Ok(treinos);
        });

        // POST /treinos/gerar/{alunoId} -> Motor Inteligente DV Performance: gera e salva a ficha usando os exercícios e máquinas ativos da conta do Personal
        grupo.MapPost("/gerar/{alunoId:int}", async (int alunoId, int? usuarioId, AppDbContext db) =>
        {
            if (alunoId <= 0)
            {
                return Results.BadRequest(new { mensagem = "Informe um ID de aluno válido para gerar o treino." });
            }

            var aluno = await db.Alunos.FirstOrDefaultAsync(a => a.Id == alunoId && a.Ativo);
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

            var gruposMusculares = await db.GruposMusculares.ToListAsync();

            var exerciciosDisponiveis = await db.Exercicios
                .Include(e => e.GrupoMuscular)
                .Where(e => e.Ativo && (donoId == null || e.UsuarioId == donoId || e.UsuarioId == null))
                .ToListAsync();

            var maquinasDisponiveis = await db.Maquinas
                .Include(m => m.GrupoMuscular)
                .Where(m => m.Ativo && (donoId == null || m.UsuarioId == donoId || m.UsuarioId == null))
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

            // Desativa treinos anteriores ativos desse aluno para que o novo seja o principal (mantendo histórico)
            var treinosAnteriores = await db.Treinos
                .Where(t => t.AlunoId == aluno.Id && t.Ativo)
                .ToListAsync();

            foreach (var antigo in treinosAnteriores)
            {
                antigo.Ativo = false;
            }

            // Executa o Motor de Prescrição Inteligente usando a tabela unificada grupos_musculares e sua OrdemPrioridade (1 a 4)
            var novoTreino = GerarTreinoPersonalizado(
                aluno,
                donoId,
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

            // Recarrega o treino com as navegações completas (Exercicio, Maquina e GrupoMuscular) para retornar ao cliente
            var treinoSalvo = await db.Treinos
                .Include(t => t.Itens.OrderBy(i => i.DivisaoTreino).ThenBy(i => i.OrdemExercicio))
                    .ThenInclude(i => i.Exercicio)
                        .ThenInclude(e => e!.GrupoMuscular)
                .Include(t => t.Itens)
                    .ThenInclude(i => i.Maquina)
                        .ThenInclude(m => m!.GrupoMuscular)
                .FirstAsync(t => t.Id == novoTreino.Id);

            return Results.Created($"/treinos/{treinoSalvo.Id}", treinoSalvo);
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

            if (dadosAtualizados.Series <= 0 || dadosAtualizados.Series > 10)
            {
                return Results.BadRequest(new { mensagem = "O número de séries deve ser entre 1 e 10." });
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

            if (!string.IsNullOrWhiteSpace(dadosAtualizados.Repeticao))
            {
                item.Repeticao = dadosAtualizados.Repeticao.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dadosAtualizados.TempoDescanso))
            {
                item.TempoDescanso = dadosAtualizados.TempoDescanso.Trim();
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

    // =========================================================================
    // MOTOR DE PRESCRIÇÃO INTELIGENTE DV PERFORMANCE
    // =========================================================================
    private static Treino GerarTreinoPersonalizado(
        Aluno aluno,
        int? usuarioId,
        List<GrupoMuscular> gruposMusculares,
        List<Exercicio> exercicios,
        List<Maquina> maquinas)
    {
        var mapaGrupos = gruposMusculares.ToDictionary(g => g.Nome, g => g);

        // 1. Parâmetros base de Séries, Repetições, Descanso e Aeróbico conforme OBJETIVO, EXPERIÊNCIA e IDADE
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

        if (aluno.Idade >= 60)
        {
            proporcaoMaquinas = Math.Max(proporcaoMaquinas, 0.80);
        }

        // 3. Monta a Matriz de Divisão Semanal (Dias e grupos musculares do dia)
        var (nomeDivisao, diasPlanejados) = ObterDivisaoSemanal(aluno);

        var sufixoSexo = aluno.Sexo == "F" ? "Feminino" : "Masculino";
        var nomePlano = $"Protocolo {aluno.Objetivo} {sufixoSexo} ({aluno.Experiencia}) - {nomeDivisao}";

        var treino = new Treino
        {
            AlunoId = aluno.Id,
            UsuarioId = usuarioId,
            NomePlano = nomePlano,
            Divisao = nomeDivisao,
            TempoAerobico = tempoAerobicoGeral,
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

            var selecionadosDoDia = new List<(int prioridade, string nomeGrupo, Exercicio? ex, Maquina? maq)>();
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

                string nomeGrupoReal = exercicioEscolhido?.GrupoMuscular?.Nome
                    ?? maquinaEscolhida?.GrupoMuscular?.Nome
                    ?? grupoMuscular.Nome;

                selecionadosDoDia.Add((prioridadeReal, nomeGrupoReal, exercicioEscolhido, maquinaEscolhida));
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

                string repeticaoItem = repeticoes;
                if (itemSelecionado.nomeGrupo is "Abdômen" or "Panturrilha" or "Lombar" && aluno.Objetivo == "Força")
                {
                    repeticaoItem = "12-15";
                }

                treino.Itens.Add(new ItemTreino
                {
                    DivisaoTreino = tituloDia,
                    OrdemExercicio = ordem++,
                    ExercicioId = itemSelecionado.ex?.Id,
                    MaquinaId = itemSelecionado.maq?.Id,
                    Series = seriesItem,
                    Repeticao = repeticaoItem,
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
        int series = aluno.Experiencia switch
        {
            "Iniciante" => 3,
            "Intermediário" => 3,
            "Avançado" => 4,
            _ => 3
        };

        string repeticoes;
        int descansoSegundos;
        string aerobicoGeral;
        string aerobicoDia;

        switch (aluno.Objetivo)
        {
            case "Emagrecimento":
                repeticoes = "12-15";
                descansoSegundos = aluno.Experiencia == "Avançado" ? 40 : 45;
                aerobicoGeral = "25 a 30 min de cardio moderado/HIIT pós-treino em todas as sessões";
                aerobicoDia = "25 min Esteira/Bike ou Escada (pós-treino)";
                break;

            case "Força":
                series = aluno.Experiencia == "Iniciante" ? 3 : 4;
                repeticoes = aluno.Experiencia == "Iniciante" ? "6-8" : "4-6";
                descansoSegundos = 120;
                aerobicoGeral = "10 min de mobilidade e cardio leve apenas para aquecimento";
                aerobicoDia = "10 min Bike/Esteira leve (aquecimento)";
                break;

            case "Resistência":
                repeticoes = "15-20";
                descansoSegundos = 40;
                aerobicoGeral = "20 min de cardio contínuo (zona aeróbica) pós-treino";
                aerobicoDia = "20 min Esteira/Elíptico contínuo";
                break;

            case "Condicionamento":
                repeticoes = "12-15";
                descansoSegundos = 50;
                aerobicoGeral = "20 min de cardio intervalado pós-treino";
                aerobicoDia = "20 min Cardio intervalado (Esteira/Remo)";
                break;

            case "Hipertrofia":
            default:
                if (aluno.Experiencia == "Intermediário") series = 4;
                repeticoes = "8-12";
                descansoSegundos = aluno.Experiencia == "Avançado" ? 75 : 60;
                aerobicoGeral = "15 min de cardio moderado pós-treino (sem comprometer a recuperação muscular)";
                aerobicoDia = "15 min Caminhada inclinada / Bike leve";
                break;
        }

        if (aluno.Idade >= 60)
        {
            descansoSegundos += 15;
            series = Math.Min(series, 3);
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

        if (aluno.Idade >= 60)
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
                nomeDivisao = aluno.Objetivo is "Emagrecimento" or "Condicionamento"
                    ? "ABA Metabólico Feminino (3x)"
                    : "ABA Ênfase Inferiores (3x)";

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
}
