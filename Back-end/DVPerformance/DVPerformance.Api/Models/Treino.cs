namespace DVPerformance.Api.Models;

public class Treino
{
    public int Id { get; set; }

    // FK obrigatória que vincula a ficha de treino ao Aluno
    public int AlunoId { get; set; }

    // FK opcional que vincula ao Personal que gerou o treino
    public int? UsuarioId { get; set; }

    // Título descritivo gerado pelo motor (ex: "Protocolo Emagrecimento Feminino - ABA (3x)")
    public string NomePlano { get; set; } = string.Empty;

    // Resumo da divisão adotada (ex: "Full Body", "AB", "ABC", "ABCD", "ABCDE")
    public string Divisao { get; set; } = string.Empty;

    // Orientação geral de aeróbico do protocolo (ex: "25 min Esteira/Bike moderado pós-treino")
    public string TempoAerobico { get; set; } = string.Empty;

    // Data/hora de geração da ficha (em UTC)
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Exclusão lógica (Soft Delete) / identifica se a ficha está ativa
    public bool Ativo { get; set; } = true;

    // Lista de itens (exercícios e máquinas) que compõem os dias deste treino
    public List<ItemTreino> Itens { get; set; } = new();
}
