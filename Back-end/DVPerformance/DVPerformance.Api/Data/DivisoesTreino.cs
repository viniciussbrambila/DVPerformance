namespace DVPerformance.Api.Data;

public sealed record SessaoDivisaoTreino(string Titulo, string[] Grupos)
{
    // O catálogo mantém um grupo conjunto, mas a sessão exige movimentos específicos.
    public string[] FocosAdutoresAbdutores => !Grupos.Contains("Adutores / Abdutores") ? [] :
        [
            .. (Grupos.Contains("Quadríceps") ? new[] { "aducao" } : []),
            .. (Grupos.Contains("Glúteos") ? new[] { "abducao" } : [])
        ];

    public string[] GruposExibicao => Grupos.SelectMany(grupo =>
        grupo == "Adutores / Abdutores" && FocosAdutoresAbdutores.Length > 0
            ? FocosAdutoresAbdutores.Select(foco => foco == "aducao" ? "Adutores" : "Abdutores")
            : [grupo]).ToArray();
}

public sealed record DivisaoTreinoConfiguracao(
    string Id,
    string Nome,
    int Dias,
    string Descricao,
    IReadOnlyList<SessaoDivisaoTreino> Sessoes,
    string NivelMinimo = "Iniciante");

public static class DivisoesTreino
{
    public const string IdManual = "manual";
    public const string NomeManual = "Treino Manual";

    private static readonly string[] FullBodyA =
        ["Peito", "Costas", "Quadríceps", "Posterior de Coxa", "Panturrilha", "Adutores / Abdutores"];
    private static readonly string[] FullBodyB =
        ["Peito", "Costas", "Ombros", "Glúteos", "Panturrilha", "Adutores / Abdutores"];
    private static readonly string[] FullBodyC =
        ["Peito", "Costas", "Ombros", "Quadríceps", "Posterior de Coxa", "Adutores / Abdutores"];
    private static readonly string[] Upper =
        ["Peito", "Costas", "Ombros", "Bíceps", "Tríceps"];
    private static readonly string[] UpperComAbdomen =
        ["Peito", "Costas", "Ombros", "Bíceps", "Tríceps", "Abdômen"];
    private static readonly string[] UpperCompleto =
        ["Peito", "Costas", "Ombros", "Trapézio", "Bíceps", "Tríceps"];
    private static readonly string[] Lower =
        ["Quadríceps", "Posterior de Coxa", "Glúteos", "Panturrilha", "Adutores / Abdutores"];
    private static readonly string[] Push = ["Peito", "Ombros", "Tríceps"];
    private static readonly string[] PushComAbdomen = ["Peito", "Ombros", "Tríceps", "Abdômen"];
    private static readonly string[] Pull = ["Costas", "Trapézio", "Bíceps"];
    private static readonly string[] EmpurrarAnterior =
        ["Peito", "Ombros", "Tríceps", "Quadríceps", "Adutores / Abdutores"];
    private static readonly string[] PuxarPosterior =
        ["Costas", "Bíceps", "Posterior de Coxa", "Glúteos", "Panturrilha", "Adutores / Abdutores"];

    private static SessaoDivisaoTreino Sessao(char letra, string nome, string[] grupos) =>
        new($"Treino {letra} - {nome}", grupos);

    private static DivisaoTreinoConfiguracao Divisao(
        string id, string nome, int dias, string nivelMinimo, string descricao, params SessaoDivisaoTreino[] sessoes) =>
        new(id, nome, dias, descricao, sessoes, nivelMinimo);

