using System.Security.Cryptography;
using System.Text;
using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using DVPerformance.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class AuthEndpoints
{
    public const string CodigoRecuperacaoAcademicoPadrao = "DVP-2026";

    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/auth" e limita tentativas para mitigar força bruta
        var grupo = app.MapGroup("/auth")
            .RequireRateLimiting("AuthRateLimit");

        // POST /auth/cadastro -> Cria uma nova conta de Personal no banco PostgreSQL já com todos os exercícios e máquinas pré-cadastrados
        grupo.MapPost("/cadastro", async (AuthRequest? dados, AppDbContext db) =>
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

            // O nome de usuário diferencia maiúsculas e minúsculas, como no login.
            var usuarioJaExiste = await db.Usuarios.AnyAsync(u =>
                u.NomeUsuario == nomeLimpo);

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
                NomeUsuario = nomeLimpo,
                Senha = SegurancaService.GerarHashSenha(dados.Senha),
                Perfil = "admin",
                CatalogoInicializado = false,
                Ativo = true
            };

            db.Usuarios.Add(novoUsuario);
            await db.SaveChangesAsync();

            // Provisiona automaticamente o catálogo completo de exercícios e máquinas na nova conta do Personal
            await CatalogoPadraoService.GarantirCatalogoDoPersonalAsync(db, novoUsuario);

            var token = SegurancaService.GerarTokenSessao(novoUsuario);

            // Retorna os dados da conta criada sem expor a senha
            return Results.Created($"/auth/usuarios/{novoUsuario.Id}", new
            {
                id = novoUsuario.Id,
                usuario = novoUsuario.NomeUsuario,
                nome = novoUsuario.NomeUsuario,
                perfil = novoUsuario.Perfil,
                ativo = novoUsuario.Ativo,
                token,
                mensagem = "Conta criada com sucesso com exercícios e máquinas pré-cadastrados."
            });
        });

        // POST /auth/login -> Autentica o Personal no banco PostgreSQL e garante os pré-cadastros caso a conta seja antiga
        grupo.MapPost("/login", async (AuthRequest? dados, AppDbContext db) =>
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

            var conta = await db.Usuarios.FirstOrDefaultAsync(u =>
                u.NomeUsuario == nomeLimpo);

            if (conta is null || !SegurancaService.VerificarSenha(dados.Senha, conta.Senha, out var precisaAtualizarHash))
            {
                return Results.Json(new
                {
                    mensagem = "Usuário ou senha incorretos.",
                    erros = new[] { "Credenciais inválidas. Verifique seu usuário e senha." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            if (!conta.Ativo)
            {
                return Results.Json(new
                {
                    mensagem = "Esta conta está desativada.",
                    erros = new[] { "A conta informada foi desativada. Entre em contato com o suporte para reativá-la." }
                }, statusCode: StatusCodes.Status403Forbidden);
            }

            // Migra automaticamente a senha legada em texto plano para PBKDF2-SHA256 no primeiro login válido
            if (precisaAtualizarHash)
            {
                conta.Senha = SegurancaService.GerarHashSenha(dados.Senha);
                await db.SaveChangesAsync();
            }

            // Garante que contas antigas recebam os pré-cadastros ao entrar (respeitando personalizações posteriores)
            if (!conta.CatalogoInicializado)
            {
                await CatalogoPadraoService.GarantirCatalogoDoPersonalAsync(db, conta);
            }

            var token = SegurancaService.GerarTokenSessao(conta);

            return Results.Ok(new
            {
                id = conta.Id,
                usuario = conta.NomeUsuario,
                nome = conta.NomeUsuario,
                perfil = conta.Perfil,
                ativo = conta.Ativo,
                token,
                mensagem = "Login realizado com sucesso."
            });
        });

        // PUT /auth/recuperar-senha -> Altera a senha de um usuário ativo mediante comprovação (senhaAtual ou codigoRecuperacao)
        grupo.MapPut("/recuperar-senha", async (RecuperarSenhaRequest? dados, AppDbContext db, IConfiguration config) =>
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

            var conta = await db.Usuarios.FirstOrDefaultAsync(u =>
                u.NomeUsuario == nomeLimpo && u.Ativo);

            if (conta is null)
            {
                return Results.Json(new
                {
                    mensagem = "Não foi possível redefinir a senha.",
                    erros = new[] { "Usuário não encontrado ou credenciais de verificação inválidas." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var codigoEsperado = config["Security:RecoveryCode"];
            if (string.IsNullOrWhiteSpace(codigoEsperado))
            {
                codigoEsperado = CodigoRecuperacaoAcademicoPadrao;
            }

            var autorizadoPorSenhaAtual = !string.IsNullOrWhiteSpace(dados.SenhaAtual) &&
                SegurancaService.VerificarSenha(dados.SenhaAtual, conta.Senha, out _);

            var autorizadoPorCodigo = !string.IsNullOrWhiteSpace(dados.CodigoRecuperacao) &&
                CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(dados.CodigoRecuperacao.Trim()),
                    Encoding.UTF8.GetBytes(codigoEsperado.Trim()));

            if (!autorizadoPorSenhaAtual && !autorizadoPorCodigo)
            {
                return Results.Json(new
                {
                    mensagem = "Código de recuperação ou senha atual inválidos.",
                    erros = new[] { "Informe a senha atual correta ou o código de recuperação válido da academia." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            conta.Senha = SegurancaService.GerarHashSenha(dados.NovaSenha);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                mensagem = "Senha alterada com sucesso! Você já pode entrar com a nova senha."
            });
        });
    }

    public static List<string> ValidarCadastro(AuthRequest? dados)
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

    public static List<string> ValidarLogin(AuthRequest? dados)
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

    public static List<string> ValidarRecuperacaoSenha(RecuperarSenhaRequest? dados)
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

        if (string.IsNullOrWhiteSpace(dados.SenhaAtual) && string.IsNullOrWhiteSpace(dados.CodigoRecuperacao))
        {
            erros.Add("Informe a senha atual ou o código de recuperação da conta.");
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
