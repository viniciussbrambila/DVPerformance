namespace DVPerformance.Api.Models;

public record AtualizarItemTreinoRequest(
    int Id,
    int? ExercicioId,
    int? MaquinaId,
    int Series,
    string? Repeticao,
    string? TempoDescanso);

public record AtualizarItensTreinoRequest(List<AtualizarItemTreinoRequest?>? Itens);
