using System.Globalization;
using System.Text;

namespace DVPerformance.Api.Data;

/// <summary>
/// Ênfases e padrões usados para variar o estímulo dentro de cada grupo muscular.
/// As categorias não representam isolamento anatômico nem ausência de sobreposição.
/// Variações de equipamento do mesmo padrão compartilham a mesma classificação.
/// </summary>
public static class FocosMusculares
{
    public sealed record OpcaoFoco(string Id, string Nome);

    // Valor interno para itens sem foco reconhecido; não faz parte das opções de cadastro.
    public const string Geral = "geral";

    public static readonly string[] NiveisExperiencia = ["Iniciante", "Intermediário", "Avançado"];

    public static IReadOnlyDictionary<string, OpcaoFoco[]> Opcoes { get; } =
        new Dictionary<string, OpcaoFoco[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Peito"] =
            [
                new("superior", "Superior / movimento inclinado"),
                new("esternocostal", "Esternocostal / movimento horizontal"),
                new("inferior", "Inferior / movimento declinado")
            ],
            ["Costas"] =
            [
                new("puxada_vertical", "Puxada vertical"),
                new("remada_horizontal", "Remada horizontal"),
                new("extensao_ombro", "Extensão de ombro / pullover")
            ],
            ["Quadríceps"] =
            [
                new("dominante_joelho", "Agachamento / pressão de pernas"),
                new("unilateral_joelho", "Unilateral / afundo, passada e búlgaro"),
                new("extensao_joelho", "Extensão de joelho")
            ],
            ["Posterior de Coxa"] =
            [
                new("extensao_quadril", "Extensão de quadril"),
                new("flexao_joelho", "Flexão de joelho")
            ],
            ["Glúteos"] =
            [
                new("extensao_quadril_alongado", "Extensão com maior flexão de quadril"),
                new("extensao_quadril_encurtado", "Extensão / ponte e coice")
            ],
            ["Ombros"] =
            [
                new("anterior", "Anterior / desenvolvimento e elevação frontal"),
                new("lateral", "Lateral / abdução de ombro"),
                new("posterior", "Posterior / abertura inversa")
            ],
            ["Trapézio"] =
            [
                new("superior", "Superior / elevação escapular"),
                new("medio", "Médio / retração escapular"),
                new("inferior", "Inferior / depressão e elevação em Y")
            ],
            ["Adutores / Abdutores"] =
            [
                new("aducao", "Adução do quadril"),
                new("abducao", "Abdução do quadril")
            ],
            ["Bíceps"] =
            [
                new("ombro_neutro", "Braço ao lado do tronco"),
                new("ombro_estendido", "Braço atrás do tronco"),
                new("ombro_fletido", "Braço à frente / apoiado"),
                new("pegada_neutra", "Pegada neutra / martelo")
            ],
            ["Tríceps"] =
            [
                new("braco_elevado", "Braço elevado / francês e testa"),
                new("braco_ao_lado", "Braço ao lado / extensão na polia"),
                new("braco_atras", "Braço atrás / coice"),
                new("empurrao_composto", "Empurrão composto / paralelas e supino fechado")
            ],
            ["Panturrilha"] =
            [
                new("joelho_estendido", "Joelho estendido"),
                new("joelho_fletido", "Joelho flexionado / sentado"),
                new("dorsiflexao", "Dorsiflexão / tibial anterior")
            ],
            ["Antebraço"] =
            [
                new("flexao_punho", "Flexão de punho"),
                new("extensao_punho", "Extensão de punho"),
                new("flexao_cotovelo_pronada", "Flexão de cotovelo / pegada pronada"),
                new("pegada", "Sustentação / pegada")
            ],
            ["Abdômen"] =
            [
                new("flexao_tronco", "Flexão do tronco"),
                new("retroversao_pelve", "Retroversão da pelve / infra"),
                new("anti_extensao", "Resistência à extensão / prancha frontal"),
                new("estabilizacao_lateral", "Estabilização lateral"),
                new("rotacao", "Rotação do tronco"),
                new("anti_rotacao", "Resistência à rotação")
            ],
            ["Lombar"] =
            [
                new("extensao_tronco", "Extensão do tronco"),
                new("estabilizacao", "Estabilização do tronco"),
                new("dobradica_quadril", "Dobradiça do quadril")
            ]
        };

