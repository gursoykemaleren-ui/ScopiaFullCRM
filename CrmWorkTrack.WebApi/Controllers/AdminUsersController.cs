using BCrypt.Net;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminUsersController(AppDbContext db)
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
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        if (!await IsCurrentUserAdminAsync(ct))
            return Forbid();

        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new
            {
                id = u.Id,
                userName = u.UserName,
                email = u.Email,
                departmentId = u.DepartmentId,
                departmentName = u.Department != null ? u.Department.Name : null,
                isActive = u.IsActive,
                createdAt = u.CreatedAt,

                roles = _db.UserRoles
                    .Where(ur => ur.UserId == u.Id && ur.IsActive)
                    .Join(
                        _db.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new
                        {
                            id = r.Id,
                            name = r.Name
                        }
                    )
                    .ToList()
            })
            .ToListAsync(ct);

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken ct)
    {
        if (!await IsCurrentUserAdminAsync(ct))
            return Forbid();

        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        var userName = request.UserName?.Trim();
        var email = request.Email?.Trim();
        var password = request.Password?.Trim();

        if (string.IsNullOrWhiteSpace(userName))
            return BadRequest(new { message = "UserName is required." });

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required." });

        if (string.IsNullOrWhiteSpace(password))
            return BadRequest(new { message = "Password is required." });

        if (password.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters." });

        var exists = await _db.Users.AnyAsync(
            u => u.UserName == userName || u.Email == email,
            ct);

        if (exists)
            return BadRequest(new { message = "UserName or Email already exists." });

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _db.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value && d.IsActive, ct);

            if (!departmentExists)
                return BadRequest(new { message = "Selected department does not exist." });
        }

        var user = new User
        {
            UserName = userName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DepartmentId = request.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        if (request.RoleId.HasValue)
        {
            var roleExists = await _db.Roles.AnyAsync(
                r => r.Id == request.RoleId.Value,
                ct);

            if (!roleExists)
                return BadRequest(new { message = "Selected role does not exist." });

            _db.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = request.RoleId.Value,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(ct);
        }

        return Ok(new
        {
            message = "User created successfully.",
            id = user.Id,
            userName = user.UserName,
            email = user.Email,
            departmentId = user.DepartmentId,
            isActive = user.IsActive
        });
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        int id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken ct)
    {
        if (!await IsCurrentUserAdminAsync(ct))
            return Forbid();

        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
            return NotFound(new { message = "User not found." });

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "User status updated successfully.",
            id = user.Id,
            isActive = user.IsActive
        });
    }

    [HttpPost("{id:int}/role")]
    public async Task<IActionResult> SetUserRole(
        int id,
        [FromBody] SetUserRoleRequest request,
        CancellationToken ct)
    {
        if (!await IsCurrentUserAdminAsync(ct))
            return Forbid();

        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
            return NotFound(new { message = "User not found." });

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, ct);

        if (role == null)
            return BadRequest(new { message = "Role not found." });

        var currentUserRoles = await _db.UserRoles
            .Where(ur => ur.UserId == id)
            .ToListAsync(ct);

        foreach (var userRole in currentUserRoles)
        {
            userRole.IsActive = false;
            userRole.UpdatedAt = DateTime.UtcNow;
        }

        var existingRole = currentUserRoles.FirstOrDefault(ur => ur.RoleId == request.RoleId);

        if (existingRole == null)
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId = id,
                RoleId = request.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingRole.IsActive = true;
            existingRole.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "User role updated successfully.",
            userId = user.Id,
            roleId = role.Id,
            roleName = role.Name
        });
    }

    [HttpPost("{id:int}/department")]
    public async Task<IActionResult> SetUserDepartment(
        int id,
        [FromBody] SetUserDepartmentRequest request,
        CancellationToken ct)
    {
        if (!await IsCurrentUserAdminAsync(ct))
            return Forbid();

        if (request == null)
            return BadRequest(new { message = "Request body is required." });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user == null)
            return NotFound(new { message = "User not found." });

        if (request.DepartmentId.HasValue)
        {
            var departmentExists = await _db.Departments
                .AnyAsync(d => d.Id == request.DepartmentId.Value && d.IsActive, ct);

            if (!departmentExists)
                return BadRequest(new { message = "Selected department does not exist." });
        }

        user.DepartmentId = request.DepartmentId;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "User department updated successfully.",
            userId = user.Id,
            departmentId = user.DepartmentId
        });
    }
}

public class CreateAdminUserRequest
{
    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public int? RoleId { get; set; }

    public int? DepartmentId { get; set; }
}

public class UpdateUserStatusRequest
{
    public bool IsActive { get; set; }
}

public class SetUserRoleRequest
{
    public int RoleId { get; set; }
}

public class SetUserDepartmentRequest
{
    public int? DepartmentId { get; set; }
}