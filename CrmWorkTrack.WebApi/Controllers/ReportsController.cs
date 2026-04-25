using CrmWorkTrack.Application.Reports.DTOs;
using CrmWorkTrack.Domain.Enums;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db)
    {
        _db = db;
    }
    //
    [Authorize]
    [HttpGet("customer-job-summary")]
    public async Task<IActionResult> GetCustomerJobSummary(CancellationToken ct)
    {
        var items = await _db.Customers
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CustomerJobSummaryDto(
                c.Id,
                c.CompanyName,
                c.Jobs.Count(j => j.IsActive),
                c.Jobs.Count(j => j.IsActive && j.Status == JobStatus.Open),
                c.Jobs.Count(j => j.IsActive && j.Status == JobStatus.InProgress),
                c.Jobs.Count(j => j.IsActive && j.Status == JobStatus.Completed),
                c.Jobs.Count(j => j.IsActive && j.Status == JobStatus.Cancelled)
            ))
            .OrderByDescending(x => x.TotalJobs)
            .ToListAsync(ct);

        return Ok(items);
    }
    [Authorize]
    [HttpGet("user-performance-summary")]
    public async Task<IActionResult> GetUserPerformanceSummary(CancellationToken ct)
    {
        var users = await _db.Users
            .AsNoTracking()
            .Select(u => new
            {
                u.Id,
                u.UserName
            })
            .ToListAsync(ct);

        var jobs = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.IsActive && j.AssignedToUserId != null)
            .Select(j => new
            {
                AssignedToUserId = j.AssignedToUserId!.Value,
                j.Status
            })
            .ToListAsync(ct);

        var items = users
            .Select(u =>
            {
                var userJobs = jobs.Where(j => j.AssignedToUserId == u.Id).ToList();

                var openJobs = userJobs.Count(j => j.Status == JobStatus.Open);
                var inProgressJobs = userJobs.Count(j => j.Status == JobStatus.InProgress);
                var completedJobs = userJobs.Count(j => j.Status == JobStatus.Completed);
                var cancelledJobs = userJobs.Count(j => j.Status == JobStatus.Cancelled);

                var assignedJobs = openJobs + inProgressJobs + completedJobs + cancelledJobs;

                return new UserPerformanceSummaryDto(
                    u.Id,
                    u.UserName,
                    assignedJobs,
                    openJobs,
                    inProgressJobs,
                    completedJobs,
                    cancelledJobs
                );
            })
            .OrderByDescending(x => x.AssignedJobs)
            .ToList();

        return Ok(items);
    }

    // GET: api/reports/jobs-summary?startDate=2026-03-01&endDate=2026-03-31
    [Authorize]
    [HttpGet("jobs-summary")]
    public async Task<IActionResult> GetJobsSummary(
    [FromQuery] DateTime? startDate,
    [FromQuery] DateTime? endDate,
    CancellationToken ct)
    {
        if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
            return BadRequest("startDate cannot be greater than endDate.");

        var query = _db.Jobs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .AsQueryable();

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(x => x.CreatedAt >= start);
        }

        if (endDate.HasValue)
        {
            var endExclusive = endDate.Value.Date.AddDays(1);
            query = query.Where(x => x.CreatedAt < endExclusive);
        }

        var totalJobs = await query.CountAsync(ct);
        var openJobs = await query.CountAsync(x => x.Status == JobStatus.Open, ct);
        var inProgressJobs = await query.CountAsync(x => x.Status == JobStatus.InProgress, ct);
        var completedJobs = await query.CountAsync(x => x.Status == JobStatus.Completed, ct);
        var cancelledJobs = await query.CountAsync(x => x.Status == JobStatus.Cancelled, ct);

        var dto = new JobReportSummaryDto(
            startDate,
            endDate,
            totalJobs,
            openJobs,
            inProgressJobs,
            completedJobs,
            cancelledJobs
        );

        return Ok(dto);
    }
}

