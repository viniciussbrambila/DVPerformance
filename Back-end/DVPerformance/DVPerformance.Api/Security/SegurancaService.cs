using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DVPerformance.Api.Models;

namespace DVPerformance.Api.Security;

/// <summary>
/// Serviço utilitário de criptografia e segurança da API DV Performance:
/// 1) Hash de senhas com PBKDF2-HMAC-SHA256 + salt aleatório de 128 bits e comparação em tempo constante;
/// 2) Emissão e validação de tokens de sessão assinados com HMAC-SHA256 (padrão JWT HS256).
/// </summary>
public static class SegurancaService
{
    private const string PrefixoHash = "PBKDF2$SHA256";
    private const int IteracoesPadrao = 100_000;
    private const int TamanhoSaltBytes = 16;
    private const int TamanhoChaveBytes = 32;
    private static readonly TimeSpan ValidadeTokenPadrao = TimeSpan.FromHours(12);

    // Chave de assinatura padrão de desenvolvimento acadêmico caso nenhuma variável de ambiente esteja definida.
    private const string SegredoDesenvolvimentoPadrao =
        "DVPerformance-Academic-Project-HMAC-SHA256-Secret-Key-2026-Univali";

    private static string _segredoJwt = SegredoDesenvolvimentoPadrao;

    public static void ConfigurarSegredo(string? segredoConfigurado)
    {
        if (!string.IsNullOrWhiteSpace(segredoConfigurado) && segredoConfigurado.Trim().Length >= 32)
        {
            _segredoJwt = segredoConfigurado.Trim();
        }
    }

    /// <summary>
    /// Verifica se o valor armazenado já está no formato PBKDF2-HMAC-SHA256.
    /// </summary>
    public static bool SenhaEstaHasheada(string? valorArmazenado) =>
        !string.IsNullOrWhiteSpace(valorArmazenado) &&
        valorArmazenado.StartsWith(PrefixoHash + "$", StringComparison.Ordinal);

    /// <summary>
    /// Gera um hash seguro no formato PBKDF2$SHA256${iteracoes}${saltBase64}${hashBase64} (90 caracteres, compatível com varchar(100)).
    /// </summary>
    public static string GerarHashSenha(string senha)
    {
        if (string.IsNullOrEmpty(senha))
        {
            throw new ArgumentException("A senha não pode ser vazia.", nameof(senha));
        }

        Span<byte> salt = stackalloc byte[TamanhoSaltBytes];
        RandomNumberGenerator.Fill(salt);

        Span<byte> subkey = stackalloc byte[TamanhoChaveBytes];
        Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(senha),
            salt,
            subkey,
            IteracoesPadrao,
            HashAlgorithmName.SHA256);

