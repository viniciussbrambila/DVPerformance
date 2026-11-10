using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DVPerformance.Api.Models;

public class Aluno
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Sexo { get; set; } = string.Empty;

    // Alunos anteriores à migração permanecem sem data até a edição do cadastro.
    public DateOnly? DataDeNascimento { get; set; }

    [NotMapped]
    [JsonIgnore]
    public int? Idade => CalcularIdade(DataDeNascimento, ObterDataAtual());

    public static DateOnly ObterDataAtual(DateTimeOffset? instante = null)
    {
        var horarioLocal = TimeZoneInfo.ConvertTime(
            instante ?? DateTimeOffset.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"));
        return DateOnly.FromDateTime(horarioLocal.DateTime);
    }

    public static int? CalcularIdade(DateOnly? nascimento, DateOnly referencia)
    {
        if (!nascimento.HasValue) return null;
        var data = nascimento.Value;
        var idade = referencia.Year - data.Year;
        // Em anos não bissextos, o aniversário de 29/02 ocorre em 01/03.
        if (referencia.Month < data.Month
            || (referencia.Month == data.Month && referencia.Day < data.Day))
            idade--;
        return idade;
    }

    public string Experiencia { get; set; } = string.Empty;

    public int FrequenciaSemanal { get; set; }

    public string Objetivo { get; set; } = string.Empty;

    // Medidas corporais adicionadas para relatórios e acompanhamento
    public decimal? Peso { get; set; }

    public decimal? Altura { get; set; }

    // Data de cadastro do aluno no sistema
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Chave estrangeira (FK) que vincula o aluno ao Personal que o cadastrou
    public int? UsuarioId { get; set; }

    // Exclusão lógica (Soft Delete): mantém o histórico no banco quando desativado (false)
    public bool Ativo { get; set; } = true;
}
