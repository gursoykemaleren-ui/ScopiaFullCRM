using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                userCount = x.Users.Count(u => u.IsActive)
            })
            .ToListAsync(ct);

        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var department = await _context.Departments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                users = x.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.UserName)
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (department == null)
            return NotFound("Departman bulunamadı.");

        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken ct)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Departman adı zorunludur.");

        var exists = await _context.Departments
            .AnyAsync(x => x.Name == name, ct);

        if (exists)
            return BadRequest("Bu departman zaten mevcut.");

        var department = new Department
        {
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "Departman başarıyla oluşturuldu.",
            department.Id,
            department.Name
        });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken ct)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (department == null)
            return NotFound("Departman bulunamadı.");

        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Departman adı zorunludur.");

        var exists = await _context.Departments
            .AnyAsync(x => x.Id != id && x.Name == name, ct);

        if (exists)
            return BadRequest("Bu departman adı başka bir kayıtta kullanılıyor.");

        department.Name = name;
        department.Description = request.Description?.Trim();
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            message = "Departman başarıyla güncellendi.",
            department.Id,
            department.Name
        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (department == null)
            return NotFound("Departman bulunamadı.");

        department.IsActive = false;
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return Ok("Departman pasife alındı.");
    }
}

public class CreateDepartmentRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}

public class UpdateDepartmentRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }
}