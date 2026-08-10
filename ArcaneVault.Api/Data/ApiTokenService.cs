/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace ArcaneVault.Api.Data;

public record ApiIdentity(string UserName, string RoleName);

public class ApiTokenService(IConfiguration configuration)
{
    private readonly byte[] _secret = Encoding.UTF8.GetBytes(configuration["ApiTokenSecret"]
        ?? throw new InvalidOperationException("ApiTokenSecret is not configured."));

    public string Create(string userName, string roleName)
    {
        // The nonce makes otherwise identical tokens unique; the HMAC protects all four payload fields.
        var expires = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);
        var unsigned = string.Join('.', Base64Url(Encoding.UTF8.GetBytes(userName)),
            Base64Url(Encoding.UTF8.GetBytes(roleName)), expires, Guid.NewGuid().ToString("N"));
        var signature = HMACSHA256.HashData(_secret, Encoding.UTF8.GetBytes(unsigned));
        return $"{unsigned}.{Base64Url(signature)}";
    }

    public ApiIdentity? Validate(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        var parts = token.Split('.');
        if (parts.Length != 5) return null;
        try
        {
            var unsigned = string.Join('.', parts.Take(4));
            var expected = HMACSHA256.HashData(_secret, Encoding.UTF8.GetBytes(unsigned));
            var supplied = FromBase64Url(parts[4]);
            // Fixed-time comparison avoids leaking how much of a signature matched.
            if (!CryptographicOperations.FixedTimeEquals(expected, supplied)) return null;
            if (!long.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var expiresAt)
                || expiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;
            var userName = Encoding.UTF8.GetString(FromBase64Url(parts[0]));
            var roleName = Encoding.UTF8.GetString(FromBase64Url(parts[1]));
            return string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(roleName)
                ? null : new ApiIdentity(userName, roleName);
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException or ArgumentException)
        {
            return null;
        }
    }

    private static string Base64Url(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] FromBase64Url(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded += (padded.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };
        return Convert.FromBase64String(padded);
    }
}
