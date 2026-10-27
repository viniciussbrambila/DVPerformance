namespace DVPerformance.Api.Models;

public class Exercicio
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    // Região corporal macro preenchida automaticamente (Membros Superiores, Membros Inferiores, Core / Tronco)
    public string Regiao { get; set; } = string.Empty;

    // Grupo muscular específico selecionado pelo Personal
    public string Grupo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    // Chave estrangeira opcional para vincular ao Personal que cadastrou (ou null se for padrão do sistema)
    public int? UsuarioId { get; set; }

    // Exclusão lógica (Soft Delete): mantém o histórico no banco quando desativado (false)
    public bool Ativo { get; set; } = true;
}
