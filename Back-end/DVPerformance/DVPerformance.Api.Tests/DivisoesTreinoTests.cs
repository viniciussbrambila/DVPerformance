using System.Reflection;
using System.Text.Json;
using DVPerformance.Api.Data;
using DVPerformance.Api.Endpoints;
using DVPerformance.Api.Models;

namespace DVPerformance.Api.Tests;

internal static class DivisoesTreinoTests
{
    public static void Executar(Action<string, Action> testar, Action<bool, string> exigir)
    {
        string[] full = ["Peito", "Costas", "Ombros", "Quadríceps", "Posterior de Coxa", "Glúteos", "Panturrilha"];
        string[] upper = ["Peito", "Costas", "Ombros", "Bíceps", "Tríceps"];
        string[] lower = ["Quadríceps", "Posterior de Coxa", "Glúteos", "Panturrilha"];
        string[] push = ["Peito", "Ombros", "Tríceps"];
        string[] pull = ["Costas", "Trapézio", "Bíceps"];
        var esperados = new Dictionary<string, string[][]>
        {
            ["full-body-ab"] = [full, full],
            ["ppl"] = [push, pull, lower],
            ["upper-lower"] = [upper, lower, upper, lower],
            ["abcd"] = [["Peito", "Tríceps"], ["Costas", "Bíceps"], lower, ["Ombros", "Abdômen"]],
            ["abcde"] = [["Peito", "Tríceps"], ["Costas", "Bíceps"], ["Quadríceps", "Panturrilha"],
                ["Ombros", "Trapézio", "Abdômen"], ["Posterior de Coxa", "Glúteos"]],
            ["ulppl"] = [["Peito", "Costas", "Ombros", "Trapézio", "Bíceps", "Tríceps"], lower, push, pull, lower],
            ["bro-split-5"] = [["Peito"], ["Costas"], ["Ombros", "Trapézio"], lower, ["Bíceps", "Tríceps", "Antebraço"]],
            ["ppl-2x"] = [push, pull, lower, push, pull, lower],
            ["arnold"] = [["Peito", "Costas"], ["Ombros", "Bíceps", "Tríceps"], lower,
                ["Peito", "Costas"], ["Ombros", "Bíceps", "Tríceps"], lower],
            ["bro-split-6"] = [["Costas"], ["Peito", "Abdômen"], ["Quadríceps", "Panturrilha"],
                ["Ombros", "Trapézio"], ["Posterior de Coxa", "Glúteos"], ["Bíceps", "Tríceps"]]
        };

        testar("opções por frequência e distribuição correspondem às tabelas do personal", () =>
        {
            var idsPorDias = new Dictionary<int, string[]>
            {
                [2] = ["full-body-ab"], [3] = ["ppl"], [4] = ["upper-lower", "abcd"],
                [5] = ["abcde", "ulppl", "bro-split-5"], [6] = ["ppl-2x", "arnold", "bro-split-6"]
            };
            exigir(DivisoesTreino.Todas.Count == esperados.Count, "Oferecer exatamente as dez divisões acordadas.");
            foreach (var (dias, ids) in idsPorDias)
                exigir(DivisoesTreino.ObterDisponiveis(dias).Select(d => d.Id).SequenceEqual(ids),
                    $"Opções incorretas para {dias} dias.");
            foreach (var config in DivisoesTreino.Todas)
            {
                var gruposEsperados = esperados[config.Id];
                exigir(config.Sessoes.Count == config.Dias && config.Dias == gruposEsperados.Length,
                    "Quantidade de sessões deve corresponder à frequência.");
                for (int i = 0; i < gruposEsperados.Length; i++)
                {
                    exigir(config.Sessoes[i].Grupos.ToHashSet().SetEquals(gruposEsperados[i]),
                        $"{config.Id}, dia {i}: grupos diferentes da tabela.");
                    exigir(config.Sessoes[i].Titulo.StartsWith($"Treino {(char)('A' + i)} - "),
                        "Preservar os rótulos e a ordem A/B/C/D/E/F.");
                }
                exigir(DivisoesTreino.Obter(config.Id, config.Dias)?.Id == config.Id, "Resolver ID escolhido.");
                exigir(DivisoesTreino.Obter(config.Id, config.Dias == 6 ? 5 : config.Dias + 1) is null,
                    "Rejeitar divisão incompatível com os dias do aluno.");
            }
            exigir(DivisoesTreino.Obter(null, 4) is null
                && DivisoesTreino.Obter("", 4) is null
                && DivisoesTreino.Obter("inexistente", 4) is null
                && DivisoesTreino.Obter("phul", 4) is null, "Exigir uma opção válida; PHUL não foi incluído.");
            exigir(DivisoesTreino.ObterDisponiveis(1).Count == 0
                && DivisoesTreino.ObterDisponiveis(7).Count == 0, "Não assumir frequência diferente da informada.");
        });

        testar("todas as divisões cobrem somente os músculos definidos e mantêm prescrição por nível", () =>
        {
            var (grupos, exercicios, maquinas) = Catalogo();
            foreach (var config in DivisoesTreino.Todas)
            foreach (var sexo in new[] { "M", "F" })
            foreach (var nivel in new[] { "Iniciante", "Intermediário", "Avançado" })
            {
                var aluno = AlunoPara(config.Dias, sexo, nivel);
                exigir(TreinoEndpoints.ValidarCatalogoDivisao(config, grupos, exercicios, maquinas).Count == 0,
                    "Catálogo completo deve atender toda a divisão.");
                var treino = Gerar(aluno, config, grupos, exercicios, maquinas);
                exigir(treino.Divisao == config.Nome && treino.NomePlano.Contains(config.Nome),
                    "Preservar a metodologia escolhida na ficha.");
                exigir(treino.Itens.Select(i => i.DivisaoTreino).Distinct().Count() == config.Dias,
                    "Gerar todos os dias.");
                for (int dia = 0; dia < config.Dias; dia++)
                {
                    var itens = treino.Itens.Where(i => i.DivisaoTreino == config.Sessoes[dia].Titulo).ToArray();
                    var grupoIds = itens.Select(i => Grupo(i, exercicios, maquinas)).ToArray();
                    var nomes = grupoIds.Select(id => grupos.Single(g => g.Id == id).Nome).ToHashSet();
                    exigir(nomes.SetEquals(esperados[config.Id][dia]), "Cobrir todos os grupos sem inserir músculos extras.");
                    exigir(itens.Select(Identidade).Distinct().Count() == itens.Length,
                        "Nunca repetir o mesmo exercício ou máquina no mesmo dia.");
                    exigir(itens.All(i => i.ExercicioId.HasValue ^ i.MaquinaId.HasValue), "Cada item tem exatamente um vínculo.");
                    exigir(itens.Select(i => i.OrdemExercicio).SequenceEqual(Enumerable.Range(1, itens.Length)),
                        "Manter ordem contínua por dia.");
                    var prioridades = grupoIds.Select(id => grupos.Single(g => g.Id == id).OrdemPrioridade).ToArray();
                    exigir(prioridades.SequenceEqual(prioridades.Order()), "Ordenar músculos grandes antes dos auxiliares.");
                    var volume = nivel == "Iniciante" ? 5 : nivel == "Intermediário" ? 6 : 7;
                    exigir(itens.Length == Math.Max(volume, esperados[config.Id][dia].Length),
                        "Preservar a meta de volume sem cortar grupos obrigatórios.");
                    foreach (var item in itens)
                    {
                        var prioridade = grupos.Single(g => g.Id == Grupo(item, exercicios, maquinas)).OrdemPrioridade;
                        var series = nivel == "Iniciante" ? 3 : nivel == "Intermediário" && prioridade >= 3 ? 3 : 4;
                        exigir(item.Series == series, "Preservar séries por experiência e grupo.");
                        exigir(item.Repeticao == (nivel == "Iniciante" ? "12-15" : nivel == "Intermediário" ? "10-12" : "8-12"),
                            "Preservar faixa de repetições.");
                        exigir(item.TempoDescanso == (nivel == "Avançado" ? "75s" : "60s"), "Preservar descanso.");
                        exigir(item.TempoAerobico == (nivel == "Iniciante" ? 15 : 20), "Preservar aeróbico.");
                    }
                }
            }
        });

        testar("sessões equivalentes usam opções diferentes quando o catálogo permite", () =>
        {
            var (grupos, exercicios, maquinas) = Catalogo();
            for (int tentativa = 0; tentativa < 10; tentativa++)
            foreach (var config in DivisoesTreino.Todas)
            {
                var treino = Gerar(AlunoPara(config.Dias, "M", "Avançado"), config, grupos, exercicios, maquinas);
                exigir(treino.Itens.Select(Identidade).Distinct().Count() == treino.Itens.Count,
                    "Priorizar movimentos inéditos na semana antes de repetir entre sessões.");
            }
        });

        testar("catálogo escasso permite reuso entre dias sem duplicar dentro da sessão", () =>
        {
            var (grupos, exercicios, maquinas) = Catalogo(1);
            var bro = DivisoesTreino.Obter("bro-split-5", 5)!;
            var treino = Gerar(AlunoPara(5, "M", "Avançado"), bro, grupos, exercicios, maquinas);
            var peito = treino.Itens.Where(i => i.DivisaoTreino == bro.Sessoes[0].Titulo).ToArray();
            exigir(peito.Length == 2 && peito.Select(Identidade).Distinct().Count() == 2,
                "Exercício e máquina com mesmo ID são diferentes; não duplicar nenhum deles para completar volume.");
            foreach (var dia in treino.Itens.GroupBy(i => i.DivisaoTreino))
                exigir(dia.Select(Identidade).Distinct().Count() == dia.Count(), "Catálogo curto não permite duplicação no dia.");

            var full = DivisoesTreino.Obter("full-body-ab", 2)!;
            var variado = Gerar(AlunoPara(2), full, grupos, exercicios, maquinas);
            exigir(variado.Itens.Select(Identidade).Distinct().Count() == variado.Itens.Count,
                "Escolher opção inédita do outro catálogo antes de repetir o equipamento preferido.");
            var reduzido = Gerar(AlunoPara(2), full, grupos, exercicios, new());
            exigir(reduzido.Itens.Count == 14 && reduzido.Itens.Select(Identidade).Distinct().Count() == 7,
                "Após esgotar alternativas, reutilizar entre sessões e manter ambos os dias.");
        });

        testar("grupo ausente ou inativo impede cobertura sem substituir por outro da região", () =>
        {
            var (grupos, exercicios, maquinas) = Catalogo();
            var peitoId = grupos.Single(g => g.Nome == "Peito").Id;
            foreach (var ex in exercicios.Where(e => e.GrupoMuscularId == peitoId)) ex.Ativo = false;
            foreach (var maq in maquinas.Where(m => m.GrupoMuscularId == peitoId)) maq.Ativo = false;
            var config = DivisoesTreino.Obter("bro-split-5", 5)!;
            var erros = TreinoEndpoints.ValidarCatalogoDivisao(config, grupos, exercicios, maquinas);
            exigir(erros.Count == 1 && erros[0].Contains("Peito"), "Informar o grupo indisponível antes de salvar.");
            var treino = Gerar(AlunoPara(5), config, grupos, exercicios, maquinas);
            exigir(!treino.Itens.Any(i => i.DivisaoTreino == config.Sessoes[0].Titulo),
                "Músculos de costas ou ombros não podem ocupar uma vaga de peito.");
            exigir(TreinoEndpoints.ValidarCatalogoDivisao(config,
                grupos.Where(g => g.Id != peitoId).ToList(), exercicios, maquinas).Count == 1,
                "Grupo mestre ausente também deve produzir erro.");
        });

        testar("novas divisões preservam limites fisiológicos de alunos idosos", () =>
        {
            var (grupos, exercicios, maquinas) = Catalogo();
            foreach (var config in DivisoesTreino.Todas.Where(d => d.Dias <= 5))
            {
                var aluno = AlunoPara(config.Dias, "F", "Intermediário");
                aluno.DataDeNascimento = Aluno.ObterDataAtual().AddYears(-70);
                var treino = Gerar(aluno, config, grupos, exercicios, maquinas);
                exigir(treino.Itens.All(i => i.Series == 3 && i.TempoDescanso == "75s"
                    && i.TempoAerobico == 15 && i.Repeticao == "10-12"), "Manter ajustes de idade.");
            }
        });

        testar("resposta da ficha mantém divisão escolhida e ordem de todas as sessões", () =>
        {
            var (grupos, exercicios, maquinas) = Catalogo();
            foreach (var config in DivisoesTreino.Todas)
            {
                var treino = Gerar(AlunoPara(config.Dias), config, grupos, exercicios, maquinas);
                var formatar = typeof(TreinoEndpoints).GetMethod("FormatarPlanoResposta", BindingFlags.NonPublic | BindingFlags.Static)!;
                var resposta = JsonSerializer.SerializeToElement(formatar.Invoke(null, [treino]));
                exigir(resposta.GetProperty("divisao").GetString() == config.Nome, "Expor o nome persistido.");
                exigir(resposta.GetProperty("dias").EnumerateArray().Select(d => d.GetProperty("nome").GetString())
                    .SequenceEqual(config.Sessoes.Select(s => s.Titulo)), "Ordenar a ficha de A até F.");
            }
        });
    }

