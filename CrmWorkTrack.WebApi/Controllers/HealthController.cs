using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.Application.Interfaces.Repositories;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IUserRepository _userRepository;

    public HealthController(AppDbContext context, IUserRepository userRepository)
    {
        _context = context;
        _userRepository = userRepository;
    }

    [HttpGet]
    public IActionResult Ping()
    {
        return Ok(new { status = "API is running" });
    }

    [HttpGet("db")]
    public async Task<IActionResult> Db()
    {
        var canConnect = await _context.Database.CanConnectAsync();
        return Ok(new { db = canConnect });
    }

/* test*/

    [HttpGet("user-exists")]
    public async Task<IActionResult> UserExists([FromQuery] string email, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("email zorunlu");

        var exists = await _userRepository.EmailExistsAsync(email, ct);
        return Ok(new { email, exists });
    }
}
