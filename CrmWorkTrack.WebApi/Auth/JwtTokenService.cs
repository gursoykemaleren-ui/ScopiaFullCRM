using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CrmWorkTrack.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CrmWorkTrack.WebApi.Auth;

public interface IJwtTokenService
{
    (string token, DateTime expiresAt) CreateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public JwtTokenService(IConfiguration configuration, IWebHostEnvironment env)
    {
        _configuration = configuration;
        _env = env;
    }

    public (string token, DateTime expiresAt) CreateToken(User user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var jwt = _configuration.GetSection("Jwt");

        var keyStr = jwt["Key"];
        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];

        if (string.IsNullOrWhiteSpace(keyStr))
            throw new InvalidOperationException("Jwt:Key missing.");
        if (string.IsNullOrWhiteSpace(issuer))
            throw new InvalidOperationException("Jwt:Issuer missing.");
        if (string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Jwt:Audience missing.");

        // ExpiresMinutes config yoksa:
        // - Development: 480 dk (8 saat)
        // - Prod: 60 dk
        var expiresMinutesStr = jwt["ExpiresMinutes"];
        var expiresMinutes =
            int.TryParse(expiresMinutesStr, out var m) && m > 0
                ? m
                : (_env.IsDevelopment() ? 480 : 60);

        var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("userName", user.UserName),
        };
        foreach (var r in roles.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
            claims.Add(new Claim(ClaimTypes.Role, r));

        foreach (var p in permissions.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
        {
            var normalized = p.StartsWith("perm:", StringComparison.OrdinalIgnoreCase)
                ? p.Substring("perm:".Length)
                : p;

            claims.Add(new Claim("perm", normalized));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}