    private static Aluno AlunoPara(int dias, string sexo = "M", string nivel = "Iniciante") =>
        new() { Id = 1, Nome = "Aluno", Sexo = sexo, Experiencia = nivel, FrequenciaSemanal = dias,
            DataDeNascimento = Aluno.ObterDataAtual().AddYears(-30), Objetivo = "Hipertrofia" };

    private static Treino Gerar(Aluno aluno, DivisaoTreinoConfiguracao config,
        List<GrupoMuscular> grupos, List<Exercicio> exercicios, List<Maquina> maquinas) =>
        TreinoEndpoints.GerarTreinoPersonalizado(aluno, 1, 1, 2, grupos, exercicios, maquinas, config);

    private static (string Tipo, int Id) Identidade(ItemTreino item) =>
        item.ExercicioId is int id ? ("E", id) : ("M", item.MaquinaId!.Value);

    private static int Grupo(ItemTreino item, List<Exercicio> exercicios, List<Maquina> maquinas) =>
        item.ExercicioId is int id ? exercicios.Single(e => e.Id == id).GrupoMuscularId
            : maquinas.Single(m => m.Id == item.MaquinaId).GrupoMuscularId;

    private static (List<GrupoMuscular>, List<Exercicio>, List<Maquina>) Catalogo(int porGrupo = 8)
    {
        string[] nomes = ["Peito", "Costas", "Quadríceps", "Posterior de Coxa", "Glúteos",
            "Ombros", "Trapézio", "Bíceps", "Tríceps", "Panturrilha", "Antebraço", "Abdômen"];
        var grupos = nomes.Select((nome, indice) => new GrupoMuscular
        {
            Id = indice + 1, Nome = nome,
            Regiao = nome is "Quadríceps" or "Posterior de Coxa" or "Glúteos" or "Panturrilha"
                ? "Membros Inferiores" : "Membros Superiores",
            OrdemPrioridade = indice < 5 ? 1 : indice < 7 ? 2 : indice < 10 ? 3 : 4
        }).ToList();
        var exercicios = grupos.SelectMany(g => Enumerable.Range(1, porGrupo).Select(i =>
            new Exercicio { Id = g.Id * 100 + i, Nome = $"{g.Nome} livre {i}",
                GrupoMuscularId = g.Id, GrupoMuscular = g, UsuarioId = 1 })).ToList();
        var maquinas = grupos.SelectMany(g => Enumerable.Range(1, porGrupo).Select(i =>
            new Maquina { Id = g.Id * 100 + i, Nome = $"{g.Nome} máquina {i}",
                GrupoMuscularId = g.Id, GrupoMuscular = g, UsuarioId = 1 })).ToList();
        return (grupos, exercicios, maquinas);
    }
}
