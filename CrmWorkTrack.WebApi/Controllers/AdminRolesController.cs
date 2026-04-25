using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.Domain.Entities;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/admin/roles")]
public class AdminRolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminRolesController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/admin/roles
    [HttpGet]
    [Authorize(Policy = "roles.manage")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                RoleId = r.Id,
                r.Name,
                r.Description,
                r.IsActive
            })
            .ToListAsync();

        return Ok(roles);
    }

    // POST: api/admin/roles
    [HttpPost]
    [Authorize(Policy = "roles.manage")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        request.Name = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Role name is required.");

        if (await _db.Roles.AnyAsync(r => r.Name == request.Name))
            return BadRequest("Role already exists.");

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return Ok(new { RoleId = role.Id, role.Name });
    }

    // GET: api/admin/roles/{id}/permissions
    [HttpGet("{id:int}/permissions")]
    [Authorize(Policy = "roles.manage")]
    public async Task<IActionResult> GetRolePermissions(int id)
    {
        var roleExists = await _db.Roles.AsNoTracking().AnyAsync(r => r.Id == id);
        if (!roleExists) return NotFound("Role not found.");

        var perms = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == id && rp.IsActive)
            .Join(_db.Permissions.AsNoTracking(),
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => new
                {
                    PermissionId = p.Id,
                    p.Code,
                    p.Description
                })
            .OrderBy(x => x.Code)
            .ToListAsync();

        return Ok(perms);
    }

    // POST: api/admin/roles/{id}/permissions
    [HttpPost("{id:int}/permissions")]
    [Authorize(Policy = "roles.manage")]
    public async Task<IActionResult> SetRolePermissions(int id, [FromBody] SetRolePermissionsRequest request)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (role is null) return NotFound("Role not found.");

        request.AddPermissionCodes ??= Array.Empty<string>();
        request.RemovePermissionCodes ??= Array.Empty<string>();

        var addCodes = request.AddPermissionCodes
            .Select(x => (x ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var removeCodes = request.RemovePermissionCodes
            .Select(x => (x ?? "").Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var allCodes = addCodes.Concat(removeCodes).Distinct().ToList();
        if (allCodes.Count == 0)
            return BadRequest("No permission codes provided.");

        // Check codes exist
        var permissions = await _db.Permissions
            .Where(p => allCodes.Contains(p.Code))
            .ToListAsync();

        var foundCodes = permissions.Select(p => p.Code).ToHashSet();
        var missing = allCodes.Where(c => !foundCodes.Contains(c)).ToList();
        if (missing.Count > 0)
            return BadRequest(new { message = "Unknown permission codes.", missing });

        // Preload existing RolePermissions for that role (avoid N+1)
        var permIds = permissions.Select(p => p.Id).ToList();

        var existingRps = await _db.RolePermissions
            .Where(rp => rp.RoleId == id && permIds.Contains(rp.PermissionId))
            .ToListAsync();

        // ADD (activate or insert)
        foreach (var code in addCodes)
        {
            var perm = permissions.First(p => p.Code == code);

            var existing = existingRps.FirstOrDefault(rp => rp.PermissionId == perm.Id);
            if (existing is null)
            {
                _db.RolePermissions.Add(new RolePermission
                {
                    RoleId = id,
                    PermissionId = perm.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        // REMOVE (soft disable)
        foreach (var code in removeCodes)
        {
            var perm = permissions.First(p => p.Code == code);

            var existing = existingRps.FirstOrDefault(rp => rp.PermissionId == perm.Id);
            if (existing is not null && existing.IsActive)
            {
                existing.IsActive = false;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "Role permissions updated." });
    }
}

// ----- DTOs -----
public class CreateRoleRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class SetRolePermissionsRequest
{
    public string[]? AddPermissionCodes { get; set; }
    public string[]? RemovePermissionCodes { get; set; }
}