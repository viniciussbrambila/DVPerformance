using System.Text.Json;
using DVPerformance.Api.Data;
using DVPerformance.Api.Endpoints;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DVPerformance.Api.Tests;

internal static class FocosMuscularesTests
{
    public static void Executar(Action<string, Action> testar, Action<bool, string> exigir)
    {
        var grupos = DivisoesTreinoTests.GruposPadrao();
        var configuracao = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=validacao_sem_conexao;Username=teste;Password=teste").Options;
        using var db = new AppDbContext(configuracao);

        testar("opções de foco cobrem os 14 grupos e rejeitam classificações incompatíveis", () =>
        {
            exigir(FocosMusculares.Opcoes.Keys.ToHashSet().SetEquals(grupos.Select(g => g.Nome)),
                "Disponibilizar opções para todos os grupos, sem criar novos músculos.");
            foreach (var grupo in grupos)
            {
                var opcoes = FocosMusculares.Opcoes[grupo.Nome];
                exigir(opcoes.Select(o => o.Id).Distinct().Count() == opcoes.Length
                    && opcoes.All(o => !string.IsNullOrWhiteSpace(o.Nome)), "Opções devem ter chaves únicas e rótulos.");
                exigir(opcoes.All(o => FocosMusculares.Valido(grupo.Nome, o.Id))
                    && FocosMusculares.Valido(grupo.Nome, "geral"), "Aceitar as opções divulgadas ao cadastro.");
                exigir(!FocosMusculares.Valido(grupo.Nome, "inexistente")
                    && !FocosMusculares.Valido(grupo.Nome, null)
                    && !FocosMusculares.Valido(grupo.Nome, " "), "Não aceitar valores fora das opções.");
                exigir(FocosMusculares.Inferir(grupo.Id, null) == "geral"
                    && FocosMusculares.Inferir(grupo.Nome, "Movimento personalizado desconhecido") == "geral",
                    "Nome desconhecido não pode ganhar uma classificação inventada.");
            }
            exigir(!FocosMusculares.Valido("Peito", "braco_elevado")
                && !FocosMusculares.Valido("Grupo inexistente", "geral"), "Validar a combinação de grupo e foco.");
            exigir(FocosMusculares.Inferir(0, "Supino Inclinado") == "geral"
                && FocosMusculares.Inferir(999, "Supino Inclinado") == "geral", "IDs desconhecidos devem ser conservadores.");
        });

        testar("classificação agrupa variações de equipamento e diferencia ênfases reconhecidas", () =>
        {
            string[] inclinados = ["Supino Inclinado com Barra", "Supino Inclinado com Halteres",
                "Crucifixo Inclinado com Halteres", "Supino Inclinado Máquina"];
            exigir(inclinados.All(nome => FocosMusculares.Inferir("Peito", nome) == "superior"),
                "Barra, halter, crucifixo e máquina inclinados compartilham o foco superior.");
            exigir(FocosMusculares.Inferir("Peito", "Supino Reto com Barra") == "esternocostal"
                && FocosMusculares.Inferir("Peito", "Supino Declinado com Barra") == "inferior",
                "Diferenciar os padrões horizontal, inclinado e declinado.");
            string[] elevados = ["Tríceps Francês Bilateral com Halter", "Tríceps Francês Unilateral com Halter",
                "Tríceps Francês na Polia Alta com Corda", "Tríceps Testa com Barra W"];
            exigir(elevados.All(nome => FocosMusculares.Inferir("Tríceps", nome) == "braco_elevado"),
                "Francês e suas variações, inclusive testa, não podem formar focos artificiais por equipamento.");
            exigir(FocosMusculares.Inferir("TRICEPS", "TRICEPS PULLEY COM CORDA") == "braco_ao_lado"
                && FocosMusculares.Inferir("Tríceps", "Tríceps Coice com Halter") == "braco_atras",
                "Separar padrões do tríceps e tolerar nomes sem acento ou em maiúsculas.");
            exigir(FocosMusculares.Inferir("Ombros", "Elevação Lateral Inclinada no Banco") == "lateral"
                && FocosMusculares.Inferir("Ombros", "Crucifixo Inverso Inclinado com Halteres") == "posterior",
                "Inclinação do banco não altera o grupo nem torna toda elevação um foco superior.");
        });

        testar("foco explícito prevalece e dados inválidos mantêm fallback conservador", () =>
        {
            exigir(FocosMusculares.Resolver("Peito", "Supino Inclinado", "inferior") == "inferior",
                "Preservar classificação manual válida ao resolver um item.");
            exigir(FocosMusculares.Resolver("Peito", "Supino Inclinado", "geral") == "geral",
                "Geral escolhido explicitamente não deve ser substituído por inferência.");
            exigir(FocosMusculares.Resolver("Peito", "Supino Inclinado", null) == "superior"
                && FocosMusculares.Resolver("Peito", "Supino Inclinado", " ") == "superior",
                "Classificar automaticamente cadastros anteriores sem foco.");
            exigir(FocosMusculares.Resolver("Peito", "Supino Inclinado", "braco_elevado") == "geral",
                "Um foco inválido nunca deve abrir uma vaga adicional na geração.");
        });

        testar("todo o catálogo padrão recebe classificação válida para o grupo", () =>
        {
            foreach (var item in CatalogoPadraoService.ExerciciosPadrao.Concat(CatalogoPadraoService.MaquinasPadrao))
            {
                var grupo = grupos.Single(g => g.Id == item.GrupoMuscularId);
                var foco = FocosMusculares.Inferir(item.GrupoMuscularId, item.Nome);
                exigir(FocosMusculares.Valido(grupo.Nome, foco), $"Foco incompatível: {grupo.Nome} / {item.Nome}.");
                exigir(foco == FocosMusculares.Inferir(grupo.Nome, item.Nome), "IDs e nomes devem produzir a mesma classificação.");
                var geralEsperado = item.Nome is "Pullover com Halter" or "Rolo de Punho (Wrist Roller)";
                exigir((foco == "geral") == geralEsperado, $"Revisar classificação de {item.Nome} ({foco}).");
            }
        });

        testar("API e modelo persistem foco e removem a idade legada", () =>
        {
            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            foreach (var (tipo, item) in new (Type, object)[]
            {
                (typeof(Exercicio), new Exercicio { FocoMuscular = "superior" }),
                (typeof(Maquina), new Maquina { FocoMuscular = "superior" })
            })
            {
                var json = JsonSerializer.SerializeToElement(item, tipo, jsonOptions);
                exigir(json.GetProperty("focoMuscular").GetString() == "superior", "Expor foco nos dois contratos JSON.");
                var recebido = JsonSerializer.Deserialize(json.GetRawText(), tipo, jsonOptions)!;
                exigir((string?)tipo.GetProperty("FocoMuscular")!.GetValue(recebido) == "superior",
                    "Aceitar foco no cadastro e na edição.");
                var propriedade = db.Model.FindEntityType(tipo)!.FindProperty("FocoMuscular")!;
                exigir(propriedade.GetColumnName() == "foco_muscular" && !propriedade.IsNullable,
                    "Persistir a classificação nos dois catálogos.");
            }
            exigir(db.Model.FindEntityType(typeof(GrupoMuscular))!.FindProperty(nameof(GrupoMuscular.OrdemNaPrioridade)) is null,
                "O desempate da prioridade é calculado e não cria coluna.");
            exigir(db.Model.FindEntityType(typeof(Aluno))!.GetProperties()
                .All(p => p.Name != "IdadeLegada" && p.GetColumnName() != "idade_legada"),
                "Nenhuma propriedade ou coluna de idade legada permanece no modelo atual.");
            var sementes = db.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(GrupoMuscular))!.GetSeedData().ToArray();
            foreach (var grupo in grupos)
            {
                var semente = sementes.Single(s => (int)s["Id"]! == grupo.Id);
                exigir((string)semente["Nome"]! == grupo.Nome
                    && (int)semente["OrdemPrioridade"]! == grupo.OrdemPrioridade,
                    $"Manter identidade e nova prioridade do grupo {grupo.Nome}.");
            }
        });

