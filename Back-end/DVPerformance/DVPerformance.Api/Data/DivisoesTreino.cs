namespace DVPerformance.Api.Data;

public sealed record SessaoDivisaoTreino(string Titulo, string[] Grupos);

public sealed record DivisaoTreinoConfiguracao(
    string Id,
    string Nome,
    int Dias,
    string Descricao,
    IReadOnlyList<SessaoDivisaoTreino> Sessoes);

public static class DivisoesTreino
{
    private static readonly string[] FullBody =
        ["Peito", "Costas", "Ombros", "Quadríceps", "Posterior de Coxa", "Glúteos", "Panturrilha"];
    private static readonly string[] Upper =
        ["Peito", "Costas", "Ombros", "Bíceps", "Tríceps"];
    private static readonly string[] UpperCompleto =
        ["Peito", "Costas", "Ombros", "Trapézio", "Bíceps", "Tríceps"];
    private static readonly string[] Lower =
        ["Quadríceps", "Posterior de Coxa", "Glúteos", "Panturrilha"];
    private static readonly string[] Push = ["Peito", "Ombros", "Tríceps"];
    private static readonly string[] Pull = ["Costas", "Trapézio", "Bíceps"];

    private static SessaoDivisaoTreino Sessao(char letra, string nome, string[] grupos) =>
        new($"Treino {letra} - {nome}", grupos);

    private static DivisaoTreinoConfiguracao Divisao(
        string id, string nome, int dias, string descricao, params SessaoDivisaoTreino[] sessoes) =>
        new(id, nome, dias, descricao, sessoes);

    public static IReadOnlyList<DivisaoTreinoConfiguracao> Todas { get; } =
    [
        Divisao("full-body-ab", "Full Body A/B", 2,
            "Corpo inteiro nas duas sessões, com variação de exercícios.",
            Sessao('A', "Full Body A", FullBody),
            Sessao('B', "Full Body B", FullBody)),
        Divisao("ppl", "PPL", 3,
            "Push, Pull e Legs: empurrar, puxar e pernas.",
            Sessao('A', "Push", Push),
            Sessao('B', "Pull", Pull),
            Sessao('C', "Legs", Lower)),
        Divisao("upper-lower", "Upper / Lower", 4,
            "Superiores e inferiores alternados, com variação entre as sessões.",
            Sessao('A', "Upper A", Upper),
            Sessao('B', "Lower A", Lower),
            Sessao('C', "Upper B", Upper),
            Sessao('D', "Lower B", Lower)),
        Divisao("abcd", "ABCD", 4,
            "Peito e tríceps; costas e bíceps; pernas; ombros e abdômen.",
            Sessao('A', "Peito e tríceps", ["Peito", "Tríceps"]),
            Sessao('B', "Costas e bíceps", ["Costas", "Bíceps"]),
            Sessao('C', "Pernas completas", Lower),
            Sessao('D', "Ombros e abdômen", ["Ombros", "Abdômen"])),
        Divisao("abcde", "ABCDE", 5,
            "Cinco sessões com quadríceps separados de posteriores e glúteos.",
            Sessao('A', "Peito e tríceps", ["Peito", "Tríceps"]),
            Sessao('B', "Costas e bíceps", ["Costas", "Bíceps"]),
            Sessao('C', "Quadríceps e panturrilhas", ["Quadríceps", "Panturrilha"]),
            Sessao('D', "Ombros, trapézio e abdômen", ["Ombros", "Trapézio", "Abdômen"]),
            Sessao('E', "Posteriores e glúteos", ["Posterior de Coxa", "Glúteos"])),
        Divisao("ulppl", "ULPPL", 5,
            "Upper e Lower seguidos de Push, Pull e Legs.",
            Sessao('A', "Upper", UpperCompleto),
            Sessao('B', "Lower", Lower),
            Sessao('C', "Push", Push),
            Sessao('D', "Pull", Pull),
            Sessao('E', "Legs", Lower)),
        Divisao("bro-split-5", "Bro Split (5 dias)", 5,
            "Peito, costas, ombros, pernas e braços em sessões próprias.",
            Sessao('A', "Peito", ["Peito"]),
            Sessao('B', "Costas", ["Costas"]),
            Sessao('C', "Ombros e trapézio", ["Ombros", "Trapézio"]),
            Sessao('D', "Pernas completas", Lower),
            Sessao('E', "Bíceps, tríceps e antebraços", ["Bíceps", "Tríceps", "Antebraço"])),
        Divisao("ppl-2x", "PPL 2x", 6,
            "Duas sessões de Push, Pull e Legs, com variação de exercícios.",
            Sessao('A', "Push A", Push),
            Sessao('B', "Pull A", Pull),
            Sessao('C', "Legs A", Lower),
            Sessao('D', "Push B", Push),
            Sessao('E', "Pull B", Pull),
            Sessao('F', "Legs B", Lower)),
        Divisao("arnold", "Arnold Split", 6,
            "Peito e costas; ombros e braços; pernas. Duas vezes na semana.",
            Sessao('A', "Peito e costas A", ["Peito", "Costas"]),
            Sessao('B', "Ombros e braços A", ["Ombros", "Bíceps", "Tríceps"]),
            Sessao('C', "Pernas completas A", Lower),
            Sessao('D', "Peito e costas B", ["Peito", "Costas"]),
            Sessao('E', "Ombros e braços B", ["Ombros", "Bíceps", "Tríceps"]),
            Sessao('F', "Pernas completas B", Lower)),
        Divisao("bro-split-6", "Bro Split (6 dias)", 6,
            "Seis sessões, separando quadríceps de posteriores e glúteos.",
            Sessao('A', "Costas", ["Costas"]),
            Sessao('B', "Peito e abdômen", ["Peito", "Abdômen"]),
            Sessao('C', "Quadríceps e panturrilhas", ["Quadríceps", "Panturrilha"]),
            Sessao('D', "Ombros e trapézio", ["Ombros", "Trapézio"]),
            Sessao('E', "Posteriores e glúteos", ["Posterior de Coxa", "Glúteos"]),
            Sessao('F', "Bíceps e tríceps", ["Bíceps", "Tríceps"]))
    ];

    public static IReadOnlyList<DivisaoTreinoConfiguracao> ObterDisponiveis(int dias) =>
        Todas.Where(divisao => divisao.Dias == dias).ToArray();

    public static DivisaoTreinoConfiguracao? Obter(string? id, int dias) =>
        Todas.FirstOrDefault(divisao => divisao.Dias == dias
            && string.Equals(divisao.Id, id?.Trim(), StringComparison.Ordinal));
}
