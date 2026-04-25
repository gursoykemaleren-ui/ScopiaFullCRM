using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/jobs/{jobId:int}/activities")]
public class JobActivitiesController : ControllerBase
{
    private readonly AppDbContext _db;

    public JobActivitiesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Policy = "perm:jobs.activities.read")]
    public async Task<IActionResult> Get(int jobId, CancellationToken ct)
    {
        var items = await _db.Set<JobActivity>()
            .Where(x => x.JobId == jobId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.JobId,
                x.Type,
                x.Message,
                x.MetaJson,
                x.PerformedByUserId,
                x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(items);
    }
}