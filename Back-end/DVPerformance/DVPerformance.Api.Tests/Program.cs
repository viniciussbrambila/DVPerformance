using System.Reflection;
using System.Text.Json;
using DVPerformance.Api.Data;
using DVPerformance.Api.Endpoints;
using DVPerformance.Api.Migrations;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

var quantidade = 0;
void Testar(string nome, Action teste)
{
    teste();
    quantidade++;
    Console.WriteLine($"OK: {nome}");
}

void Exigir(bool condicao, string mensagem)
{
    if (!condicao) throw new InvalidOperationException(mensagem);
}

List<DicaObjetivo> Catalogo(int quantidadeDicas, string objetivo = "Hipertrofia") =>
    Enumerable.Range(1, quantidadeDicas)
        .Select(id => new DicaObjetivo { Id = id, Objetivo = objetivo, Texto = $"Dica {id}" })
        .ToList();

var formatar = typeof(TreinoEndpoints).GetMethod("FormatarPlanoResposta",
    BindingFlags.NonPublic | BindingFlags.Static)!;
JsonElement Resposta(Treino treino) =>
    JsonSerializer.SerializeToElement(formatar.Invoke(null, new object[] { treino })!);

Testar("cada ficha recebe duas dicas distintas do catálogo do objetivo", () =>
{
    var opcoes = Catalogo(12, "Força");
    for (var tentativa = 0; tentativa < 100; tentativa++)
    {
        var dicas = TreinoEndpoints.SortearDuasDicasSemRepetirCiclo(opcoes, Array.Empty<int>());
        Exigir(dicas.Count == 2 && dicas[0].Id != dicas[1].Id, "O par precisa conter duas dicas distintas.");
        Exigir(dicas.All(d => d.Objetivo == "Força"), "As dicas devem pertencer ao objetivo selecionado.");
    }
});

Testar("ambas as dicas contam no histórico e completam ciclos sem repetição", () =>
{
    var opcoes = Catalogo(12);
    var historico = new List<int>();
    for (var ficha = 0; ficha < 24; ficha++)
    {
        var anterior = historico.LastOrDefault();
        var dicas = TreinoEndpoints.SortearDuasDicasSemRepetirCiclo(opcoes, historico);
        Exigir(dicas[0].Id != dicas[1].Id, "Nunca repetir uma dica dentro da ficha.");
        if (historico.Count > 0 && historico.Count % 12 == 0)
            Exigir(dicas[0].Id != anterior, "Não repetir imediatamente a última dica ao reiniciar o ciclo.");
        historico.AddRange(dicas.Select(d => d.Id));
        if (historico.Count % 12 == 0)
            Exigir(historico.TakeLast(12).Distinct().Count() == 12, "Completar todo o catálogo antes de repetir.");
    }
});

Testar("virada do ciclo no meio do par mantém dicas distintas e não altera histórico recebido", () =>
{
    var historico = new List<int> { 1, 2 };
    for (var tentativa = 0; tentativa < 100; tentativa++)
    {
        var dicas = TreinoEndpoints.SortearDuasDicasSemRepetirCiclo(Catalogo(3), historico);
        Exigir(dicas[0].Id == 3 && dicas[1].Id != 3, "Usar a dica restante antes de iniciar o ciclo seguinte.");
    }
    Exigir(historico.SequenceEqual(new[] { 1, 2 }), "O sorteio não deve alterar o histórico fornecido.");
});

Testar("histórico de outro objetivo não consome dicas do objetivo atual", () =>
{
    var dicas = TreinoEndpoints.SortearDuasDicasSemRepetirCiclo(Catalogo(3), new[] { 50, 1, 51 });
    Exigir(dicas.Select(d => d.Id).Order().SequenceEqual(new[] { 2, 3 }), "Ignorar IDs de outros objetivos.");
});

Testar("catálogo vazio, dicas em branco e textos duplicados não criam dicas artificiais", () =>
{
    Exigir(TreinoEndpoints.SortearDuasDicasSemRepetirCiclo(new(), Array.Empty<int>()).Count == 0,
        "Catálogo vazio deve permanecer vazio.");
    var opcoes = Catalogo(1);
    opcoes.Add(new DicaObjetivo { Id = 2, Texto = " Dica 1 " });
    opcoes.Add(new DicaObjetivo { Id = 3, Texto = " " });
    Exigir(TreinoEndpoints.SortearDuasDicasSemRepetirCiclo(opcoes, Array.Empty<int>()).Count == 1,
        "Textos equivalentes ou vazios não podem compor um par.");
});

