using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/admin/roles")]
[Authorize]
public class AdminRolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminRolesController(AppDbContext db)
    {
        _db = db;
    }

    private int? GetUserId()
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return int.TryParse(userIdStr, out var userId) ? userId : null;
    }

    private async Task<bool> IsCurrentUserAdminAsync(CancellationToken ct = default)
    {
        var userId = GetUserId();

        if (userId is null)
            return false;

        return await _db.UserRoles
            .Where(ur => ur.UserId == userId.Value && ur.IsActive)
            .Join(
                _db.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name
            )
            .AnyAsync(role => role == "Admin", ct);
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        if (!await IsCurrentUserAdminAsync(ct))
            return Forbid();

        var roles = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .Select(r => new
            {
                id = r.Id,
                name = r.Name,
                description = r.Description,
                isActive = r.IsActive,
                createdAt = r.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateAdminRoleRequest request,
        CancellationToken ct)
    {
        if (!await IsCurrentUserAdminAsync(ct))
            return Forbid();

        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        var name = request.Name?.Trim();
        var description = request.Description?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Role name is required." });

        var exists = await _db.Roles.AnyAsync(r => r.Name == name, ct);

        if (exists)
            return BadRequest(new { message = "Role already exists." });

        var role = new Role
        {
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "Role created successfully.",
            id = role.Id,
            name = role.Name,
            description = role.Description,
            isActive = role.IsActive
        });
    }
}

public class CreateAdminRoleRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}