    private static readonly string[] GruposPorId =
    [
        "", "Peito", "Costas", "Quadríceps", "Posterior de Coxa", "Glúteos",
        "Ombros", "Trapézio", "Adutores / Abdutores", "Bíceps", "Tríceps",
        "Panturrilha", "Antebraço", "Abdômen", "Lombar"
    ];

    public static bool Valido(string grupo, string? foco)
    {
        var opcoes = ObterOpcoes(grupo);
        return opcoes is not null && opcoes.Any(opcao => opcao.Id == foco);
    }

    public static string Resolver(string grupo, string? nome, string? foco)
    {
        if (string.IsNullOrWhiteSpace(foco))
        {
            return Inferir(grupo, nome);
        }

        // Um foco explícito incompatível nunca é reinterpretado como outro foco.
        // Os endpoints rejeitam esse valor; o gerador mantém o fallback conservador.
        return Valido(grupo, foco) ? foco : Geral;
    }

    public static string Inferir(int grupoId, string? nome)
    {
        return grupoId > 0 && grupoId < GruposPorId.Length
            ? Inferir(GruposPorId[grupoId], nome)
            : Geral;
    }

    public static string Inferir(string grupo, string? nome)
    {
        var texto = Normalizar(nome);
        if (texto.Length == 0)
        {
            return Geral;
        }

        bool Tem(params string[] termos) => termos.Any(texto.Contains);

        // Regras específicas precedem as genéricas. Equipamento, lateralidade e
        // tipo de barra não criam um novo foco para o mesmo movimento.
        return Normalizar(grupo) switch
        {
            "peito" when Tem("pullover", "pull-over") => Geral,
            "peito" when Tem("inclinado", "inclinada", "crossover polia baixa") => "superior",
            "peito" when Tem("declinado", "declinada", "crossover polia alta") => "inferior",
            "peito" when Tem("supino reto", "crucifixo reto", "supino sentado",
                "chest press", "voador", "peck deck", "pec deck", "flexao de bracos") => "esternocostal",

            "costas" when Tem("pullover", "pull-over", "pulldown", "pull-down") => "extensao_ombro",
            "costas" when Tem("remada") => "remada_horizontal",
            "costas" when Tem("puxada", "barra fixa", "pull-up", "chin-up") => "puxada_vertical",

            "quadriceps" when Tem("extensora", "extensao de joelho", "sissy") => "extensao_joelho",
            "quadriceps" when Tem("afundo", "passada", "avanco", "bulgaro", "bulgara",
                "step-up", "step up", "subida no banco") => "unilateral_joelho",
            "quadriceps" when Tem("agachamento", "leg press", "v-squat",
                "belt squat", "pendulum squat") => "dominante_joelho",

            "posterior de coxa" when Tem("flexora", "flexor", "nordica", "nordico",
                "nordic", "glute ham", "ghd") => "flexao_joelho",
            "posterior de coxa" when Tem("stiff", "romeno", "rdl", "bom dia",
                "good morning", "extensao de quadril") => "extensao_quadril",

            "gluteos" when Tem("bulgaro", "bulgara", "afundo", "step-up", "step up",
                "subida no banco", "graviton") => "extensao_quadril_alongado",
            "gluteos" when Tem("elevacao pelvica", "hip thrust", "ponte", "coice",
                "4 apoios", "chute traseiro", "gluteo em pe", "extensao de quadril",
                "extensao diagonal") => "extensao_quadril_encurtado",

            "ombros" when Tem("inverso", "invertido", "face pull", "posterior") => "posterior",
            "ombros" when Tem("lateral", "abducao") => "lateral",
            "ombros" when Tem("desenvolvimento", "frontal", "shoulder press", "militar") => "anterior",

            "trapezio" when Tem("elevacao em y", "depressao escapular") => "inferior",
            "trapezio" when Tem("encolhimento no banco inclinado", "retracao escapular") => "medio",
            "trapezio" when Tem("encolhimento", "remada alta") => "superior",

            "adutores / abdutores" when Tem("abdu", "ostra", "clamshell") => "abducao",
            "adutores / abdutores" when Tem("adutora", "adutores", "aducao", "sumo",
                "cossaco", "afundo lateral", "copenhague") => "aducao",

            "biceps" when Tem("martelo", "pegada neutra") => "pegada_neutra",
            "biceps" when Tem("inclinada", "inclinado", "bayesiana", "bayesian") => "ombro_estendido",
            "biceps" when Tem("scott", "concentrada", "duplo biceps", "preacher") => "ombro_fletido",
            "biceps" when Tem("rosca direta", "rosca alternada",
                "biceps maquina articulada") => "ombro_neutro",

            "triceps" when Tem("frances", "testa", "acima da cabeca", "overhead") => "braco_elevado",
            "triceps" when Tem("coice", "kickback") => "braco_atras",
            "triceps" when Tem("supino fechado", "paralela", "mergulho", "dip") => "empurrao_composto",
            "triceps" when Tem("pulley", "pushdown", "push-down", "corda na polia alta",
                "supinado na polia") => "braco_ao_lado",

            "panturrilha" when Tem("tibial", "dorsiflexao") => "dorsiflexao",
            "panturrilha" when Tem("sentad", "solear", "joelho flexionado") => "joelho_fletido",
            "panturrilha" when Tem("em pe", "leg press", "smith", "joelho estendido") => "joelho_estendido",

            "antebraco" when Tem("rolo de punho", "wrist roller") => Geral,
            "antebraco" when Tem("punho invertida", "extensao de punho") => "extensao_punho",
            "antebraco" when Tem("rosca inversa", "pegada pronada") => "flexao_cotovelo_pronada",
            "antebraco" when Tem("rosca punho", "flexao de punho") => "flexao_punho",
            "antebraco" when Tem("fazendeiro", "farmer", "suspensao", "dead hang") => "pegada",

            "abdomen" when Tem("pallof", "anti-rotacao", "anti rotacao") => "anti_rotacao",
            "abdomen" when Tem("prancha lateral") => "estabilizacao_lateral",
            "abdomen" when Tem("bicicleta", "rotacao", "rotatoria", "russian twist") => "rotacao",
            "abdomen" when Tem("prancha frontal", "roda abdominal", "ab wheel") => "anti_extensao",
            "abdomen" when Tem("infra", "elevacao de pernas", "elevacao de joelhos") => "retroversao_pelve",
            "abdomen" when Tem("supra", "crunch", "abdominal no banco declinado") => "flexao_tronco",

            "lombar" when Tem("perdigueiro", "bird-dog", "bird dog", "ponte isometrica") => "estabilizacao",
            "lombar" when Tem("levantamento terra", "rack pull", "bom dia", "good morning") => "dobradica_quadril",
            "lombar" when Tem("superman", "hiperextensao", "extensao lombar") => "extensao_tronco",
            _ => Geral
        };
    }

