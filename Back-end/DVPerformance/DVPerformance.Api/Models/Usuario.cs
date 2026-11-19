using System.Text.Json.Serialization;

namespace DVPerformance.Api.Models;

public class Usuario
{
    public int Id { get; set; }

    public string NomeUsuario { get; set; } = string.Empty;

    [JsonIgnore]
    public string Senha { get; set; } = string.Empty;

    [JsonIgnore]
    public string? ChaveRecuperacaoHash { get; set; }

    [JsonIgnore]
    public string? ChaveRecuperacaoCifrada { get; set; }

    // Por padrão, todo Personal cadastrado tem perfil e permissões de Admin
    public string Perfil { get; set; } = "admin";

    // Indica se a carga inicial de exercícios e máquinas pré-cadastrados já foi provisionada nesta conta
    [JsonIgnore]
    public bool CatalogoInicializado { get; set; } = false;

    // Exclusão lógica (Soft Delete): mantém o histórico no banco quando desativado (false)
    public bool Ativo { get; set; } = true;
}

// Dados enviados pelo Front-end no Cadastro e no Login
public class AuthRequest
{
    public string Usuario { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;
}

// Dados enviados pelo Front-end na tela de Esqueci Minha Senha com chave pessoal
public class RecuperarSenhaRequest
{
    public string Usuario { get; set; } = string.Empty;

    public string ChaveRecuperacao { get; set; } = string.Empty;

    public string NovaSenha { get; set; } = string.Empty;
}

// Dados enviados pelo usuário autenticado para consultar/regenerar sua chave de recuperação
public class ChaveRecuperacaoRequest
{
    public string Senha { get; set; } = string.Empty;
}

// Dados enviados pelo usuário autenticado para alterar a senha atual
public class AlterarSenhaRequest
{
    public string SenhaAtual { get; set; } = string.Empty;

    public string NovaSenha { get; set; } = string.Empty;
}
