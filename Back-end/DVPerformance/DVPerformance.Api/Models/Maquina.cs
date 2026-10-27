namespace DVPerformance.Api.Models;

public class Maquina
{
    public int Id { get; set; }

    // Dicionário de Dados (Tabela Maquina): varchar(100), obrigatório (Não Nulo)
    public string Nome { get; set; } = string.Empty;

    // Região corporal macro preenchida automaticamente (Membros Superiores, Membros Inferiores, Core / Tronco)
    public string Regiao { get; set; } = string.Empty;

    // Dicionário de Dados (grupoMuscular): varchar(50), obrigatório (Não Nulo)
    public string Grupo { get; set; } = string.Empty;

    // Dicionário de Dados (descricao): varchar(255), opcional (Nulo = Sim)
    public string Descricao { get; set; } = string.Empty;

    // Chave estrangeira opcional para vincular ao Administrador/Personal que cadastrou (ou null se for padrão do sistema - RF03)
    public int? UsuarioId { get; set; }

    // Exclusão lógica (Soft Delete): mantém o histórico no banco quando desativada (false)
    public bool Ativo { get; set; } = true;
}