    public static IReadOnlyList<DivisaoTreinoConfiguracao> Todas { get; } =
    [
        // ==================== 2 DIAS ====================
        Divisao("full-body-ab", "Full Body A/B", 2, "Iniciante",
            "Grupos alternados entre A e B, com adutores junto de quadríceps e abdutores junto de glúteos.",
            Sessao('A', "Full Body A", FullBodyA),
            Sessao('B', "Full Body B", FullBodyB)),
        Divisao("upper-lower-2", "Superiores / Inferiores (A/B)", 2, "Iniciante",
            "Divisão simples separando membros superiores no Treino A e membros inferiores no Treino B.",
            Sessao('A', "Superiores", Upper),
            Sessao('B', "Inferiores", Lower)),

        // ==================== 3 DIAS ====================
        Divisao("full-body-abc", "Full Body A/B/C", 3, "Iniciante",
            "Corpo inteiro em três sessões equilibradas com estímulos variados ao longo da semana.",
            Sessao('A', "Full Body A", FullBodyA),
            Sessao('B', "Full Body B", FullBodyB),
            Sessao('C', "Full Body C", FullBodyC)),
        Divisao("ab-aba-3", "Superiores / Inferiores (A/B/A)", 3, "Iniciante",
            "Estrutura A/B adaptada para três dias, com duas sessões de superiores e uma de inferiores.",
            Sessao('A', "Superiores A", Upper),
            Sessao('B', "Inferiores", Lower),
            Sessao('C', "Superiores B", Upper)),
        Divisao("ppl", "PPL", 3, "Iniciante",
            "Push, Pull e Legs: empurrar, puxar e pernas.",
            Sessao('A', "Push", Push),
            Sessao('B', "Pull", Pull),
            Sessao('C', "Legs", Lower)),

        // ==================== 4 DIAS ====================
        Divisao("upper-lower", "Upper / Lower", 4, "Iniciante",
            "Superiores e inferiores alternados, com variação entre as sessões.",
            Sessao('A', "Upper A", Upper),
            Sessao('B', "Lower A", Lower),
            Sessao('C', "Upper B", Upper),
            Sessao('D', "Lower B", Lower)),
        Divisao("ab-funcional-4", "AB Funcional (Empurrar / Puxar)", 4, "Iniciante",
            "Combina cadeia de empurrar e quadríceps no A/C com cadeia de puxar e posteriores no B/D.",
            Sessao('A', "Empurrar e quadríceps A", EmpurrarAnterior),
            Sessao('B', "Puxar e posteriores A", PuxarPosterior),
            Sessao('C', "Empurrar e quadríceps B", EmpurrarAnterior),
            Sessao('D', "Puxar e posteriores B", PuxarPosterior)),
        Divisao("abcd", "ABCD", 4, "Intermediário",
            "Peito e tríceps; costas e bíceps; pernas; ombros e abdômen.",
            Sessao('A', "Peito e tríceps", ["Peito", "Tríceps"]),
            Sessao('B', "Costas e bíceps", ["Costas", "Bíceps"]),
            Sessao('C', "Pernas completas", Lower),
            Sessao('D', "Ombros e abdômen", ["Ombros", "Abdômen"])),

        // ==================== 5 DIAS ====================
        Divisao("ab-5", "Superiores / Inferiores (5 dias)", 5, "Iniciante",
            "Base simples de superiores e inferiores distribuída em cinco sessões semanais.",
            Sessao('A', "Superiores A", Upper),
            Sessao('B', "Inferiores A", Lower),
            Sessao('C', "Superiores B", Upper),
            Sessao('D', "Inferiores B", Lower),
            Sessao('E', "Superiores C", UpperComAbdomen)),
        Divisao("abc-5", "ABC Rotativo (5 dias)", 5, "Iniciante",
            "Divisão Push, Pull e Legs com repetição de empurrar e puxar na semana.",
            Sessao('A', "Push A", Push),
            Sessao('B', "Pull A", Pull),
            Sessao('C', "Legs", Lower),
            Sessao('D', "Push B", PushComAbdomen),
            Sessao('E', "Pull B", Pull)),
        Divisao("abcde", "ABCDE", 5, "Intermediário",
            "Cinco sessões com quadríceps separados de posteriores e glúteos.",
            Sessao('A', "Peito e tríceps", ["Peito", "Tríceps"]),
            Sessao('B', "Costas e bíceps", ["Costas", "Bíceps"]),
            Sessao('C', "Quadríceps e panturrilhas", ["Quadríceps", "Panturrilha", "Adutores / Abdutores"]),
            Sessao('D', "Ombros, trapézio e abdômen", ["Ombros", "Trapézio", "Abdômen"]),
            Sessao('E', "Posteriores e glúteos", ["Posterior de Coxa", "Glúteos", "Adutores / Abdutores"])),
        Divisao("ulppl", "ULPPL", 5, "Intermediário",
            "Upper e Lower seguidos de Push, Pull e Legs.",
            Sessao('A', "Upper", UpperCompleto),
            Sessao('B', "Lower", Lower),
            Sessao('C', "Push", Push),
            Sessao('D', "Pull", Pull),
            Sessao('E', "Legs", Lower)),
        Divisao("bro-split-5", "Bro Split (5 dias)", 5, "Avançado",
            "Peito, costas, ombros, pernas e braços em sessões próprias.",
            Sessao('A', "Peito e abdômen", ["Peito", "Abdômen"]),
            Sessao('B', "Costas e lombar", ["Costas", "Lombar"]),
            Sessao('C', "Ombros e trapézio", ["Ombros", "Trapézio"]),
            Sessao('D', "Pernas completas", Lower),
            Sessao('E', "Bíceps, tríceps e antebraços", ["Bíceps", "Tríceps", "Antebraço"])),

        // ==================== 6 DIAS ====================
        Divisao("upper-lower-3x", "Upper / Lower 3x", 6, "Iniciante",
            "Superiores e inferiores alternados três vezes na semana com exercícios variados.",
            Sessao('A', "Upper A", Upper),
            Sessao('B', "Lower A", Lower),
            Sessao('C', "Upper B", Upper),
            Sessao('D', "Lower B", Lower),
            Sessao('E', "Upper C", Upper),
            Sessao('F', "Lower C", Lower)),
        Divisao("ppl-2x", "PPL 2x", 6, "Iniciante",
            "Duas sessões de Push, Pull e Legs, com variação de exercícios.",
            Sessao('A', "Push A", Push),
            Sessao('B', "Pull A", Pull),
            Sessao('C', "Legs A", Lower),
            Sessao('D', "Push B", Push),
            Sessao('E', "Pull B", Pull),
            Sessao('F', "Legs B", Lower)),
        Divisao("arnold", "Arnold Split", 6, "Avançado",
            "Peito e costas; ombros e braços; pernas. Duas vezes na semana.",
            Sessao('A', "Peito e costas A", ["Peito", "Costas"]),
            Sessao('B', "Ombros e braços A", ["Ombros", "Bíceps", "Tríceps"]),
            Sessao('C', "Pernas completas A", Lower),
            Sessao('D', "Peito e costas B", ["Peito", "Costas"]),
            Sessao('E', "Ombros e braços B", ["Ombros", "Bíceps", "Tríceps"]),
            Sessao('F', "Pernas completas B", Lower)),
        Divisao("bro-split-6", "Bro Split (6 dias)", 6, "Avançado",
            "Seis sessões, separando quadríceps de posteriores e glúteos.",
            Sessao('A', "Costas e lombar", ["Costas", "Lombar"]),
            Sessao('B', "Peito e abdômen", ["Peito", "Abdômen"]),
            Sessao('C', "Quadríceps e panturrilhas", ["Quadríceps", "Panturrilha", "Adutores / Abdutores"]),
            Sessao('D', "Ombros e trapézio", ["Ombros", "Trapézio"]),
            Sessao('E', "Posteriores e glúteos", ["Posterior de Coxa", "Glúteos", "Adutores / Abdutores"]),
            Sessao('F', "Bíceps e tríceps", ["Bíceps", "Tríceps"]))
    ];

