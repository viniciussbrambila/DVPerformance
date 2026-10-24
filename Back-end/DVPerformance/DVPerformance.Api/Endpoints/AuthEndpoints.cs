using DVPerformance.Api.Models;

namespace DVPerformance.Api.Endpoints;

public static class AuthEndpoints
{
    // Lista em memória para armazenar as contas criadas (Personal / Admin)
    private static readonly List<Usuario> Usuarios = new();

    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/auth"
        var grupo = app.MapGroup("/auth");

        // POST /auth/cadastro -> Cria uma nova conta no sistema (sempre com poderes de Admin)
        grupo.MapPost("/cadastro", (AuthRequest? dados) =>
        {
            var erros = ValidarCadastro(dados);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos no cadastro da conta.",
                    erros
                });
            }

            var nomeLimpo = dados!.Usuario.Trim();

            // Verifica se já existe um usuário com o mesmo nome (ignorando maiúsculas/minúsculas)
            var usuarioJaExiste = Usuarios.Any(u =>
                string.Equals(u.NomeUsuario, nomeLimpo, StringComparison.OrdinalIgnoreCase));

            if (usuarioJaExiste)
            {
                return Results.Conflict(new
                {
                    mensagem = "Este nome de usuário já está em uso.",
                    erros = new[] { "Já existe uma conta cadastrada com este nome de usuário." }
                });
            }

            var novoUsuario = new Usuario
            {
                Id = Usuarios.Count > 0 ? Usuarios.Max(u => u.Id) + 1 : 1,
                NomeUsuario = nomeLimpo,
                Senha = dados.Senha,
                Perfil = "admin"
            };

            Usuarios.Add(novoUsuario);

            // Retorna os dados da conta criada sem expor a senha
            return Results.Created($"/auth/usuarios/{novoUsuario.Id}", new
            {
                id = novoUsuario.Id,
                usuario = novoUsuario.NomeUsuario,
                nome = novoUsuario.NomeUsuario,
                perfil = novoUsuario.Perfil,
                mensagem = "Conta criada com sucesso."
            });
        });

        // POST /auth/login -> Autentica o usuário para entrar no sistema
        grupo.MapPost("/login", (AuthRequest? dados) =>
        {
            var erros = ValidarLogin(dados);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Informe o usuário e a senha para entrar.",
                    erros
                });
            }

            var nomeLimpo = dados!.Usuario.Trim();

            var conta = Usuarios.FirstOrDefault(u =>
                string.Equals(u.NomeUsuario, nomeLimpo, StringComparison.OrdinalIgnoreCase) &&
                u.Senha == dados.Senha);

            if (conta is null)
            {
                return Results.Json(new
                {
                    mensagem = "Usuário ou senha incorretos.",
                    erros = new[] { "Credenciais inválidas. Verifique seu usuário e senha." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Ok(new
            {
                id = conta.Id,
                usuario = conta.NomeUsuario,
                nome = conta.NomeUsuario,
                perfil = conta.Perfil,
                mensagem = "Login realizado com sucesso."
            });
        });

        // PUT /auth/recuperar-senha -> Altera a senha de um usuário existente (tela Esqueci Minha Senha)
        grupo.MapPut("/recuperar-senha", (RecuperarSenhaRequest? dados) =>
        {
            var erros = ValidarRecuperacaoSenha(dados);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos para redefinição de senha.",
                    erros
                });
            }

            var nomeLimpo = dados!.Usuario.Trim();

            var conta = Usuarios.FirstOrDefault(u =>
                string.Equals(u.NomeUsuario, nomeLimpo, StringComparison.OrdinalIgnoreCase));

            if (conta is null)
            {
                return Results.NotFound(new
                {
                    mensagem = "Usuário não encontrado.",
                    erros = new[] { "Não foi encontrada nenhuma conta com o usuário informado." }
                });
            }

            conta.Senha = dados.NovaSenha;

            return Results.Ok(new
            {
                mensagem = "Senha alterada com sucesso! Você já pode entrar com a nova senha."
            });
        });
    }

    private static List<string> ValidarCadastro(AuthRequest? dados)
    {
        var erros = new List<string>();

        if (dados is null)
        {
            erros.Add("Os dados de cadastro não foram enviados.");
            return erros;
        }

        var usuario = dados.Usuario?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(usuario))
        {
            erros.Add("O nome de usuário é obrigatório.");
        }
        else if (usuario.Length < 3 || usuario.Length > 50)
        {
            erros.Add("O nome de usuário deve ter entre 3 e 50 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(dados.Senha))
        {
            erros.Add("A senha é obrigatória.");
        }
        else if (dados.Senha.Length < 6)
        {
            erros.Add("A senha deve ter ao menos 6 caracteres.");
        }

        return erros;
    }

    private static List<string> ValidarLogin(AuthRequest? dados)
    {
        var erros = new List<string>();

        if (dados is null)
        {
            erros.Add("As credenciais de login não foram enviadas.");
            return erros;
        }

        if (string.IsNullOrWhiteSpace(dados.Usuario))
        {
            erros.Add("Informe o seu usuário.");
        }

        if (string.IsNullOrWhiteSpace(dados.Senha))
        {
            erros.Add("Informe a sua senha.");
        }

        return erros;
    }

    private static List<string> ValidarRecuperacaoSenha(RecuperarSenhaRequest? dados)
    {
        var erros = new List<string>();

        if (dados is null)
        {
            erros.Add("Os dados para alteração de senha não foram enviados.");
            return erros;
        }

        if (string.IsNullOrWhiteSpace(dados.Usuario))
        {
            erros.Add("Informe o seu usuário.");
        }

        if (string.IsNullOrWhiteSpace(dados.NovaSenha))
        {
            erros.Add("Informe a nova senha.");
        }
        else if (dados.NovaSenha.Length < 6)
        {
            erros.Add("A nova senha deve ter ao menos 6 caracteres.");
        }

        return erros;
    }
}