        testar("100 sorteios limitam supinos inclinados a duas ocorrências por dia", () =>
        {
            var exercicios = Livres("Peito", "Supino Inclinado com Barra", "Supino Inclinado com Halteres",
                "Crucifixo Inclinado com Halteres");
            var maquinas = Maquinas("Peito", "Supino Inclinado Máquina", "Supino Inclinado no Smith");
            for (int tentativa = 0; tentativa < 100; tentativa++)
            {
                var treino = Gerar(Sessao("Peito"), exercicios, maquinas);
                exigir(treino.Itens.Count == 2, "Foco superior disponível em vários equipamentos ocupa no máximo duas vagas.");
            }
        });

        testar("peito cobre focos diferentes antes de repetir e limita cada foco a duas ocorrências", () =>
        {
            var exercicios = Livres("Peito", "Supino Inclinado com Barra", "Supino Inclinado com Halteres",
                "Supino Reto com Barra", "Crucifixo Reto com Halteres", "Supino Declinado com Barra");
            for (int tentativa = 0; tentativa < 30; tentativa++)
            {
                var treino = Gerar(Sessao("Peito"), exercicios, []);
                exigir(treino.Itens.Count == 5, "Usar os cinco movimentos disponíveis sem ultrapassar duas ocorrências por foco.");
                exigir(treino.Itens.Take(3).Select(i => Foco(i, exercicios, [])).Distinct().Count() == 3,
                    "Cobrir os três focos antes de selecionar a segunda ocorrência.");
                exigir(treino.Itens.GroupBy(i => Foco(i, exercicios, [])).All(g => g.Count() <= 2),
                    "Nenhuma ênfase pode ocupar três vagas.");
                exigir(treino.Itens.Select(i => Foco(i, exercicios, [])).ToHashSet()
                    .SetEquals(["superior", "esternocostal", "inferior"]), "Cobrir todas as ênfases disponíveis.");
            }
        });