Testar("motor grava os dois IDs e leitura mantém o mesmo par sem novo sorteio", () =>
{
    var opcoes = Catalogo(12);
    var sorteadas = TreinoEndpoints.SortearDuasDicasSemRepetirCiclo(opcoes, Array.Empty<int>());
    var gerar = typeof(TreinoEndpoints).GetMethod("GerarTreinoPersonalizado",
        BindingFlags.NonPublic | BindingFlags.Static)!;
    var aluno = new Aluno { Id = 1, Nome = "Aluno", Sexo = "M", Idade = 30,
        Experiencia = "Iniciante", FrequenciaSemanal = 3, Objetivo = "Hipertrofia" };
    var gerado = (Treino)gerar.Invoke(null, new object?[] {
        aluno, 1, sorteadas[0].Id, sorteadas[1].Id,
        new List<GrupoMuscular>(), new List<Exercicio>(), new List<Maquina>()
    })!;
    Exigir(gerado.DicaObjetivoId == sorteadas[0].Id
        && gerado.SegundaDicaObjetivoId == sorteadas[1].Id, "Persistir os dois vínculos na entidade.");

    // Simula a materialização por seus vínculos gravados, sem executar sorteios na leitura.
    var recarregado = new Treino {
        Id = 7, AlunoId = aluno.Id, Aluno = aluno,
        DicaObjetivoId = gerado.DicaObjetivoId,
        SegundaDicaObjetivoId = gerado.SegundaDicaObjetivoId,
        DicaObjetivo = opcoes.Single(d => d.Id == gerado.DicaObjetivoId),
        SegundaDicaObjetivo = opcoes.Single(d => d.Id == gerado.SegundaDicaObjetivoId)
    };
    var textos = sorteadas.Select(d => d.Texto).ToArray();
    for (var leitura = 0; leitura < 20; leitura++)
    {
        var resposta = Resposta(recarregado);
        Exigir(resposta.GetProperty("dicas").EnumerateArray().Select(d => d.GetString()).SequenceEqual(textos),
            "A resposta precisa manter o mesmo par e a mesma ordem.");
        Exigir(resposta.GetProperty("dica").GetString() == textos[0], "Preservar o campo legado.");
    }
});

Testar("resposta ordena exercícios pela ordem gravada em cada dia", () =>
{
    var treino = new Treino {
        Itens = new List<ItemTreino> {
            new() { Id = 1, DivisaoTreino = "Treino A", OrdemExercicio = 3 },
            new() { Id = 2, DivisaoTreino = "Treino A", OrdemExercicio = 1 },
            new() { Id = 3, DivisaoTreino = "Treino A", OrdemExercicio = 2 },
            new() { Id = 4, DivisaoTreino = "Treino B", OrdemExercicio = 1 }
        }
    };
    var dias = Resposta(treino).GetProperty("dias");
    Exigir(dias[0].GetProperty("exercicios").EnumerateArray().Select(i => i.GetProperty("id").GetInt32())
        .SequenceEqual(new[] { 2, 3, 1 }), "Respeitar OrdemExercicio mesmo com IDs em outra ordem.");
    Exigir(dias[1].GetProperty("exercicios")[0].GetProperty("id").GetInt32() == 4, "Preservar o outro dia.");
});

var configuracao = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql("Host=localhost;Database=validacao_sem_conexao;Username=teste;Password=teste")
    .Options;
using var db = new AppDbContext(configuracao);

Testar("modelo mapeia os dois vínculos e snapshot não possui alterações pendentes", () =>
{
    var treino = db.Model.FindEntityType(typeof(Treino))!;
    Exigir(treino.FindProperty(nameof(Treino.SegundaDicaObjetivoId))?.GetColumnName() == "segunda_dica_objetivo_id",
        "Mapear a segunda dica para a coluna persistida.");
    Exigir(treino.GetForeignKeys().Any(fk => fk.Properties.Any(p => p.Name == nameof(Treino.SegundaDicaObjetivoId))
        && fk.PrincipalEntityType.ClrType == typeof(DicaObjetivo)), "Manter integridade referencial da segunda dica.");
    Exigir(!db.Database.HasPendingModelChanges(), "Snapshot deve corresponder ao modelo atual.");
});

Testar("migration completa fichas antigas com dica distinta do mesmo objetivo e cria restrição", () =>
{
    var migracao = new AdicionarSegundaDicaAoTreino();
    Exigir(migracao.UpOperations.OfType<AddColumnOperation>().Single().Name == "segunda_dica_objetivo_id",
        "Criar a coluna da segunda dica.");
    var sql = migracao.UpOperations.OfType<SqlOperation>().Single().Sql;
    Exigir(sql.Contains("dica.objetivo = primeira.objetivo")
        && sql.Contains("dica.id <> primeira.id") && sql.Contains("WHERE treino.dica_objetivo_id IS NULL"),
        "Preservar a primeira dica, completar fichas sem dica e selecionar outra do mesmo objetivo.");
    Exigir(migracao.UpOperations.OfType<AddCheckConstraintOperation>()
        .Any(op => op.Name == "ck_treinos_dicas_distintas"), "Impedir IDs duplicados no banco.");
    var script = db.GetService<IMigrator>().GenerateScript(
        "20261104005602_AdicionarPesoAlturaEDataCadastroAoAluno",
        "20261108160000_AdicionarSegundaDicaAoTreino");
    Exigir(script.Contains("segunda_dica_objetivo_id") && script.Contains("FOREIGN KEY"),
        "Gerar SQL de migração válido sem conectar ao banco.");
});

Console.WriteLine($"{quantidade} testes concluídos sem acessar o banco de dados.");
