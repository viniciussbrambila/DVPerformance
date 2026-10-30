using System.ComponentModel.DataAnnotations.Schema;

namespace DVPerformance.Api.Models;

public class Maquina
{
    private string _grupo = string.Empty;
    private string _regiao = string.Empty;

    public int Id { get; set; }

    // Dicionário de Dados (Tabela Maquina): varchar(100), obrigatório (Não Nulo)
    public string Nome { get; set; } = string.Empty;

    // Chave estrangeira unificada apontando para a tabela grupos_musculares (elimina redundância com Exercícios)
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

    // Dicionário de Dados (descricao): varchar(255), opcional (Nulo = Sim)
    public string Descricao { get; set; } = string.Empty;

    // Chave estrangeira opcional para vincular ao Administrador/Personal que cadastrou (ou null se for padrão do sistema - RF03)
    public int? UsuarioId { get; set; }

    // Exclusão lógica (Soft Delete): mantém o histórico no banco quando desativada (false)
    public bool Ativo { get; set; } = true;
}
