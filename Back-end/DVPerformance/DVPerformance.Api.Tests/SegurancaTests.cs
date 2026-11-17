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
        testar("PBKDF2-SHA256 gera hash salgado compatível com varchar(100) e valida senha em tempo constante", () =>
        {
            var hash1 = SegurancaService.GerarHashSenha("SenhaSegura123");
            var hash2 = SegurancaService.GerarHashSenha("SenhaSegura123");

            exigir(SegurancaService.SenhaEstaHasheada(hash1), "O hash precisa ter o prefixo PBKDF2$SHA256$.");
            exigir(hash1.Length <= 100, $"O hash ({hash1.Length} chars) deve caber na coluna varchar(100) do banco.");
            exigir(hash1 != hash2, "Dois hashes da mesma senha devem usar sais aleatórios distintos.");

            exigir(SegurancaService.VerificarSenha("SenhaSegura123", hash1, out var atualizar1) && !atualizar1,
                "A senha correta deve ser aceita sem exigir re-hash.");
            exigir(!SegurancaService.VerificarSenha("SenhaErrada123", hash1, out _),
                "Uma senha incorreta deve ser rejeitada.");
        });

        testar("verificação de senha suporta migração transparente de senhas legadas em texto plano", () =>
        {
            exigir(!SegurancaService.SenhaEstaHasheada("123456"), "Texto plano não deve ser identificado como hash.");
            exigir(SegurancaService.VerificarSenha("123456", "123456", out var precisaAtualizar) && precisaAtualizar,
                "Senha legada correta deve ser aceita e sinalizar atualização imediata para PBKDF2.");
            exigir(!SegurancaService.VerificarSenha("654321", "123456", out _),
                "Senha legada incorreta deve ser rejeitada.");
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

        testar("recuperação de senha exige senha atual ou código de recuperação (DV-SEC-001)", () =>
        {
            var errosSemComprovacao = AuthEndpoints.ValidarRecuperacaoSenha(new RecuperarSenhaRequest
            {
                Usuario = "instrutor.teste",
                NovaSenha = "NovaSenha123"
            });
            exigir(errosSemComprovacao.Count == 1,
                "Rejeitar redefinição de senha que informe apenas usuário e nova senha.");

            var errosComCodigo = AuthEndpoints.ValidarRecuperacaoSenha(new RecuperarSenhaRequest
            {
                Usuario = "instrutor.teste",
                NovaSenha = "NovaSenha123",
                CodigoRecuperacao = AuthEndpoints.CodigoRecuperacaoAcademicoPadrao
            });
            exigir(errosComCodigo.Count == 0,
                "Aceitar payload com código de recuperação preenchido.");

            var errosComSenhaAtual = AuthEndpoints.ValidarRecuperacaoSenha(new RecuperarSenhaRequest
            {
                Usuario = "instrutor.teste",
                NovaSenha = "NovaSenha123",
                SenhaAtual = "SenhaAntiga123"
            });
            exigir(errosComSenhaAtual.Count == 0,
                "Aceitar payload com confirmação da senha atual.");
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

        testar("entidade Usuario nunca expõe o campo Senha na serialização JSON (DV-SEC-008)", () =>
        {
            var usuario = new Usuario
            {
                Id = 1,
                NomeUsuario = "personal.dv",
                Senha = SegurancaService.GerarHashSenha("Segredo123")
            };

            var json = JsonSerializer.SerializeToElement(usuario, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            exigir(!json.TryGetProperty("senha", out _) && !json.TryGetProperty("Senha", out _),
                "O campo Senha deve ser omitido por [JsonIgnore].");
        });
    }
}