        return $"{PrefixoHash}${IteracoesPadrao}${Convert.ToBase64String(salt)}${Convert.ToBase64String(subkey)}";
    }

    /// <summary>
    /// Verifica a senha informada contra o valor armazenado no banco.
    /// Suporta contas já com PBKDF2-SHA256 e contas legadas em texto plano (sinalizando necessidade de atualização automática).
    /// </summary>
    public static bool VerificarSenha(string senhaInformada, string? senhaArmazenada, out bool precisaAtualizarHash)
    {
        precisaAtualizarHash = false;

        if (string.IsNullOrEmpty(senhaInformada) || string.IsNullOrEmpty(senhaArmazenada))
        {
            return false;
        }

        if (SenhaEstaHasheada(senhaArmazenada))
        {
            var partes = senhaArmazenada.Split('$');
            if (partes.Length != 5 || !int.TryParse(partes[2], out var iteracoes) || iteracoes < 10_000)
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(partes[3]);
                var hashEsperado = Convert.FromBase64String(partes[4]);
                var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(senhaInformada),
                    salt,
                    iteracoes,
                    HashAlgorithmName.SHA256,
                    hashEsperado.Length);

                return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Compatibilidade com contas antigas salvas em texto plano antes da migração de segurança:
        var bytesInformados = Encoding.UTF8.GetBytes(senhaInformada);
        var bytesArmazenados = Encoding.UTF8.GetBytes(senhaArmazenada);
        var iguais = CryptographicOperations.FixedTimeEquals(bytesInformados, bytesArmazenados);
        if (iguais)
        {
            precisaAtualizarHash = true;
        }

        return iguais;
    }

    /// <summary>
    /// Emite um token compacto no padrão JWT (Header.Payload.Signature) assinado com HMAC-SHA256.
    /// </summary>
    public static string GerarTokenSessao(Usuario usuario, DateTimeOffset? agora = null)
    {
        var instante = agora ?? DateTimeOffset.UtcNow;
        var expiraEm = instante.Add(ValidadeTokenPadrao);

        var headerJson = """{"alg":"HS256","typ":"JWT"}""";
        var payload = new TokenPayload(
            Sub: usuario.Id,
            Usuario: usuario.NomeUsuario,
            Perfil: usuario.Perfil,
            Iat: instante.ToUnixTimeSeconds(),
            Exp: expiraEm.ToUnixTimeSeconds());

        var payloadJson = JsonSerializer.Serialize(payload);
        var segmentoHeader = CodificarBase64Url(Encoding.UTF8.GetBytes(headerJson));
        var segmentoPayload = CodificarBase64Url(Encoding.UTF8.GetBytes(payloadJson));
        var dadosAssinatura = $"{segmentoHeader}.{segmentoPayload}";
        var assinatura = AssinarHmacSha256(dadosAssinatura);

        return $"{dadosAssinatura}.{CodificarBase64Url(assinatura)}";
    }

    /// <summary>
    /// Valida a assinatura HMAC-SHA256 e a data de expiração do token e retorna o usuário autenticado.
    /// </summary>
    public static bool TentarValidarTokenSessao(string? token, out UsuarioAutenticado? usuario, DateTimeOffset? agora = null)
    {
        usuario = null;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var partes = token.Trim().Split('.');
        if (partes.Length != 3)
        {
            return false;
        }

        var dadosAssinatura = $"{partes[0]}.{partes[1]}";
        byte[] assinaturaRecebida;
        try
        {
            assinaturaRecebida = DecodificarBase64Url(partes[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var assinaturaEsperada = AssinarHmacSha256(dadosAssinatura);
        if (!CryptographicOperations.FixedTimeEquals(assinaturaRecebida, assinaturaEsperada))
        {
            return false;
        }

        try
        {
            var payloadBytes = DecodificarBase64Url(partes[1]);
            var payload = JsonSerializer.Deserialize<TokenPayload>(payloadBytes);
            if (payload is null || payload.Sub <= 0 || string.IsNullOrWhiteSpace(payload.Usuario))
            {
                return false;
            }

            var instanteAtual = (agora ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
            if (payload.Exp <= instanteAtual)
            {
                return false;
            }

            usuario = new UsuarioAutenticado(
                Id: payload.Sub,
                NomeUsuario: payload.Usuario,
                Perfil: string.IsNullOrWhiteSpace(payload.Perfil) ? "admin" : payload.Perfil);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extrai o usuário autenticado a partir do HttpContext (caso um Bearer token válido tenha sido enviado).
    /// </summary>
    public static UsuarioAutenticado? ObterUsuarioAutenticado(HttpContext? httpContext)
    {
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Items.TryGetValue(nameof(UsuarioAutenticado), out var item) &&
            item is UsuarioAutenticado autenticado)
        {
            return autenticado;
        }

        var idClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out var id) && id > 0)
        {
            var nome = httpContext.User.Identity?.Name ?? string.Empty;
            var perfil = httpContext.User.FindFirstValue(ClaimTypes.Role) ?? "admin";
            return new UsuarioAutenticado(id, nome, perfil);
        }

        return null;
    }

    private static byte[] AssinarHmacSha256(string dados)
    {
        var chave = Encoding.UTF8.GetBytes(_segredoJwt);
        return HMACSHA256.HashData(chave, Encoding.UTF8.GetBytes(dados));
    }

    private static string CodificarBase64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] DecodificarBase64Url(string entrada)
    {
        var base64 = entrada.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }

    private sealed record TokenPayload(
        [property: JsonPropertyName("sub")] int Sub,
        [property: JsonPropertyName("usuario")] string Usuario,
        [property: JsonPropertyName("perfil")] string Perfil,
        [property: JsonPropertyName("iat")] long Iat,
        [property: JsonPropertyName("exp")] long Exp);
}

public sealed record UsuarioAutenticado(int Id, string NomeUsuario, string Perfil)
{
    public bool EhAdmin => string.Equals(Perfil, "admin", StringComparison.OrdinalIgnoreCase);
}
