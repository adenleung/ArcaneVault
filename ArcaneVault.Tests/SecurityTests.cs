/* Name: Aden Leung | Student Admin No.: 252744K | Tutorial Group: IT2814 */
using ArcaneVault.Api.Data;
using Microsoft.Extensions.Configuration;

namespace ArcaneVault.Tests;

public class SecurityTests
{
    [Fact]
    public void Passwords_AreHashedAndCanBeVerified()
    {
        var hash = PasswordSecurity.Hash("A-secure-password-123");
        Assert.NotEqual("A-secure-password-123", hash);
        Assert.True(PasswordSecurity.Verify("A-secure-password-123", hash));
        Assert.False(PasswordSecurity.Verify("wrong-password", hash));
    }

    [Fact]
    public void SignedToken_RejectsTampering()
    {
        var service = TokenService();
        var token = service.Create("collector", "User");
        Assert.Equal("collector", service.Validate(token)?.UserName);
        Assert.Null(service.Validate(token + "x"));
        Assert.Null(service.Validate("not-a-token"));
    }

    private static ApiTokenService TokenService() => new(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiTokenSecret"] = "a-test-secret-that-is-at-least-32-characters" })
        .Build());
}
