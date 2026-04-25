using System.Security.Claims;
using CrmWorkTrack.Application.Interfaces.Auth.DTOs;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.WebApi.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IUserAuthQuery _userAuthQuery;

    public AuthController(AppDbContext db, IJwtTokenService jwt, IUserAuthQuery userAuthQuery)
    {
        _db = db;
        _jwt = jwt;
        _userAuthQuery = userAuthQuery;
    }

    public sealed class ResetPasswordRequest
    {
        public string UserName { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        var userName =
            User.FindFirstValue("userName")
            ?? User.Identity?.Name;

        var roles = User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
        var perms = User.FindAll("perm").Select(x => x.Value).ToList();

        return Ok(new
        {
            userId,
            userName,
            roles,
            perms,
            allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(x => x.UserName == request.UserName);

        if (user is null)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        if (!user.IsActive)
            return Forbid();

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .Distinct()
            .ToList();

        var perms = await _userAuthQuery.GetUserPermissionsAsync(user.Id);

        var (accessToken, expiresAt) = _jwt.CreateToken(user, roles, perms);

        var refreshToken = RefreshTokenHelpers.GenerateToken();
        var (hash, salt) = RefreshTokenHelpers.HashToken(refreshToken);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            TokenSalt = salt,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        });

        await _db.SaveChangesAsync();

        return Ok(new LoginResponse(
      accessToken,
      expiresAt,
      refreshToken,
      user.Id.ToString(),
      user.UserName ?? string.Empty,
      roles,
      perms.ToList()
     ));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        var activeTokens = await _db.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        RefreshToken? matchedToken = null;

        foreach (var token in activeTokens)
        {
            if (RefreshTokenHelpers.Verify(request.RefreshToken, token.TokenSalt, token.TokenHash))
            {
                matchedToken = token;
                break;
            }
        }

        if (matchedToken is null)
            return Unauthorized("Geçersiz refresh token.");

        if (!matchedToken.User.IsActive)
            return Forbid();

        matchedToken.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var user = matchedToken.User;

        var roles = await _db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .Distinct()
            .ToListAsync();

        var perms = await _userAuthQuery.GetUserPermissionsAsync(user.Id);

        var (accessToken, expiresAt) = _jwt.CreateToken(user, roles, perms);

        var newRefreshToken = RefreshTokenHelpers.GenerateToken();
        var (hash, salt) = RefreshTokenHelpers.HashToken(newRefreshToken);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = hash,
            TokenSalt = salt,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(14)
        });

        await _db.SaveChangesAsync();

        return Ok(new LoginResponse(
        accessToken,
        expiresAt,
        newRefreshToken,
        user.Id.ToString(),
        user.UserName ?? string.Empty,
        roles,
        perms.ToList()
        ));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest("UserName zorunlu.");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("NewPassword zorunlu.");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName);
        if (user is null)
            return NotFound("User bulunamadı");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();

        return Ok("Şifre güncellendi");
    }
}