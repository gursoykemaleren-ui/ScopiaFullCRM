using Microsoft.AspNetCore.Mvc;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/dev")]
public class DevController : ControllerBase
{
    [HttpGet("hash")]
    public IActionResult Hash()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
        return Ok(hash);
    }
}
