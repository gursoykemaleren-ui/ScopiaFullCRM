using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    [Authorize]
    public async Task<IActionResult> GetRoles()
    {
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
            .ToListAsync();

        return Ok(roles);
    }

    // POST: api/admin/roles
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateRole([FromBody] CreateAdminRoleRequest request)
    {
        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        var name = request.Name?.Trim();
        var description = request.Description?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Role name is required." });

        var exists = await _db.Roles.AnyAsync(r => r.Name == name);

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
        await _db.SaveChangesAsync();

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