    /// <summary>
    /// Classifica a complexidade biomecânica do movimento:
    /// 1 = Iniciante / Geral, 2 = Intermediário+, 3 = Avançado.
    /// </summary>
    public static int NivelComplexidade(string? nome)
    {
        var texto = Normalizar(nome);
        if (texto.Length == 0)
        {
            return 1;
        }

        bool Tem(params string[] termos) => termos.Any(texto.Contains);

        if (Tem("nordica", "nordico", "nordic", "sissy", "copenhague",
            "glute ham", "ghd", "roda abdominal", "ab wheel"))
        {
            return 3;
        }

        if ((Tem("barra fixa") && !Tem("graviton", "assistid"))
            || (Tem("paralela", "mergulho") && !Tem("graviton", "assistid", "maquina"))
            || Tem("agachamento frontal", "bom dia com barra", "good morning",
                "levantamento terra convencional", "levantamento terra sumo",
                "terra romeno", "rdl", "remada cavalinho livre"))
        {
            return 2;
        }

        return 1;
    }

    public static bool NivelValido(string? nivel) =>
        Normalizar(nivel) is "iniciante" or "intermediario" or "avancado";

    public static string InferirNivelMinimo(string? nome) =>
        NivelComplexidade(nome) switch
        {
            3 => "Avançado",
            2 => "Intermediário",
            _ => "Iniciante"
        };

