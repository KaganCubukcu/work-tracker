using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Auth;

public class TokenService(IConfiguration config)
{
    private readonly string _issuer = config["Jwt:Issuer"]!;
    private readonly string _audience = config["Jwt:Audience"]!;
    private readonly string _signingKey = config["Jwt:SigningKey"]!;
    private readonly int _accessTokenMinutes = config.GetValue<int>("Jwt:AccessTokenMinutes");
    private readonly int _refreshTokenDays = config.GetValue<int>("Jwt:RefreshTokenDays");

    public int RefreshTokenDays => _refreshTokenDays;

    public string CreateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public static string HashRefreshToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
