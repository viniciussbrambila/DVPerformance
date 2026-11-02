namespace DVPerformance.Api.Models;

public class DicaObjetivo
{
    public int Id { get; set; }

    // Objetivo correspondente ao cadastro do Aluno ("Hipertrofia", "Emagrecimento", "Força", "Resistência")
    public string Objetivo { get; set; } = string.Empty;

    // Texto da dica/orientação exibida no PDF em Planos Exportados
    public string Texto { get; set; } = string.Empty;

    // Permite ativar/desativar dicas mantendo integridade dos planos já gerados
    public bool Ativo { get; set; } = true;
}