    public static int OrdemNivel(string? experiencia) => (experiencia ?? string.Empty).Trim() switch
    {
        "Iniciante" => 1,
        "Intermediário" or "Intermediario" => 2,
        "Avançado" or "Avancado" => 3,
        _ => 3
    };

    public static bool CompativelComExperiencia(DivisaoTreinoConfiguracao divisao, string? experiencia) =>
        string.IsNullOrWhiteSpace(experiencia) || OrdemNivel(divisao.NivelMinimo) <= OrdemNivel(experiencia);

    public static bool EhManual(string? id) =>
        string.Equals(id?.Trim(), IdManual, StringComparison.OrdinalIgnoreCase);

    public static string[] ObterTitulosDiasManuais(int dias)
    {
        int quantidade = Math.Clamp(dias, 2, 6);
        return Enumerable.Range(0, quantidade)
            .Select(i => $"Treino {(char)('A' + i)}")
            .ToArray();
    }

    public static IReadOnlyList<DivisaoTreinoConfiguracao> ObterDisponiveis(int dias, string? experiencia = null) =>
        Todas.Where(divisao => divisao.Dias == dias && CompativelComExperiencia(divisao, experiencia)).ToArray();

    public static DivisaoTreinoConfiguracao? Obter(string? id, int dias, string? experiencia = null) =>
        Todas.FirstOrDefault(divisao => divisao.Dias == dias
            && string.Equals(divisao.Id, id?.Trim(), StringComparison.Ordinal)
            && CompativelComExperiencia(divisao, experiencia));
}
