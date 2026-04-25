using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/_permtest")]
public class PermTestController : ControllerBase
{
    [Authorize(Policy = "perm:jobs.create")]
    [HttpGet("jobs-create")]
    public IActionResult JobsCreate()
    {
        return Ok("PERM OK");
    }
}
