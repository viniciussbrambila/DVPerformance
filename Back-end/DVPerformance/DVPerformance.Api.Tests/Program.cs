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
        BindingFlags.Public | BindingFlags.Static)!;
    var aluno = new Aluno { Id = 1, Nome = "Aluno", Sexo = "M", DataDeNascimento = Aluno.ObterDataAtual().AddYears(-30),
        Experiencia = "Iniciante", FrequenciaSemanal = 3, Objetivo = "Hipertrofia" };
    var gerado = (Treino)gerar.Invoke(null, new object?[] {
        aluno, 1, sorteadas[0].Id, sorteadas[1].Id,
        new List<GrupoMuscular>(), new List<Exercicio>(), new List<Maquina>(),
        DivisoesTreino.Obter("ppl", 3)!
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

Testar("idade muda no aniversário, inclusive em 29 de fevereiro, sem persistir idade", () =>
{
    var nascimento = new DateOnly(2000, 11, 9);
    Exigir(Aluno.CalcularIdade(nascimento, new DateOnly(2026, 11, 8)) == 25, "Antes do aniversário.");
    Exigir(Aluno.CalcularIdade(nascimento, new DateOnly(2026, 11, 9)) == 26, "No aniversário.");
    Exigir(Aluno.CalcularIdade(nascimento, new DateOnly(2026, 11, 10)) == 26, "Após o aniversário.");
    var bissexto = new DateOnly(2000, 2, 29);
    Exigir(Aluno.CalcularIdade(bissexto, new DateOnly(2025, 2, 28)) == 24, "29/02 aguarda março em ano comum.");
    Exigir(Aluno.CalcularIdade(bissexto, new DateOnly(2025, 3, 1)) == 25, "29/02 faz aniversário em 01/03.");
    Exigir(Aluno.CalcularIdade(bissexto, new DateOnly(2024, 2, 29)) == 24, "29/02 em ano bissexto.");
    Exigir(Aluno.CalcularIdade(null, new DateOnly(2026, 11, 9)) is null, "Não inventar idade de cadastro legado.");
    Exigir(Aluno.ObterDataAtual(new DateTimeOffset(2026, 11, 10, 1, 30, 0, TimeSpan.Zero))
        == new DateOnly(2026, 11, 9), "Usar a data de São Paulo na virada do dia em UTC.");
});

Testar("nascimento é obrigatório, válido e mantém a faixa de 13 a 120 anos", () =>
{
    var referencia = new DateOnly(2026, 11, 9);
    var aluno = new Aluno { Experiencia = "Iniciante", FrequenciaSemanal = 3 };
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, referencia).Count == 1, "Exigir nascimento ausente.");
    aluno.DataDeNascimento = referencia.AddDays(1);
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, referencia).Single().Contains("futuro"), "Rejeitar nascimento futuro.");
    aluno.DataDeNascimento = referencia.AddYears(-13).AddDays(1);
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, referencia).Count == 1, "Rejeitar quem ainda tem 12 anos.");
    aluno.DataDeNascimento = referencia.AddYears(-13);
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, referencia).Count == 0, "Aceitar exatamente 13 anos.");
    aluno.DataDeNascimento = referencia.AddYears(-121).AddDays(1);
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, referencia).Count == 0, "Aceitar enquanto tem 120 anos.");
    aluno.DataDeNascimento = referencia.AddYears(-121);
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, referencia).Count == 1, "Rejeitar 121 anos.");
});

Testar("regras etárias passam a valer após completar 66 anos", () =>
{
    var aluno = new Aluno { DataDeNascimento = new DateOnly(1960, 11, 9),
        Experiencia = "Avançado", FrequenciaSemanal = 6 };
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, new DateOnly(2026, 11, 8)).Count == 0,
        "Permitir o perfil enquanto tem 65 anos.");
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, new DateOnly(2026, 11, 9)).Count == 2,
        "Revalidar experiência e frequência quando completa 66 anos.");
    aluno.Experiencia = "Intermediário";
    aluno.FrequenciaSemanal = 5;
    Exigir(AlunoEndpoints.ValidarDadosEtarios(aluno, new DateOnly(2026, 11, 9)).Count == 0,
        "Aceitar o perfil ajustado para a idade atual.");
});

Testar("API serializa a data ISO e banco mantém somente nascimento e legado privado", () =>
{
    var aluno = new Aluno { DataDeNascimento = new DateOnly(2000, 11, 9) };
    var json = JsonSerializer.SerializeToElement(aluno, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    Exigir(json.GetProperty("dataDeNascimento").GetString() == "2000-11-09", "Preservar a data sem horário ou fuso.");
    Exigir(!json.TryGetProperty("idade", out _) && !json.TryGetProperty("idadeLegada", out _),
        "Não enviar idade derivada ou legada no contrato público.");
    var entidade = db.Model.FindEntityType(typeof(Aluno))!;
    var propriedade = entidade.FindProperty(nameof(Aluno.DataDeNascimento))!;
    Exigir(propriedade.GetColumnName() == "data_de_nascimento" && propriedade.GetColumnType() == "date"
        && propriedade.IsNullable, "Permitir nascimento pendente apenas nos registros migrados.");
    Exigir(entidade.FindProperty(nameof(Aluno.Idade)) is null, "Idade calculada não é persistida.");
    Exigir(entidade.FindProperty("IdadeLegada")!.IsShadowProperty(), "Guardar o histórico sem expor na entidade.");
});

Testar("migração preserva idade anterior e não inventa aniversário para alunos antigos", () =>
{
    var migracao = new SubstituirIdadePorDataDeNascimento();
    Exigir(migracao.UpOperations.OfType<RenameColumnOperation>()
        .Any(op => op.Name == "idade" && op.NewName == "idade_legada"), "Preservar a coluna histórica.");
    Exigir(!migracao.UpOperations.OfType<DropColumnOperation>().Any(), "Não perder o dado anterior.");
    var coluna = migracao.UpOperations.OfType<AddColumnOperation>().Single();
    Exigir(coluna.Name == "data_de_nascimento" && coluna.IsNullable && coluna.DefaultValue is null
        && coluna.DefaultValueSql is null, "Manter os aniversários desconhecidos em branco.");
    Exigir(!migracao.UpOperations.OfType<SqlOperation>().Any(), "Não estimar datas com base na idade.");
    var script = db.GetService<IMigrator>().GenerateScript(
        "20261108160000_AdicionarSegundaDicaAoTreino",
        "20261109235000_SubstituirIdadePorDataDeNascimento");
    Exigir(script.Contains("RENAME COLUMN idade TO idade_legada")
        && script.Contains("ADD data_de_nascimento date"), "Gerar SQL da migração sem conexão.");
});

DVPerformance.Api.Tests.DivisoesTreinoTests.Executar(Testar, Exigir);

Console.WriteLine($"{quantidade} testes concluídos sem acessar o banco de dados.");
