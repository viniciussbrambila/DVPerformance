using System.Text.Json;
using DVPerformance.Api.Endpoints;
using DVPerformance.Api.Models;
using DVPerformance.Api.Security;
using Microsoft.AspNetCore.Http;

namespace DVPerformance.Api.Tests;

public static class SegurancaTests
{
    public static void Executar(Action<string, Action> testar, Action<bool, string> exigir)
    {
        testar("cadastro valida usuário e senha entre 6 e 100 caracteres", () =>
        {
            exigir(AuthEndpoints.ValidarCadastro(null).Count > 0,
                "Rejeitar cadastro sem dados.");
            exigir(AuthEndpoints.ValidarCadastro(new AuthRequest
            {
                Usuario = " ", Senha = "Senha123"
            }).Count > 0, "Exigir usuário no cadastro.");
            foreach (var senha in new[] { "", "12345", new string('a', 101) })
            {
                exigir(AuthEndpoints.ValidarCadastro(new AuthRequest
                {
                    Usuario = "personal.teste", Senha = senha
                }).Count > 0, "Cadastro deve respeitar os limites da coluna de senha.");
            }
            foreach (var senha in new[] { "123456", new string('a', 100) })
            {
                exigir(AuthEndpoints.ValidarCadastro(new AuthRequest
                {
                    Usuario = "personal.teste", Senha = senha
                }).Count == 0, "Aceitar os limites de tamanho no cadastro.");
            }
        });

        testar("token JWT HS256 autentica sessão legítima e rejeita adulteração ou expiração", () =>
        {
            var usuario = new Usuario
            {
                Id = 42,
                NomeUsuario = "personal.teste",
                Perfil = "admin",
                Ativo = true
            };

            var token = SegurancaService.GerarTokenSessao(usuario);
            exigir(SegurancaService.TentarValidarTokenSessao(token, out var autenticado) && autenticado is not null,
                "Token recém-emitido deve ser considerado válido.");
            exigir(autenticado!.Id == 42 && autenticado.NomeUsuario == "personal.teste" && autenticado.Perfil == "admin",
                "Claims de identidade e perfil devem ser preservadas no token.");

            // Adulteração da assinatura
            var partes = token.Split('.');
            var tokenAdulterado = $"{partes[0]}.{partes[1]}.assinaturaInvalida";
            exigir(!SegurancaService.TentarValidarTokenSessao(tokenAdulterado, out _),
                "Token com assinatura adulterada deve ser rejeitado.");

            // Token expirado (emitido há 13 horas, validade padrão de 12 horas)
            var agora = DateTimeOffset.UtcNow;
            var tokenExpirado = SegurancaService.GerarTokenSessao(usuario, agora.AddHours(-13));
            exigir(!SegurancaService.TentarValidarTokenSessao(tokenExpirado, out _, agora),
                "Token expirado deve ser rejeitado.");
        });

        testar("autorização multi-tenant bloqueia acesso cruzado entre personais (DV-SEC-002)", () =>
        {
            var contexto = new DefaultHttpContext();
            AutorizacaoEndpoints.AplicarClaimsNoContexto(
                contexto,
                new UsuarioAutenticado(7, "personal7", "admin"));

            var erroOutroUsuario = AutorizacaoEndpoints.ValidarMesmoUsuario(contexto, 99);
            exigir(erroOutroUsuario is not null, "Deve bloquear acesso a recurso pertencente a outro usuarioId.");

            var erroMesmoUsuario = AutorizacaoEndpoints.ValidarMesmoUsuario(contexto, 7);
            exigir(erroMesmoUsuario is null, "Deve permitir acesso ao próprio recurso do personal autenticado.");
        });

        testar("entidade Usuario nunca expõe Senha na serialização JSON (DV-SEC-008)", () =>
        {
            var usuario = new Usuario
            {
                Id = 1,
                NomeUsuario = "personal.dv",
                Senha = "Segredo123"
            };

            var json = JsonSerializer.SerializeToElement(usuario, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            exigir(!json.TryGetProperty("senha", out _) && !json.TryGetProperty("Senha", out _),
                "O campo Senha deve ser omitido por [JsonIgnore].");
        });
    }
}
