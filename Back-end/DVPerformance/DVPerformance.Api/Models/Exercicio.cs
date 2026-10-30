using System.ComponentModel.DataAnnotations.Schema;

namespace DVPerformance.Api.Models;

public class Exercicio
{
    private string _grupo = string.Empty;
    private string _regiao = string.Empty;

    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    // Chave estrangeira unificada apontando para a tabela grupos_musculares (elimina redundância com Máquinas)
    public int GrupoMuscularId { get; set; }

    public GrupoMuscular? GrupoMuscular { get; set; }

    // Propriedades calculadas/compatíveis com o JSON do Front-end (lidas direto de GrupoMuscular)
    [NotMapped]
    public string Regiao
    {
        get => GrupoMuscular?.Regiao ?? _regiao;
        set => _regiao = value ?? string.Empty;
    }

    [NotMapped]
    public string Grupo
    {
        get => GrupoMuscular?.Nome ?? _grupo;
        set => _grupo = value ?? string.Empty;
    }

    public string Descricao { get; set; } = string.Empty;

    // Chave estrangeira opcional para vincular ao Personal que cadastrou (ou null se for padrão do sistema)
    public int? UsuarioId { get; set; }

    // Exclusão lógica (Soft Delete): mantém o histórico no banco quando desativado (false)
    public bool Ativo { get; set; } = true;
}
