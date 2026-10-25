namespace DVPerformance.Api.Models;

public class Aluno
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Sexo { get; set; } = string.Empty;

    public int Idade { get; set; }

    public string Experiencia { get; set; } = string.Empty;

    public int FrequenciaSemanal { get; set; }

    public string Objetivo { get; set; } = string.Empty;

    // Chave estrangeira (FK) que vincula o aluno ao Personal que o cadastrou
    public int? UsuarioId { get; set; }
}
