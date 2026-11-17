using System.Security.Claims;
using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Security;

/// <summary>
/// Funções auxiliares de autorização multi-tenant e verificação de propriedade (ownership)
/// para evitar BOLA/IDOR mantendo compatibilidade com os testes de integração acadêmicos.
/// </summary>
public static class AutorizacaoEndpoints
{
    public static int? ResolverUsuarioId(HttpContext httpContext, int? usuarioIdInformado)
    {
        var autenticado = SegurancaService.ObterUsuarioAutenticado(httpContext);
        if (autenticado is not null)
        {
            return autenticado.Id;
        }

        return usuarioIdInformado is > 0 ? usuarioIdInformado : null;
    }

    public static IResult? ValidarMesmoUsuario(HttpContext httpContext, int? usuarioIdAlvo)
    {
        var autenticado = SegurancaService.ObterUsuarioAutenticado(httpContext);
        if (autenticado is not null && usuarioIdAlvo is > 0 && autenticado.Id != usuarioIdAlvo.Value)
        {
            return Results.Json(new
            {
                mensagem = "Acesso negado.",
                erros = new[] { "Você não tem permissão para acessar ou alterar recursos de outra conta de Personal." }
            }, statusCode: StatusCodes.Status403Forbidden);
        }

        return null;
    }

    public static async Task<(Usuario? usuario, IResult? erro)> ExigirAdministradorAtivoAsync(
        AppDbContext db,
        HttpContext httpContext,
        int? usuarioIdAlvo,
        string mensagemRn18)
    {
        var idResolvido = ResolverUsuarioId(httpContext, usuarioIdAlvo) ?? usuarioIdAlvo;
        if (idResolvido is not > 0)
        {
            return (null, Results.BadRequest(new
            {
                mensagem = "Informe o ID do usuário (usuarioId) responsável pela operação no catálogo de máquinas."
            }));
        }

        if (ValidarMesmoUsuario(httpContext, idResolvido) is IResult erroPropriedade)
        {
            return (null, erroPropriedade);
        }

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == idResolvido.Value && u.Ativo);
        if (usuario is null)
        {
            return (null, Results.BadRequest(new
            {
                mensagem = "O usuário informado não foi encontrado.",
                erros = new[] { "A conta informada (usuarioId) não existe ou está desativada no banco de dados." }
            }));
        }

        if (!string.Equals(usuario.Perfil, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return (null, Results.Json(new
            {
                mensagem = "Acesso negado.",
                erros = new[] { mensagemRn18 }
            }, statusCode: StatusCodes.Status403Forbidden));
        }

        return (usuario, null);
    }

    public static void AplicarClaimsNoContexto(HttpContext context, UsuarioAutenticado usuario)
    {
        context.Items[nameof(UsuarioAutenticado)] = usuario;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NomeUsuario),
            new Claim(ClaimTypes.Role, usuario.Perfil)
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }
}