    public static string ResolverNivelMinimo(string? nome, string? nivelInformado) =>
        Normalizar(nivelInformado) switch
        {
            "iniciante" => "Iniciante",
            "intermediario" => "Intermediário",
            "avancado" => "Avançado",
            _ => InferirNivelMinimo(nome)
        };

    public static bool CompativelComExperiencia(
        string? nome,
        string? experiencia,
        int? idade = null,
        string? nivelMinimoItem = null)
    {
        int limite = idade > 65 ? 1 : Normalizar(experiencia) switch
        {
            "iniciante" => 1,
            "avancado" => 3,
            _ => 2
        };

        int exigido = ResolverNivelMinimo(nome, nivelMinimoItem) switch
        {
            "Avançado" => 3,
            "Intermediário" => 2,
            _ => 1
        };

        return exigido <= limite;
    }

    /// <summary>
    /// Define a ordem biomecânica dentro do mesmo grupo muscular:
    /// 0 = Multiarticulares / Compostos base, 1 = Unilaterais / Intermediários, 2 = Isoladores / Finalizadores.
    /// </summary>
    public static int OrdemBiomecanica(string grupo, string? nome, string? foco = null)
    {
        var texto = Normalizar(nome);
        var focoResolvido = Resolver(grupo, nome, foco);
        bool Tem(params string[] termos) => termos.Any(texto.Contains);

        return Normalizar(grupo) switch
        {
            "peito" => Tem("crucifixo", "voador", "peck deck", "pec deck", "crossover", "pullover", "pull-over") ? 2 : 0,
            "costas" => focoResolvido == "extensao_ombro" || Tem("pullover", "pull-over", "pulldown", "pull-down") ? 2 : 0,
            "quadriceps" => focoResolvido == "extensao_joelho" ? 2 : focoResolvido == "unilateral_joelho" ? 1 : 0,
            "posterior de coxa" => focoResolvido == "flexao_joelho" ? 1 : 0,
            "gluteos" => Tem("coice", "4 apoios", "chute traseiro", "gluteo em pe", "extensao diagonal", "caneleira") ? 2
                : focoResolvido == "extensao_quadril_alongado" ? 1 : 0,
            "ombros" => Tem("desenvolvimento", "shoulder press", "militar") ? 0 : 2,
            "triceps" => focoResolvido == "empurrao_composto" ? 0 : focoResolvido == "braco_atras" ? 2 : 1,
            "biceps" => focoResolvido is "ombro_neutro" or "pegada_neutra" ? 0 : 1,
            _ => 1
        };
    }

    private static OpcaoFoco[]? ObterOpcoes(string grupo)
    {
        if (Opcoes.TryGetValue(grupo, out var opcoes))
        {
            return opcoes;
        }

        var grupoNormalizado = Normalizar(grupo);
        return Opcoes.FirstOrDefault(item => Normalizar(item.Key) == grupoNormalizado).Value;
    }

    private static string Normalizar(string? texto)
    {
        var normalizado = (texto ?? string.Empty).Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder(normalizado.Length);
        foreach (var caractere in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                resultado.Append(char.ToLowerInvariant(caractere));
            }
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC).Trim();
    }
}
