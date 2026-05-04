using CrmWorkTrack.Application.Dashboard.DTOs;
using CrmWorkTrack.Domain.Enums;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    private int? GetUserId()
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return int.TryParse(userIdStr, out var userId) ? userId : null;
    }
    [Authorize]
    [HttpGet("job-status-distribution")]
    public async Task<IActionResult> GetJobStatusDistribution(CancellationToken ct)
    {
        var open = await _db.Jobs.CountAsync(x => x.IsActive && x.Status == JobStatus.Open, ct);
        var inProgress = await _db.Jobs.CountAsync(x => x.IsActive && x.Status == JobStatus.InProgress, ct);
        var completed = await _db.Jobs.CountAsync(x => x.IsActive && x.Status == JobStatus.Completed, ct);
        var cancelled = await _db.Jobs.CountAsync(x => x.IsActive && x.Status == JobStatus.Cancelled, ct);

        var dto = new JobStatusDistributionDto(
            open,
            inProgress,
            completed,
            cancelled
        );

        return Ok(dto);
    }

    [Authorize]
    [HttpGet("recent-activities")]
    public async Task<IActionResult> GetRecentActivities(CancellationToken ct)
    {
        var items = await _db.JobActivities
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new RecentActivityDto(
                x.Id,
                x.JobId,
                x.Type,
                x.Message,
                x.PerformedByUserId,
                x.CreatedAt
            ))
            .ToListAsync(ct);

        return Ok(items);
    }
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var totalCustomers = await _db.Customers.CountAsync(x => x.IsActive, ct);
        var totalJobs = await _db.Jobs.CountAsync(x => x.IsActive, ct);
        var openJobs = await _db.Jobs.CountAsync(x => x.IsActive && x.Status == JobStatus.Open, ct);
        var inProgressJobs = await _db.Jobs.CountAsync(x => x.IsActive && x.Status == JobStatus.InProgress, ct);
        var completedJobs = await _db.Jobs.CountAsync(x => x.IsActive && x.Status == JobStatus.Completed, ct);
        var cancelledJobs = await _db.Jobs.CountAsync(x => x.IsActive && x.Status == JobStatus.Cancelled, ct);
        var myAssignedJobs = await _db.Jobs.CountAsync(x => x.IsActive && x.AssignedToUserId == userId, ct);

        var overdueJobs = await _db.Jobs.CountAsync(
        x => x.IsActive && x.DueDate != null && x.DueDate < DateTime.UtcNow && !x.IsCompleted,ct);
        var dueTodayJobs = await _db.Jobs.CountAsync(
            x => x.IsActive && x.DueDate != null && x.DueDate.Value.Date == DateTime.UtcNow.Date,
            ct);

        var dto = new DashboardSummaryDto(
                  totalCustomers,
                  totalJobs,
                  openJobs,
                  inProgressJobs,
                  completedJobs,
                  cancelledJobs,
                  myAssignedJobs,
                  overdueJobs,
                  dueTodayJobs
                  );

        return Ok(dto);
    }
    [Authorize]
    [HttpGet("jobs-by-priority")]
    public async Task<IActionResult> GetJobsByPriority(CancellationToken ct)
    {
        var high = await _db.Jobs
            .Where(x => x.IsActive && x.Priority == "High")
            .CountAsync(ct);

        var medium = await _db.Jobs
            .Where(x => x.IsActive && x.Priority == "Medium")
            .CountAsync(ct);

        var low = await _db.Jobs
            .Where(x => x.IsActive && x.Priority == "Low")
            .CountAsync(ct);

        return Ok(new
        {
            high,
            medium,
            low
        });
    }
    [Authorize]
    [HttpGet("jobs-per-customer")]
    public async Task<IActionResult> GetJobsPerCustomer(CancellationToken ct)
    {
        var items = await _db.Jobs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .GroupBy(x => new { x.CustomerId, CustomerName = x.Customer.CompanyName })
            .Select(g => new
            {
                customerId = g.Key.CustomerId,
                customerName = g.Key.CustomerName,
                jobCount = g.Count()
            })
            .OrderByDescending(x => x.jobCount)
            .ThenBy(x => x.customerName)
            .ToListAsync(ct);

        return Ok(items);
    }
    [Authorize]
    [HttpGet("due-soon")]
    public async Task<IActionResult> GetDueSoonJobs(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var next7Days = now.AddDays(7);

        var count = await _db.Jobs
            .CountAsync(x =>
                x.IsActive &&
                !x.IsCompleted &&
                x.DueDate != null &&
                x.DueDate >= now &&
                x.DueDate <= next7Days,
                ct);

        return Ok(new
        {
            count
        });
    }
    [Authorize]
    [HttpGet("recent-jobs")]
    public async Task<IActionResult> GetRecentJobs(CancellationToken ct)
    {
        var jobs = await _db.Jobs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new
            {
                x.Id,
                x.Title,
                status = x.Status.ToString(),
                x.Priority,
                x.CustomerId,
                customerName = x.Customer.CompanyName,
                x.DueDate,
                x.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(jobs);
    }
    [Authorize]
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueJobs(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var count = await _db.Jobs.CountAsync(x =>
            x.IsActive &&
            !x.IsCompleted &&
            x.DueDate != null &&
            x.DueDate < now,
            ct);

        return Ok(new
        {
            count
        });
    }
    [Authorize]
    [HttpGet("unassigned")]
    public async Task<IActionResult> GetUnassignedJobs(CancellationToken ct)
    {
        var count = await _db.Jobs.CountAsync(x =>
            x.IsActive &&
            x.AssignedToUserId == null,
            ct);

        return Ok(new
        {
            count
        });
    }
}
