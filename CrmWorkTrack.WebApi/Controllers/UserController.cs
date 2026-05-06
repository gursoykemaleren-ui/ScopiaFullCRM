using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.Email,
                x.DepartmentId,
                DepartmentName = x.Department != null ? x.Department.Name : null
            })
            .ToListAsync();

        return Ok(users);
    }
}