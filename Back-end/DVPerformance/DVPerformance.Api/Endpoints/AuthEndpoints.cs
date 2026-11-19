using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using DVPerformance.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/auth" e limita tentativas para mitigar força bruta
        var grupo = app.MapGroup("/auth")
            .RequireRateLimiting("AuthRateLimit");

        // POST /auth/cadastro -> Cria uma nova conta de Personal no banco PostgreSQL com chave de recuperação pessoal
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

            var chaveRecuperacao = SegurancaService.GerarChaveRecuperacao();

            var novoUsuario = new Usuario
            {
                NomeUsuario = nomeLimpo,
                Senha = SegurancaService.GerarHashSenha(dados.Senha),
                ChaveRecuperacaoHash = SegurancaService.GerarHashChaveRecuperacao(chaveRecuperacao),
                ChaveRecuperacaoCifrada = SegurancaService.CifrarChaveRecuperacao(chaveRecuperacao),
                Perfil = "admin",
                CatalogoInicializado = false,
                Ativo = true
            };

            db.Usuarios.Add(novoUsuario);
            await db.SaveChangesAsync();

            // Provisiona automaticamente o catálogo completo de exercícios e máquinas na nova conta do Personal
            await CatalogoPadraoService.GarantirCatalogoDoPersonalAsync(db, novoUsuario);

            var token = SegurancaService.GerarTokenSessao(novoUsuario);

            // Retorna os dados da conta criada e a chave pessoal de recuperação (sem expor senha ou hashes)
            return Results.Created($"/auth/usuarios/{novoUsuario.Id}", new
            {
                id = novoUsuario.Id,
                usuario = novoUsuario.NomeUsuario,
                nome = novoUsuario.NomeUsuario,
                perfil = novoUsuario.Perfil,
                ativo = novoUsuario.Ativo,
                token,
                chaveRecuperacao,
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

        // PUT /auth/recuperar-senha -> Altera a senha mediante comprovação pela chave de recuperação pessoal da conta
        grupo.MapPut("/recuperar-senha", async (RecuperarSenhaRequest? dados, AppDbContext db) =>
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

            if (conta is null || !SegurancaService.VerificarChaveRecuperacao(dados.ChaveRecuperacao, conta.ChaveRecuperacaoHash))
            {
                return Results.Json(new
                {
                    mensagem = "Usuário ou chave de recuperação inválidos.",
                    erros = new[] { "Verifique o usuário e a chave pessoal de recuperação da conta." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            if (SegurancaService.VerificarSenha(dados.NovaSenha, conta.Senha, out _))
            {
                return Results.BadRequest(new
                {
                    mensagem = "A nova senha deve ser diferente da senha atual.",
                    erros = new[] { "Escolha uma senha diferente da senha atual da conta." }
                });
            }

            var novaChaveRecuperacao = SegurancaService.GerarChaveRecuperacao();

            conta.Senha = SegurancaService.GerarHashSenha(dados.NovaSenha);
            conta.ChaveRecuperacaoHash = SegurancaService.GerarHashChaveRecuperacao(novaChaveRecuperacao);
            conta.ChaveRecuperacaoCifrada = SegurancaService.CifrarChaveRecuperacao(novaChaveRecuperacao);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                mensagem = "Senha alterada com sucesso! Uma nova chave de recuperação foi gerada para sua conta.",
                chaveRecuperacao = novaChaveRecuperacao
            });
        });

        // POST /auth/chave-recuperacao -> Revela a chave de recuperação pessoal após confirmar a senha da conta autenticada
        grupo.MapPost("/chave-recuperacao", async (ChaveRecuperacaoRequest? dados, HttpContext httpContext, AppDbContext db) =>
        {
            var autenticado = SegurancaService.ObterUsuarioAutenticado(httpContext);
            if (autenticado is null)
            {
                return Results.Json(new
                {
                    mensagem = "Sessão inválida ou expirada. Faça login novamente.",
                    erros = new[] { "É necessário estar autenticado para consultar a chave de recuperação." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            if (dados is null || string.IsNullOrWhiteSpace(dados.Senha))
            {
                return Results.BadRequest(new
                {
                    mensagem = "Informe sua senha para visualizar a chave de recuperação.",
                    erros = new[] { "Informe a senha de cadastro da conta." }
                });
            }

            var conta = await db.Usuarios.FirstOrDefaultAsync(u =>
                u.Id == autenticado.Id && u.Ativo);

            if (conta is null || !SegurancaService.VerificarSenha(dados.Senha, conta.Senha, out _))
            {
                return Results.Json(new
                {
                    mensagem = "Senha incorreta.",
                    erros = new[] { "A senha informada não confere com a senha atual da conta." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var chaveRecuperacao = SegurancaService.DecifrarChaveRecuperacao(conta.ChaveRecuperacaoCifrada);
            if (string.IsNullOrWhiteSpace(chaveRecuperacao) || string.IsNullOrWhiteSpace(conta.ChaveRecuperacaoHash))
            {
                chaveRecuperacao = SegurancaService.GerarChaveRecuperacao();
                conta.ChaveRecuperacaoHash = SegurancaService.GerarHashChaveRecuperacao(chaveRecuperacao);
                conta.ChaveRecuperacaoCifrada = SegurancaService.CifrarChaveRecuperacao(chaveRecuperacao);
                await db.SaveChangesAsync();
            }

            return Results.Ok(new
            {
                chaveRecuperacao
            });
        });

        // POST /auth/chave-recuperacao/regenerar -> Gera uma nova chave de recuperação após confirmar a senha
        grupo.MapPost("/chave-recuperacao/regenerar", async (ChaveRecuperacaoRequest? dados, HttpContext httpContext, AppDbContext db) =>
        {
            var autenticado = SegurancaService.ObterUsuarioAutenticado(httpContext);
            if (autenticado is null)
            {
                return Results.Json(new
                {
                    mensagem = "Sessão inválida ou expirada. Faça login novamente.",
                    erros = new[] { "É necessário estar autenticado para gerar uma nova chave." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            if (dados is null || string.IsNullOrWhiteSpace(dados.Senha))
            {
                return Results.BadRequest(new
                {
                    mensagem = "Informe sua senha para gerar uma nova chave de recuperação.",
                    erros = new[] { "Informe a senha de cadastro da conta." }
                });
            }

            var conta = await db.Usuarios.FirstOrDefaultAsync(u =>
                u.Id == autenticado.Id && u.Ativo);

            if (conta is null || !SegurancaService.VerificarSenha(dados.Senha, conta.Senha, out _))
            {
                return Results.Json(new
                {
                    mensagem = "Senha incorreta.",
                    erros = new[] { "A senha informada não confere com a senha atual da conta." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var novaChave = SegurancaService.GerarChaveRecuperacao();
            conta.ChaveRecuperacaoHash = SegurancaService.GerarHashChaveRecuperacao(novaChave);
            conta.ChaveRecuperacaoCifrada = SegurancaService.CifrarChaveRecuperacao(novaChave);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                chaveRecuperacao = novaChave,
                mensagem = "Nova chave de recuperação gerada com sucesso."
            });
        });

        // PUT /auth/alterar-senha -> Altera a senha da conta autenticada exigindo confirmação da senha atual
        grupo.MapPut("/alterar-senha", async (AlterarSenhaRequest? dados, HttpContext httpContext, AppDbContext db) =>
        {
            var autenticado = SegurancaService.ObterUsuarioAutenticado(httpContext);
            if (autenticado is null)
            {
                return Results.Json(new
                {
                    mensagem = "Sessão inválida ou expirada. Faça login novamente.",
                    erros = new[] { "É necessário estar autenticado para alterar a senha." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            var erros = ValidarAlteracaoSenha(dados);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos para alteração de senha.",
                    erros
                });
            }

            var conta = await db.Usuarios.FirstOrDefaultAsync(u =>
                u.Id == autenticado.Id && u.Ativo);

            if (conta is null || !SegurancaService.VerificarSenha(dados!.SenhaAtual, conta.Senha, out _))
            {
                return Results.Json(new
                {
                    mensagem = "Senha atual incorreta.",
                    erros = new[] { "A senha atual informada não confere." }
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            if (SegurancaService.VerificarSenha(dados.NovaSenha, conta.Senha, out _))
            {
                return Results.BadRequest(new
                {
                    mensagem = "A nova senha deve ser diferente da senha atual.",
                    erros = new[] { "A nova senha não pode ser igual à senha atual." }
                });
            }

            conta.Senha = SegurancaService.GerarHashSenha(dados.NovaSenha);
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                mensagem = "Senha alterada com sucesso!"
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
            erros.Add("Os dados para redefinição de senha não foram enviados.");
            return erros;
        }

        if (string.IsNullOrWhiteSpace(dados.Usuario))
        {
            erros.Add("Informe o seu usuário.");
        }

        if (string.IsNullOrWhiteSpace(dados.ChaveRecuperacao))
        {
            erros.Add("Informe a chave de recuperação da conta.");
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

    public static List<string> ValidarAlteracaoSenha(AlterarSenhaRequest? dados)
    {
        var erros = new List<string>();

        if (dados is null)
        {
            erros.Add("Os dados para alteração de senha não foram enviados.");
            return erros;
        }

        if (string.IsNullOrWhiteSpace(dados.SenhaAtual))
        {
            erros.Add("Informe a senha atual.");
        }

        if (string.IsNullOrWhiteSpace(dados.NovaSenha))
        {
            erros.Add("Informe a nova senha.");
        }
        else if (dados.NovaSenha.Length < 6)
        {
            erros.Add("A nova senha deve ter ao menos 6 caracteres.");
        }
        else if (string.Equals(dados.SenhaAtual, dados.NovaSenha, StringComparison.Ordinal))
        {
            erros.Add("A nova senha deve ser diferente da senha atual.");
        }

        return erros;
    }
}
