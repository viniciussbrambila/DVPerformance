namespace DVPerformance.Api.Models;

public record AtualizarItemTreinoRequest(
    int Id,
    int? ExercicioId,
    int? MaquinaId,
    int Series,
    string? Repeticao,
    string? TempoDescanso,
    string? DivisaoTreino = null,
    int? TempoAerobico = null);

public record AtualizarItensTreinoRequest(List<AtualizarItemTreinoRequest?>? Itens);
