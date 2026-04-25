using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Infrastructure.Persistence;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/admin/permissions")]
public class AdminPermissionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminPermissionsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/admin/permissions
    [HttpGet]
    [Authorize(Policy = "permissions.manage")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                PermissionId = x.Id,
                x.Code,
                x.Description
            })
            .ToListAsync();

        return Ok(items);
    }
}
