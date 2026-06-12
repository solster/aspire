using System.Text;
using Aspire.Hosting.Azure;

namespace Solster.Aspire.Hosting.Azure.PostgreSQL.Tests;

public class AccessTokenClaimsTests
{
    [Theory]
    [InlineData("upn", "user@example.com")]
    [InlineData("preferred_username", "preferred@example.com")]
    public void GetUsername_ReturnsExpectedClaim(string claim, string expected)
    {
        var token = CreateJwt($$"""{ "{{claim}}": "{{expected}}" }""");

        var username = AccessTokenClaims.GetUsername(token);

        Assert.Equal(expected, username);
    }

    [Theory]
    [InlineData("")]
    [InlineData("opaque-token")]
    [InlineData("header.payload")]
    [InlineData("header.not-base64url.signature")]
    public void GetUsername_ReturnsNullForMalformedTokens(string token)
    {
        var username = AccessTokenClaims.GetUsername(token);

        Assert.Null(username);
    }

    [Fact]
    public void GetUsername_ReturnsNullWhenUsernameClaimsAreMissing()
    {
        var token = CreateJwt("""{ "aud": "https://ossrdbms-aad.database.windows.net" }""");

        var username = AccessTokenClaims.GetUsername(token);

        Assert.Null(username);
    }

    [Theory]
    [InlineData("""{ "upn": 123 }""")]
    [InlineData("""{ "preferred_username": true }""")]
    public void GetUsername_ReturnsNullWhenUsernameClaimsAreNotStrings(string payload)
    {
        var token = CreateJwt(payload);

        var username = AccessTokenClaims.GetUsername(token);

        Assert.Null(username);
    }

    private static string CreateJwt(string payload)
    {
        return $"{Base64UrlEncode("""{ "alg": "none" }""")}.{Base64UrlEncode(payload)}.";
    }

    private static string Base64UrlEncode(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