        testar("francês e pulley respeitam duas ocorrências por foco entre equipamentos", () =>
        {
            var exercicios = Livres("Tríceps", "Tríceps Francês Bilateral com Halter",
                "Tríceps Francês Unilateral com Halter", "Tríceps Testa com Barra W");
            var maquinas = Maquinas("Tríceps", "Tríceps Francês na Polia Alta com Corda",
                "Tríceps Pulley com Corda", "Tríceps Pulley com Barra Reta");
            for (int tentativa = 0; tentativa < 100; tentativa++)
            {
                var treino = Gerar(Sessao("Tríceps"), exercicios, maquinas);
                exigir(treino.Itens.Count == 4 && treino.Itens.GroupBy(i => Foco(i, exercicios, maquinas)).All(g => g.Count() == 2)
                    && treino.Itens.Select(i => Foco(i, exercicios, maquinas))
                    .ToHashSet().SetEquals(["braco_elevado", "braco_ao_lado"]),
                    "Combinar ênfases distintas e tratar francês/testa e pulley/corda como variações dos mesmos padrões.");
            }
        });

        testar("foco distinto de outro equipamento prevalece sobre a meta de máquinas", () =>
        {
            var exercicios = Livres("Peito", "Supino Reto com Halteres");
            var maquinas = Maquinas("Peito", "Supino Inclinado Máquina", "Supino Inclinado no Smith");
            var treino = Gerar(Sessao("Peito"), exercicios, maquinas, "Iniciante");
            exigir(treino.Itens.Count == 3 && treino.Itens.Count(i => i.ExercicioId.HasValue) == 1
                && treino.Itens.Count(i => i.MaquinaId.HasValue) == 2, "Admitir duas máquinas do mesmo foco após cobrir o outro foco.");
            exigir(treino.Itens.Take(2).Select(i => Foco(i, exercicios, maquinas)).Distinct().Count() == 2,
                "Usar o exercício livre antes da segunda máquina do mesmo foco.");
        });

