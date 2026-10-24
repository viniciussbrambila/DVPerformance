namespace DVPerformance.Api.Models;

public class Usuario
{
    public int Id { get; set; }

    public string NomeUsuario { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;

    // Por padrão, todo Personal cadastrado tem perfil e permissões de Admin
    public string Perfil { get; set; } = "admin";
}

// Dados enviados pelo Front-end no Cadastro e no Login
public class AuthRequest
{
    public string Usuario { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;
}

// Dados enviados pelo Front-end na tela de Esqueci Minha Senha
public class RecuperarSenhaRequest
{
    public string Usuario { get; set; } = string.Empty;

    public string NovaSenha { get; set; } = string.Empty;
}
