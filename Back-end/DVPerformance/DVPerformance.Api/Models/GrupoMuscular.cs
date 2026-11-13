using System.ComponentModel.DataAnnotations.Schema;

namespace DVPerformance.Api.Models;

public class GrupoMuscular
{
    public int Id { get; set; }

    // Nome do grupo muscular unificado entre Exercícios e Máquinas (ex: "Peito", "Costas", "Quadríceps")
    public string Nome { get; set; } = string.Empty;

    // Região corporal macro ("Membros Superiores", "Membros Inferiores", "Core / Tronco")
    public string Regiao { get; set; } = string.Empty;

    // Prioridade na montagem do treino:
    // 1 = Grandes | 2 = Braços, ombros e adutores/abdutores | 3 = Trapézio e panturrilha | 4 = Finalizadores
    public int OrdemPrioridade { get; set; }

    [NotMapped]
    public int OrdemNaPrioridade => Nome switch
    {
        "Bíceps" => 0,
        "Tríceps" => 1,
        "Ombros" => 2,
        _ => 3
    };
}