        testar("geral não se combina com outros focos e classificados têm preferência", () =>
        {
            var exercicios = Livres("Peito", "Exercício personalizado A", "Exercício personalizado B");
            var maquinas = Maquinas("Peito", "Máquina personalizada A", "Máquina personalizada B");
            exigir(Gerar(Sessao("Peito"), exercicios, maquinas).Itens.Count == 1,
                "Cadastros desconhecidos não criam focos únicos artificiais.");
            var classificados = Livres("Peito", "Supino Inclinado com Barra", "Supino Reto com Barra");
            for (int tentativa = 0; tentativa < 30; tentativa++)
            {
                var treino = Gerar(Sessao("Peito"), classificados, maquinas, "Iniciante");
                exigir(treino.Itens.Count == 2 && treino.Itens.All(i => i.ExercicioId.HasValue),
                    "Mesmo preferindo máquinas, escolher focos específicos e impedir geral após eles.");
            }
            classificados[0].FocoMuscular = "geral";
            classificados[1].FocoMuscular = "invalido";
            exigir(Gerar(Sessao("Peito"), classificados, []).Itens.Count == 1,
                "Foco explícito geral ou inválido também bloqueia duplicação no fallback.");
        });

        testar("focos podem reaparecer em outro dia quando o catálogo se esgota", () =>
        {
            var config = new DivisaoTreinoConfiguracao("teste", "Teste", 2, "", [
                new("Treino A - Peito", ["Peito"]), new("Treino B - Peito", ["Peito"])]);
            var exercicios = Livres("Peito", "Supino Inclinado com Barra", "Supino Reto com Barra", "Supino Declinado com Barra");
            var treino = Gerar(config, exercicios, []);
            exigir(treino.Itens.Count == 6 && treino.Itens.Select(i => i.ExercicioId).Distinct().Count() == 3,
                "Reutilizar os três movimentos no segundo dia, preservando a restrição dentro de cada sessão.");
            foreach (var dia in treino.Itens.GroupBy(i => i.DivisaoTreino))
                exigir(dia.Select(i => Foco(i, exercicios, [])).Distinct().Count() == 3, "Reiniciar focos usados por dia.");
        });

        testar("bíceps e tríceps precedem ombros, trapézio passa à terceira prioridade e séries são preservadas", () =>
        {
            string[] nomes = ["Ombros", "Tríceps", "Bíceps", "Trapézio", "Peito", "Panturrilha", "Abdômen"];
            var exercicios = nomes.SelectMany(n => Livres(n, $"Movimento de {n}"))
                .Select((e, i) => { e.Id = i + 1; return e; }).ToList();
            var treino = Gerar(Sessao(nomes), exercicios, [], "Intermediário");
            var sequencia = treino.Itens.Select(i => exercicios.Single(e => e.Id == i.ExercicioId).GrupoMuscular!.Nome);
            exigir(sequencia.SequenceEqual(["Peito", "Bíceps", "Tríceps", "Ombros", "Trapézio", "Panturrilha", "Abdômen"]),
                "Desempatar braços antes dos ombros e mover trapézio à terceira faixa.");
            foreach (var item in treino.Itens)
            {
                var nome = exercicios.Single(e => e.Id == item.ExercicioId).GrupoMuscular!.Nome;
                var series = nome is "Bíceps" or "Tríceps" or "Panturrilha" or "Abdômen" ? 3 : 4;
                exigir(item.Series == series, $"A reordenação não altera a prescrição de séries de {nome}.");
            }
        });

        testar("caso da foto respeita as metas 6/7/8 com somente dois exercícios de abdômen", () =>
        {
            var exercicios = CatalogoPadraoService.ExerciciosPadrao.Select((item, i) => new Exercicio {
                Id = i + 1, Nome = item.Nome, GrupoMuscularId = item.GrupoMuscularId,
                GrupoMuscular = grupos.Single(g => g.Id == item.GrupoMuscularId) }).ToList();
            var maquinas = CatalogoPadraoService.MaquinasPadrao.Select((item, i) => new Maquina {
                Id = i + 1, Nome = item.Nome, GrupoMuscularId = item.GrupoMuscularId,
                GrupoMuscular = grupos.Single(g => g.Id == item.GrupoMuscularId) }).ToList();
            foreach (var nivel in new[] { "Iniciante", "Intermediário", "Avançado" })
            for (int tentativa = 0; tentativa < 100; tentativa++)
            {
                // Mantém peito/abdômen na sessão B, reproduzindo a posição da foto.
                var config = DivisoesTreino.Obter("bro-split-6", 6)!;
                var treino = Gerar(config, exercicios, maquinas, nivel);
                var itens = treino.Itens.Where(i => i.DivisaoTreino == config.Sessoes[1].Titulo).ToArray();
                var porGrupo = itens.GroupBy(i => i.ExercicioId is int id
                    ? exercicios.Single(e => e.Id == id).GrupoMuscularId : maquinas.Single(m => m.Id == i.MaquinaId).GrupoMuscularId)
                    .ToDictionary(g => grupos.Single(x => x.Id == g.Key).Nome, g => g.Count());
                var peitoEsperado = nivel == "Iniciante" ? 4 : nivel == "Intermediário" ? 5 : 6;
                exigir(porGrupo["Peito"] == peitoEsperado && porGrupo["Abdômen"] == 2,
                    "O volume adicional deve ir para o peito depois de atingir o limite do abdômen.");
            }
        });

