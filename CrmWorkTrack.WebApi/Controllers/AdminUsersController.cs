using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.Domain.Entities;
using BCrypt.Net;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminUsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/admin/users
    [HttpGet]
    [Authorize(Policy = "users.read")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                UserId = u.Id,
                u.UserName,
                u.Email,
                u.IsActive,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // POST: api/admin/users
    [HttpPost]
    [Authorize(Policy = "users.create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        request.UserName = request.UserName.Trim();
        request.Email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest("UserName is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");

        var exists = await _db.Users.AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email);
        if (exists)
            return BadRequest("UserName or Email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { UserId = user.Id, user.UserName });
    }

    // POST: api/admin/users/{id}/roles
    [HttpPost("{id:int}/roles")]
    [Authorize(Policy = "roles.manage")]
    public async Task<IActionResult> SetUserRoles(int id, [FromBody] SetUserRolesRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound("User not found.");

        request.AddRoleIds ??= Array.Empty<int>();
        request.RemoveRoleIds ??= Array.Empty<int>();

        var addIds = request.AddRoleIds.Distinct().ToList();
        var removeIds = request.RemoveRoleIds.Distinct().ToList();

        var allRoleIds = addIds.Concat(removeIds).Distinct().ToList();
        if (allRoleIds.Count == 0)
            return BadRequest("No role ids provided.");

        var roles = await _db.Roles
            .Where(r => allRoleIds.Contains(r.Id))
            .ToListAsync();

        var foundIds = roles.Select(r => r.Id).ToHashSet();
        var missing = allRoleIds.Where(x => !foundIds.Contains(x)).ToList();
        if (missing.Count > 0)
            return BadRequest(new { message = "Unknown role ids.", missing });

        // preload existing userroles
        var existing = await _db.UserRoles
            .Where(ur => ur.UserId == id && allRoleIds.Contains(ur.RoleId))
            .ToListAsync();

        // ADD (activate or insert)
        foreach (var roleId in addIds)
        {
            var ur = existing.FirstOrDefault(x => x.RoleId == roleId);
            if (ur is null)
            {
                _db.UserRoles.Add(new UserRole
                {
                    UserId = id,
                    RoleId = roleId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (!ur.IsActive)
            {
                ur.IsActive = true;
                ur.UpdatedAt = DateTime.UtcNow;
            }
        }

        // REMOVE (soft disable)
        foreach (var roleId in removeIds)
        {
            var ur = existing.FirstOrDefault(x => x.RoleId == roleId);
            if (ur is not null && ur.IsActive)
            {
                ur.IsActive = false;
                ur.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "User roles updated." });
    }
}

// DTOs
public class CreateUserRequest
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class SetUserRolesRequest
{
    public int[]? AddRoleIds { get; set; }
    public int[]? RemoveRoleIds { get; set; }
}