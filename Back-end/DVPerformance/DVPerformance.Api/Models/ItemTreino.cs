using System.Text.Json.Serialization;

namespace DVPerformance.Api.Models;

public class ItemTreino
{
    // idItem conforme modelagem inicial
    public int Id { get; set; }

    // FK que vincula o item à ficha de treino (Cabeçalho)
    public int TreinoId { get; set; }

    [JsonIgnore]
    public Treino? Treino { get; set; }

    // divisaoTreino: identifica o dia/foco (ex: "Treino A - Inferiores (Quadríceps e Glúteos)")
    public string DivisaoTreino { get; set; } = string.Empty;

    // ordemExercicio: sequência pedagógica do exercício no dia (1, 2, 3...)
    public int OrdemExercicio { get; set; }

    // FK opcional quando o movimento vem da tabela Exercicios (livres / peso corporal)
    public int? ExercicioId { get; set; }

    public Exercicio? Exercicio { get; set; }

    // FK opcional quando o movimento vem da tabela Maquinas (equipamentos / polias)
    public int? MaquinaId { get; set; }

    public Maquina? Maquina { get; set; }

    // series: quantidade de séries prescritas (ex: 3, 4)
    public int Series { get; set; }

    // repeticao: faixa de repetições conforme o objetivo (ex: "12-15", "8-12", "4-6")
    public string Repeticao { get; set; } = string.Empty;

    // tempoDescanso: intervalo entre séries (ex: "45s", "60s", "120s")
    public string TempoDescanso { get; set; } = string.Empty;

    // tempoAerobico: prescrição de cardio daquele dia (ex: "25 min HIIT / Esteira pós-treino")
    public string TempoAerobico { get; set; } = string.Empty;
}