        testar("todos os grupos das prioridades três e quatro ficam limitados a dois exercícios por dia", () =>
        {
            foreach (var grupo in grupos.Where(g => g.OrdemPrioridade >= 3))
            {
                var focos = FocosMusculares.Opcoes[grupo.Nome].Where(f => f.Id != "geral").ToArray();
                var exercicios = Enumerable.Range(0, 12).Select(i => new Exercicio {
                    Id = i + 1, Nome = $"Movimento {i}", GrupoMuscularId = grupo.Id,
                    GrupoMuscular = grupo, FocoMuscular = focos[i % focos.Length].Id }).ToList();
                var config = new DivisaoTreinoConfiguracao("teste", "Teste", 2, "", [
                    new("Treino A", [grupo.Nome]), new("Treino B", [grupo.Nome])]);
                var treino = Gerar(config, exercicios, []);
                exigir(treino.Itens.Count == 4, "O limite de dois é por dia, e deve reiniciar na sessão seguinte.");
                foreach (var dia in treino.Itens.GroupBy(i => i.DivisaoTreino))
                    exigir(dia.Count() == 2 && dia.Select(i => Foco(i, exercicios, [])).Distinct().Count() == 2,
                        $"{grupo.Nome} deve ter no máximo dois movimentos, com focos distintos.");
            }
        });

        testar("todos os grupos das prioridades um e dois aceitam no máximo dois movimentos por foco", () =>
        {
            foreach (var grupo in grupos.Where(g => g.OrdemPrioridade <= 2))
            foreach (var foco in FocosMusculares.Opcoes[grupo.Nome].Where(f => f.Id != "geral"))
            {
                var exercicios = Enumerable.Range(1, 4).Select(i => new Exercicio {
                    Id = i, Nome = $"Livre {i}", GrupoMuscularId = grupo.Id,
                    GrupoMuscular = grupo, FocoMuscular = foco.Id }).ToList();
                var maquinas = Enumerable.Range(1, 4).Select(i => new Maquina {
                    Id = i, Nome = $"Máquina {i}", GrupoMuscularId = grupo.Id,
                    GrupoMuscular = grupo, FocoMuscular = foco.Id }).ToList();
                var treino = Gerar(Sessao(grupo.Nome), exercicios, maquinas);
                exigir(treino.Itens.Count == 2,
                    $"{grupo.Nome} / {foco.Id} permite duas ocorrências, somando os dois catálogos.");
                exigir(treino.Itens.Select(i => (i.ExercicioId, i.MaquinaId)).Distinct().Count() == 2,
                    "As duas ocorrências devem usar movimentos diferentes.");
            }
        });

        testar("catálogo escasso não permite completar o volume com abdômen excedente", () =>
        {
            var peito = Livres("Peito", "Supino Reto com Barra");
            var abdomen = Livres("Abdômen", "Abdominal Supra", "Abdominal Infra",
                "Prancha Frontal", "Prancha Lateral", "Abdominal Bicicleta")
                .Select(e => { e.Id += 10; return e; }).ToList();
            var exercicios = peito.Concat(abdomen).ToList();
            var treino = Gerar(Sessao("Abdômen", "Peito"), exercicios, []);
            exigir(treino.Itens.Count == 3
                && treino.Itens.Count(i => abdomen.Any(e => e.Id == i.ExercicioId)) == 2,
                "Mesmo com o auxiliar listado primeiro, parar após um peito disponível e dois abdominais.");
        });

