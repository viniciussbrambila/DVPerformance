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

        testar("chave pessoal de recuperação é única, validada por hash e decifrada com integridade", () =>
        {
            var chave1 = SegurancaService.GerarChaveRecuperacao();
            var chave2 = SegurancaService.GerarChaveRecuperacao();

            exigir(chave1.StartsWith("DVP-", StringComparison.Ordinal) && chave1.Length == 18,
                "A chave pessoal deve seguir o formato DVP-XXXX-XXXX-XXXX.");
            exigir(chave1 != chave2, "Duas chaves geradas devem ser distintas.");

            var hash = SegurancaService.GerarHashChaveRecuperacao(chave1);
            exigir(hash.Length <= 100, "O hash da chave deve caber em varchar(100).");
            exigir(SegurancaService.VerificarChaveRecuperacao(chave1.ToLowerInvariant(), hash),
                "A verificação deve aceitar a chave com normalização de espaços/caixa.");
            exigir(!SegurancaService.VerificarChaveRecuperacao(chave2, hash),
                "Chave incorreta deve ser rejeitada.");

            var cifrada = SegurancaService.CifrarChaveRecuperacao(chave1);
            exigir(cifrada.Length <= 200, "A chave cifrada deve caber em varchar(200).");
            exigir(SegurancaService.DecifrarChaveRecuperacao(cifrada) == chave1,
                "A decifragem AES-GCM deve restaurar exatamente a chave original.");
            exigir(SegurancaService.DecifrarChaveRecuperacao(cifrada + "adulterada") is null,
                "Texto cifrado adulterado deve falhar na verificação de tag.");
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

        testar("recuperação de senha exige chave pessoal e alteração de senha bloqueia senha repetida", () =>
        {
            var errosSemChave = AuthEndpoints.ValidarRecuperacaoSenha(new RecuperarSenhaRequest
            {
                Usuario = "instrutor.teste",
                NovaSenha = "NovaSenha123"
            });
            exigir(errosSemChave.Count == 1,
                "Rejeitar redefinição de senha sem a chave de recuperação pessoal.");

            var errosComChave = AuthEndpoints.ValidarRecuperacaoSenha(new RecuperarSenhaRequest
            {
                Usuario = "instrutor.teste",
                ChaveRecuperacao = "DVP-ABCD-EFGH-JKLM",
                NovaSenha = "NovaSenha123"
            });
            exigir(errosComChave.Count == 0,
                "Aceitar payload com usuário, chave pessoal e nova senha válidos.");

            var errosSenhaIgual = AuthEndpoints.ValidarAlteracaoSenha(new AlterarSenhaRequest
            {
                SenhaAtual = "MesmaSenha123",
                NovaSenha = "MesmaSenha123"
            });
            exigir(errosSenhaIgual.Count == 1,
                "Rejeitar alteração quando a nova senha é igual à senha atual.");

            var errosAlteracaoValida = AuthEndpoints.ValidarAlteracaoSenha(new AlterarSenhaRequest
            {
                SenhaAtual = "SenhaAtual123",
                NovaSenha = "NovaSenha123"
            });
            exigir(errosAlteracaoValida.Count == 0,
                "Aceitar alteração quando a nova senha é válida e diferente da atual.");
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

        testar("entidade Usuario nunca expõe Senha nem hashes/cifra de recuperação na serialização JSON (DV-SEC-008)", () =>
        {
            var usuario = new Usuario
            {
                Id = 1,
                NomeUsuario = "personal.dv",
                Senha = SegurancaService.GerarHashSenha("Segredo123"),
                ChaveRecuperacaoHash = SegurancaService.GerarHashChaveRecuperacao("DVP-ABCD-EFGH-JKLM"),
                ChaveRecuperacaoCifrada = SegurancaService.CifrarChaveRecuperacao("DVP-ABCD-EFGH-JKLM")
            };

            var json = JsonSerializer.SerializeToElement(usuario, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            exigir(!json.TryGetProperty("senha", out _) && !json.TryGetProperty("Senha", out _),
                "O campo Senha deve ser omitido por [JsonIgnore].");
            exigir(!json.TryGetProperty("chaveRecuperacaoHash", out _) && !json.TryGetProperty("chaveRecuperacaoCifrada", out _),
                "Os campos da chave de recuperação devem ser omitidos por [JsonIgnore].");
        });
    }
}
