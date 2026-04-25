using Microsoft.AspNetCore.Mvc;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("home")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Welcome" });
    }
}