        testar("catálogo real respeita focos por grupo em todas as divisões", () =>
        {
            var exercicios = CatalogoPadraoService.ExerciciosPadrao.Select((item, i) => new Exercicio {
                Id = i + 1, Nome = item.Nome, GrupoMuscularId = item.GrupoMuscularId,
                GrupoMuscular = grupos.Single(g => g.Id == item.GrupoMuscularId) }).ToList();
            var maquinas = CatalogoPadraoService.MaquinasPadrao.Select((item, i) => new Maquina {
                Id = i + 1, Nome = item.Nome, GrupoMuscularId = item.GrupoMuscularId,
                GrupoMuscular = grupos.Single(g => g.Id == item.GrupoMuscularId) }).ToList();
            for (int tentativa = 0; tentativa < 10; tentativa++)
            foreach (var config in DivisoesTreino.Todas)
            {
                var treino = Gerar(config, exercicios, maquinas);
                foreach (var dia in treino.Itens.GroupBy(i => i.DivisaoTreino))
                foreach (var grupoDia in dia.GroupBy(i => i.ExercicioId is int id
                    ? exercicios.Single(e => e.Id == id).GrupoMuscularId : maquinas.Single(m => m.Id == i.MaquinaId).GrupoMuscularId))
                {
                    var focos = grupoDia.Select(i => Foco(i, exercicios, maquinas)).ToArray();
                    var grupo = grupos.Single(g => g.Id == grupoDia.Key);
                    exigir((grupo.OrdemPrioridade <= 2 || focos.Length <= 2)
                        && focos.GroupBy(f => f).All(f => f.Count() <= (grupo.OrdemPrioridade <= 2 ? 2 : 1))
                        && (!focos.Contains("geral") || focos.Length == 1),
                        $"Limite excedido em {config.Nome} / {dia.Key} / grupo {grupoDia.Key}.");
                }
            }
        });
    }

    private static DivisaoTreinoConfiguracao Sessao(params string[] grupos) =>
        new("teste", "Teste", 1, "", [new("Treino A - Teste", grupos)]);

    private static List<Exercicio> Livres(string nomeGrupo, params string[] nomes)
    {
        var grupo = DivisoesTreinoTests.GruposPadrao().Single(g => g.Nome == nomeGrupo);
        return nomes.Select((nome, i) => new Exercicio { Id = i + 1, Nome = nome,
            GrupoMuscularId = grupo.Id, GrupoMuscular = grupo }).ToList();
    }

    private static List<Maquina> Maquinas(string nomeGrupo, params string[] nomes)
    {
        var grupo = DivisoesTreinoTests.GruposPadrao().Single(g => g.Nome == nomeGrupo);
        return nomes.Select((nome, i) => new Maquina { Id = i + 1, Nome = nome,
            GrupoMuscularId = grupo.Id, GrupoMuscular = grupo }).ToList();
    }

    private static Treino Gerar(DivisaoTreinoConfiguracao config, List<Exercicio> exercicios,
        List<Maquina> maquinas, string nivel = "Avançado") =>
        TreinoEndpoints.GerarTreinoPersonalizado(new Aluno { Id = 1, Nome = "Aluno", Sexo = "M",
            Experiencia = nivel, FrequenciaSemanal = config.Dias, Objetivo = "Hipertrofia",
            DataDeNascimento = Aluno.ObterDataAtual().AddYears(-30) }, 1, 1, 2,
            DivisoesTreinoTests.GruposPadrao(), exercicios, maquinas, config);

    private static string Foco(ItemTreino item, List<Exercicio> exercicios, List<Maquina> maquinas)
    {
        if (item.ExercicioId is int id)
        {
            var exercicio = exercicios.Single(e => e.Id == id);
            return FocosMusculares.Resolver(exercicio.GrupoMuscular!.Nome, exercicio.Nome, exercicio.FocoMuscular);
        }
        var maquina = maquinas.Single(m => m.Id == item.MaquinaId);
        return FocosMusculares.Resolver(maquina.GrupoMuscular!.Nome, maquina.Nome, maquina.FocoMuscular);
    }
}
