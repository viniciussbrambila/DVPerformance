using System.Text.Json;
using DVPerformance.Api.Data;
using DVPerformance.Api.Endpoints;
using DVPerformance.Api.Models;

namespace DVPerformance.Api.Tests;

internal static class AdutoresAbdutoresTests
{
    public static void Executar(Action<string, Action> testar, Action<bool, string> exigir)
    {
        var grupos = DivisoesTreinoTests.GruposPadrao();
        var exercicios = CatalogoPadraoService.ExerciciosPadrao.Select((item, i) => new Exercicio {
            Id = i + 1, Nome = item.Nome, GrupoMuscularId = item.GrupoMuscularId,
            GrupoMuscular = grupos.Single(g => g.Id == item.GrupoMuscularId) }).ToList();
        var maquinas = CatalogoPadraoService.MaquinasPadrao.Select((item, i) => new Maquina {
            Id = i + 1, Nome = item.Nome, GrupoMuscularId = item.GrupoMuscularId,
            GrupoMuscular = grupos.Single(g => g.Id == item.GrupoMuscularId) }).ToList();

        string Grupo(ItemTreino item) => item.ExercicioId is int id
            ? exercicios.Single(e => e.Id == id).Grupo : maquinas.Single(m => m.Id == item.MaquinaId).Grupo;
        string Foco(ItemTreino item) => item.ExercicioId is int id
            ? FocosMusculares.Resolver(Grupo(item), exercicios.Single(e => e.Id == id).Nome,
                exercicios.Single(e => e.Id == id).FocoMuscular)
            : FocosMusculares.Resolver(Grupo(item), maquinas.Single(m => m.Id == item.MaquinaId).Nome,
                maquinas.Single(m => m.Id == item.MaquinaId).FocoMuscular);
        Treino Gerar(DivisaoTreinoConfiguracao config, string nivel, List<Exercicio>? livres = null, List<Maquina>? equipamentos = null) =>
            TreinoEndpoints.GerarTreinoPersonalizado(new Aluno {
                Id = 1, Nome = "Teste", Sexo = "M", Experiencia = nivel, FrequenciaSemanal = config.Dias,
                DataDeNascimento = Aluno.ObterDataAtual().AddYears(-30), Objetivo = "Hipertrofia"
            }, 1, 1, 2, grupos, livres ?? exercicios, equipamentos ?? maquinas, config);

        testar("todas as divisões respeitam 6/7/8 e incluem o foco correto junto de quadríceps e glúteos", () =>
        {
            foreach (var config in DivisoesTreino.Todas)
            foreach (var (nivel, meta) in new[] { ("Iniciante", 6), ("Intermediário", 7), ("Avançado", 8) })
            for (int tentativa = 0; tentativa < 20; tentativa++)
            {
                exigir(TreinoEndpoints.ValidarCatalogoDivisao(config, grupos, exercicios, maquinas).Count == 0,
                    "O catálogo padrão precisa atender às novas exigências.");
                var treino = Gerar(config, nivel);
                foreach (var sessao in config.Sessoes)
                {
                    var itens = treino.Itens.Where(i => i.DivisaoTreino == sessao.Titulo).ToArray();
                    exigir(itens.Length <= meta, $"{config.Id} / {sessao.Titulo} excedeu {meta}.");
                    if (nivel == "Iniciante")
                        exigir(itens.Length == 6, $"{config.Id} / {sessao.Titulo}: catálogo padrão deve completar seis.");
                    var focos = itens.Where(i => Grupo(i) == "Adutores / Abdutores").Select(Foco).ToArray();
                    exigir(focos.Count(f => f == "aducao") == (sessao.Grupos.Contains("Quadríceps") ? 1 : 0),
                        "Exatamente um adutor no dia de quadríceps.");
                    exigir(focos.Count(f => f == "abducao") == (sessao.Grupos.Contains("Glúteos") ? 1 : 0),
                        "Exatamente um abdutor no dia de glúteos.");
                    exigir(focos.All(f => f is "aducao" or "abducao"),
                        "Não substituir um movimento específico por um foco geral.");
                    exigir(itens.Select(i => (i.ExercicioId, i.MaquinaId)).Distinct().Count() == itens.Length,
                        "Não repetir o mesmo cadastro para completar a meta.");
                }
            }
        });

        testar("Full Body alterna os grupos e preserva cobertura semanal dentro das metas", () =>
        {
            var config = DivisoesTreino.Obter("full-body-ab", 2)!;
            foreach (var (nivel, meta) in new[] { ("Iniciante", 6), ("Intermediário", 7), ("Avançado", 8) })
            {
                var treino = Gerar(config, nivel);
                var dias = treino.Itens.GroupBy(i => i.DivisaoTreino).ToArray();
                exigir(dias.Length == 2 && dias.All(d => d.Count() == meta), "Respeitar a meta em A e B.");
                exigir(treino.Itens.Select(Grupo).ToHashSet().SetEquals([
                    "Peito", "Costas", "Ombros", "Quadríceps", "Posterior de Coxa",
                    "Glúteos", "Panturrilha", "Adutores / Abdutores"]),
                    "A alternância não pode eliminar um grupo da semana.");
                exigir(!dias[0].Select(Grupo).ToHashSet().SetEquals(dias[1].Select(Grupo)),
                    "A e B devem alternar grupos.");
                exigir(dias.All(d => d.Any(i => Grupo(i) is "Peito" or "Costas")
                    && d.Any(i => Grupo(i) is "Quadríceps" or "Glúteos")),
                    "As duas sessões devem incluir superiores e inferiores.");
            }
        });

        testar("catálogo curto permite intermediário e avançado abaixo da meta sem exceder limites", () =>
        {
            var config = DivisoesTreino.Obter("bro-split-5", 5)!;
            foreach (var nivel in new[] { "Intermediário", "Avançado" })
            {
                var treino = Gerar(config, nivel);
                var peito = treino.Itens.Where(i => Grupo(i) == "Peito").ToArray();
                exigir(peito.Length == 6 && peito.GroupBy(Foco).All(g => g.Count() <= 2),
                    "Peito com três focos permite seis movimentos, mesmo quando a meta é sete ou oito.");
            }
        });

        testar("validação distingue falta de adução e abdução, inclusive com o grupo presente", () =>
        {
            var config = DivisoesTreino.Obter("abcde", 5)!;
            foreach (var foco in new[] { "aducao", "abducao" })
            {
                var livres = exercicios.Where(e => e.Grupo != "Adutores / Abdutores"
                    || FocosMusculares.Resolver(e.Grupo, e.Nome, e.FocoMuscular) != foco).ToList();
                var equipamentos = maquinas.Where(m => m.Grupo != "Adutores / Abdutores"
                    || FocosMusculares.Resolver(m.Grupo, m.Nome, m.FocoMuscular) != foco).ToList();
                var erros = TreinoEndpoints.ValidarCatalogoDivisao(config, grupos, livres, equipamentos);
                var dia = foco == "aducao" ? "Treino C" : "Treino E";
                var nome = foco == "aducao" ? "adutores (adução)" : "abdutores (abdução)";
                exigir(erros.Count == 1 && erros[0].Contains(dia) && erros[0].Contains(nome),
                    "Avisar precisamente qual movimento e sessão precisam de cadastro.");
                var treino = Gerar(config, "Avançado", livres, equipamentos);
                exigir(!treino.Itens.Where(i => i.DivisaoTreino.StartsWith(dia))
                    .Any(i => Grupo(i) == "Adutores / Abdutores"),
                    "Mesmo em chamada direta, não substituir pelo foco oposto.");
            }
        });

        testar("movimentos sem classificação ou inativos não substituem os focos obrigatórios", () =>
        {
            var config = DivisoesTreino.Obter("ppl", 3)!;
            var livres = exercicios.Where(e => e.Grupo != "Adutores / Abdutores").ToList();
            var equipamentos = maquinas.Where(m => m.Grupo != "Adutores / Abdutores").ToList();
            var grupo = grupos.Single(g => g.Nome == "Adutores / Abdutores");
            livres.Add(new Exercicio { Id = 900, Nome = "Movimento sem classificação",
                GrupoMuscularId = grupo.Id, GrupoMuscular = grupo, FocoMuscular = "geral" });
            livres.Add(new Exercicio { Id = 901, Nome = "Cadeira Adutora",
                GrupoMuscularId = grupo.Id, GrupoMuscular = grupo, FocoMuscular = "aducao", Ativo = false });
            var erros = TreinoEndpoints.ValidarCatalogoDivisao(config, grupos, livres, equipamentos);
            exigir(erros.Count == 2 && erros.All(e => e.Contains("Treino C")),
                "Exigir os dois focos ativos no dia de pernas completas.");
        });

        testar("adição específica pode vir do outro catálogo e não depende de preferência por máquina", () =>
        {
            var config = DivisoesTreino.Obter("ppl", 3)!;
            var livres = exercicios.Where(e => e.Grupo != "Adutores / Abdutores"
                || FocosMusculares.Inferir(e.Grupo, e.Nome) == "aducao").ToList();
            var equipamentos = maquinas.Where(m => m.Grupo != "Adutores / Abdutores"
                || FocosMusculares.Inferir(m.Grupo, m.Nome) == "abducao").ToList();
            var treino = Gerar(config, "Iniciante", livres, equipamentos);
            var especificos = treino.Itens.Where(i => Grupo(i) == "Adutores / Abdutores").ToArray();
            exigir(especificos.Length == 2
                && especificos.Single(i => Foco(i) == "aducao").ExercicioId.HasValue
                && especificos.Single(i => Foco(i) == "abducao").MaquinaId.HasValue,
                "Usar um adutor livre e uma máquina abdutora quando forem as alternativas disponíveis.");
        });

        testar("opções da divisão mostram separadamente adutores e abdutores por sessão", () =>
        {
            var config = DivisoesTreino.Obter("abcde", 5)!;
            var resposta = JsonSerializer.SerializeToElement(config, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var sessoes = resposta.GetProperty("sessoes").EnumerateArray().ToArray();
            string[] Exibicao(int i) => sessoes[i].GetProperty("gruposExibicao").EnumerateArray()
                .Select(g => g.GetString()!).ToArray();
            exigir(Exibicao(2).Contains("Adutores") && !Exibicao(2).Contains("Abdutores"),
                "O dia de quadríceps deve apresentar adutores.");
            exigir(Exibicao(4).Contains("Abdutores") && !Exibicao(4).Contains("Adutores"),
                "O dia de glúteos deve apresentar abdutores.");
            var pernas = DivisoesTreino.Obter("ppl", 3)!.Sessoes[2];
            exigir(pernas.GruposExibicao.Contains("Adutores") && pernas.GruposExibicao.Contains("Abdutores"),
                "Pernas completas deve apresentar os dois movimentos.");
        });
    }
}
