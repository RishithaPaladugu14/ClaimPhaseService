using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClaimPhaseService.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClaimPhaseService.Api.Security;

public interface ITokenService
{
    TokenResponse? Issue(string clientId, string clientSecret);
}

public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _jwt;
    private readonly ApiClientOptions _clients;

    public TokenService(IOptions<JwtOptions> jwt, IOptions<ApiClientOptions> clients)
    {
        _jwt = jwt.Value;
        _clients = clients.Value;
    }

    public TokenResponse? Issue(string clientId, string clientSecret)
    {
        var client = _clients.Clients.FirstOrDefault(c => c.ClientId == clientId);
        if (client is null || !FixedTimeEquals(client.ClientSecret, clientSecret))
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, client.ClientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(client.Scopes.Select(s => new Claim("scope", s)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwt.ExpiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new TokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            _jwt.ExpiryMinutes * 60);
    }

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
