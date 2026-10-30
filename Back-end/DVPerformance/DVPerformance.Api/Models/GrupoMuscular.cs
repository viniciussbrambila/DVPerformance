namespace DVPerformance.Api.Models;

public class GrupoMuscular
{
    public int Id { get; set; }

    // Nome do grupo muscular unificado entre Exercícios e Máquinas (ex: "Peito", "Costas", "Quadríceps")
    public string Nome { get; set; } = string.Empty;

    // Região corporal macro ("Membros Superiores", "Membros Inferiores", "Core / Tronco")
    public string Regiao { get; set; } = string.Empty;

    // Prioridade na montagem do treino:
    // 1 = Músculo Grande (vem primeiro) | 2 = Músculo Médio | 3 = Músculo Pequeno | 4 = Core / Finalizador (vem por último)
    public int OrdemPrioridade { get; set; }
}
