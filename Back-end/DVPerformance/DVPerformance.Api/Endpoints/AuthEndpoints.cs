using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/auth"
        var grupo = app.MapGroup("/auth");

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

            // Verifica no banco se já existe um usuário com o mesmo nome (ignorando maiúsculas/minúsculas)
            var usuarioJaExiste = await db.Usuarios.AnyAsync(u =>
                u.NomeUsuario.ToLower() == nomeLimpo.ToLower());

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
                Senha = dados.Senha,
                Perfil = "admin",
                CatalogoInicializado = false,
                Ativo = true
            };

            db.Usuarios.Add(novoUsuario);
            await db.SaveChangesAsync();

            // Provisiona automaticamente o catálogo completo de exercícios e máquinas na nova conta do Personal
            await CatalogoPadraoService.GarantirCatalogoDoPersonalAsync(db, novoUsuario);

            // Retorna os dados da conta criada sem expor a senha
            return Results.Created($"/auth/usuarios/{novoUsuario.Id}", new
            {
                id = novoUsuario.Id,
                usuario = novoUsuario.NomeUsuario,
                nome = novoUsuario.NomeUsuario,
                perfil = novoUsuario.Perfil,
                ativo = novoUsuario.Ativo,
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
                u.NomeUsuario.ToLower() == nomeLimpo.ToLower() &&
                u.Senha == dados.Senha);

            if (conta is null)
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

            // Garante que contas antigas recebam os pré-cadastros ao entrar (respeitando personalizações posteriores)
            if (!conta.CatalogoInicializado)
            {
                await CatalogoPadraoService.GarantirCatalogoDoPersonalAsync(db, conta);
            }

            return Results.Ok(new
            {
                id = conta.Id,
                usuario = conta.NomeUsuario,
                nome = conta.NomeUsuario,
                perfil = conta.Perfil,
                ativo = conta.Ativo,
                mensagem = "Login realizado com sucesso."
            });
        });

        // PUT /auth/recuperar-senha -> Altera a senha de um usuário ativo existente no banco
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
                u.NomeUsuario.ToLower() == nomeLimpo.ToLower() && u.Ativo);

            if (conta is null)
            {
                return Results.NotFound(new
                {
                    mensagem = "Usuário não encontrado ou desativado.",
                    erros = new[] { "Não foi encontrada nenhuma conta ativa com o usuário informado." }
                });
            }

            conta.Senha = dados.NovaSenha;
            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                mensagem = "Senha alterada com sucesso! Você já pode entrar com a nova senha."
            });
        });

        // DELETE /auth/usuarios/{id} -> Desativa a conta do Personal (Soft Delete) mantendo o histórico
        grupo.MapDelete("/usuarios/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do usuário deve ser um número inteiro positivo." });
            }

            var conta = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.Ativo);
            if (conta is null)
            {
                return Results.NotFound(new { mensagem = "Usuário não encontrado ou já desativado." });
            }

            conta.Ativo = false;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // PATCH /auth/usuarios/{id}/reativar -> Reativa uma conta de Personal desativada anteriormente
        grupo.MapPatch("/usuarios/{id:int}/reativar", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do usuário deve ser um número inteiro positivo." });
            }

            var conta = await db.Usuarios.FindAsync(id);
            if (conta is null)
            {
                return Results.NotFound(new { mensagem = "Usuário não encontrado." });
            }

            conta.Ativo = true;
            await db.SaveChangesAsync();

            if (!conta.CatalogoInicializado)
            {
                await CatalogoPadraoService.GarantirCatalogoDoPersonalAsync(db, conta);
            }

            return Results.Ok(new
            {
                id = conta.Id,
                usuario = conta.NomeUsuario,
                perfil = conta.Perfil,
                ativo = conta.Ativo,
                mensagem = "Conta reativada com sucesso."
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
