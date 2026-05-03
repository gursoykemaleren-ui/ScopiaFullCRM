using BCrypt.Net;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
	[Authorize]
	public async Task<IActionResult> GetUsers()
	{
		var users = await _db.Users
			.AsNoTracking()
			.OrderBy(u => u.Id)
			.Select(u => new
			{
				id = u.Id,
				userName = u.UserName,
				email = u.Email,
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
			.ToListAsync();

		return Ok(users);
	}

	// POST: api/admin/users
	[HttpPost]
	[Authorize]
	public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserRequest request)
	{
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

		var exists = await _db.Users.AnyAsync(u =>
			u.UserName == userName || u.Email == email);

		if (exists)
			return BadRequest(new { message = "UserName or Email already exists." });

		var user = new User
		{
			UserName = userName,
			Email = email,
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
			IsActive = true,
			CreatedAt = DateTime.UtcNow
		};

		_db.Users.Add(user);
		await _db.SaveChangesAsync();

		if (request.RoleId.HasValue)
		{
			var roleExists = await _db.Roles.AnyAsync(r => r.Id == request.RoleId.Value);

			if (!roleExists)
				return BadRequest(new { message = "Selected role does not exist." });

			_db.UserRoles.Add(new UserRole
			{
				UserId = user.Id,
				RoleId = request.RoleId.Value,
				IsActive = true,
				CreatedAt = DateTime.UtcNow
			});

			await _db.SaveChangesAsync();
		}

		return Ok(new
		{
			message = "User created successfully.",
			id = user.Id,
			userName = user.UserName,
			email = user.Email,
			isActive = user.IsActive
		});
	}

	// PUT: api/admin/users/{id}/status
	[HttpPut("{id:int}/status")]
	[Authorize]
	public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusRequest request)
	{
		if (request == null)
			return BadRequest(new { message = "Request body is required." });

		var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

		if (user == null)
			return NotFound(new { message = "User not found." });

		user.IsActive = request.IsActive;
		user.UpdatedAt = DateTime.UtcNow;

		await _db.SaveChangesAsync();

		return Ok(new
		{
			message = "User status updated successfully.",
			id = user.Id,
			isActive = user.IsActive
		});
	}

	// POST: api/admin/users/{id}/role
	[HttpPost("{id:int}/role")]
	[Authorize]
	public async Task<IActionResult> SetUserRole(int id, [FromBody] SetUserRoleRequest request)
	{
		if (request == null)
			return BadRequest(new { message = "Request body is required." });

		var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);

		if (user == null)
			return NotFound(new { message = "User not found." });

		var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);

		if (role == null)
			return BadRequest(new { message = "Role not found." });

		var currentUserRoles = await _db.UserRoles
			.Where(ur => ur.UserId == id)
			.ToListAsync();

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

		await _db.SaveChangesAsync();

		return Ok(new
		{
			message = "User role updated successfully.",
			userId = user.Id,
			roleId = role.Id,
			roleName = role.Name
		});
	}
}

public class CreateAdminUserRequest
{
	public string? UserName { get; set; }
	public string? Email { get; set; }
	public string? Password { get; set; }
	public int? RoleId { get; set; }
}

public class UpdateUserStatusRequest
{
	public bool IsActive { get; set; }
}

public class SetUserRoleRequest
{
	public int RoleId { get; set; }
}