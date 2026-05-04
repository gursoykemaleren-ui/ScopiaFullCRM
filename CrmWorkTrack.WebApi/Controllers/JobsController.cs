using CrmWorkTrack.Infrastructure.Services;
using CrmWorkTrack.WebApi.Common.Extensions;
using CrmWorkTrack.Application.Common.Pagination;
using CrmWorkTrack.Application.Interfaces;
using CrmWorkTrack.Domain.Enums;
using CrmWorkTrack.Application.Jobs.DTOs;
using CrmWorkTrack.Application.Interfaces.Repositories;
using CrmWorkTrack.Domain.Constants;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.WebApi.Auth.Authorization.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJobRepository _jobRepository;
    private readonly IJobActivityService _jobActivityService;
    private readonly NotificationService _notificationService;

    public JobsController(
        AppDbContext db,
        IJobRepository jobRepository,
        IJobActivityService jobActivityService,
        NotificationService notificationService)
    {
        _db = db;
        _jobRepository = jobRepository;
        _jobActivityService = jobActivityService;
        _notificationService = notificationService;
    }

    private int? GetUserId()
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return int.TryParse(userIdStr, out var userId) ? userId : null;
    }


    // GET: api/jobs?page=1&pageSize=10&isCompleted=true&customerId=1&assignedToUserId=2&q=test
    [HttpGet]
    [Authorize(Policy = Permissions.Jobs.Read)]
    public async Task<IActionResult> GetPaged(
    int page = 1,
    int pageSize = 1000,
    bool? isCompleted = null,
    int? customerId = null,
    int? assignedToUserId = null,
    int? createdByUserId = null,
    string? priority = null,
    string? status = null,
    DateTime? dueDateFrom = null,
    DateTime? dueDateTo = null,
    string? q = null,
    CancellationToken ct = default)
    {
        var result = await _jobRepository.GetPagedAsync(
            page,
            pageSize,
            isCompleted,
            customerId,
            assignedToUserId,
            createdByUserId,
            priority,
            status,
            dueDateFrom,
            dueDateTo,
            q,
            ct);

        var items = result.Items.Select(x => new
        {
            x.Id,
            x.Title,
            x.Description,
            x.Status,
            x.IsCompleted,
            x.CustomerId,
            customerName = x.Customer != null ? x.Customer.CompanyName : null,
            x.CreatedByUserId,
            x.AssignedToUserId,
            x.Priority,
            x.DueDate,
            updatedAt= x.UpdatedAt.AsUtc(),
            createdAt = x.CreatedAt.AsUtc(),
        });

        return Ok(new
        {
            page,
            pageSize,
            totalCount = result.TotalCount,
            items
        });
    }

    // GET: api/jobs/my?page=1&pageSize=10&isCompleted=false&q=test
    [Authorize(Policy = Permissions.Jobs.Read)]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyJobs(
    [FromQuery] PagedRequest request,
    [FromQuery] bool? isCompleted = null,
    [FromQuery] string? q = null,
    CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var (items, totalCount) = await _jobRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            isCompleted: isCompleted,
            customerId: null,
            assignedToUserId: userId.Value,
            createdByUserId: null,
            q: q,
            ct: ct);

        var data = items.Select(x => new
        {
            x.Id,
            x.Title,
            x.Description,
            status = x.Status.ToString(),
            x.IsCompleted,
            x.CustomerId,
            customerName = x.Customer?.CompanyName,
            x.CreatedByUserId,
            x.AssignedToUserId,
            x.Priority,
            x.DueDate,
            CreatedAt = x.CreatedAt.AsUtc(),
            UpdatedAt = x.UpdatedAt.AsUtc()
        });

        var result = new PagedResult<object>(
            request.Page,
            request.PageSize,
            totalCount,
            data.ToList()
        );

        return Ok(result);
    }

    // GET: api/jobs/{id}
    [Authorize(Policy = Permissions.Jobs.Read)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var job = await _db.Jobs
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsActive)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                status = x.Status.ToString(),
                x.IsCompleted,
                x.CustomerId,
                customerName = x.Customer != null ? x.Customer.CompanyName : null,

                x.CreatedByUserId,
                createdByUserName = x.CreatedByUser != null
                    ? x.CreatedByUser.UserName
                    : null,

                x.AssignedToUserId,
                assignedToUserName = x.AssignedToUser != null
                    ? x.AssignedToUser.UserName
                    : null,

                x.Priority,
                x.DueDate,
                CreatedAt = x.CreatedAt.AsUtc(),
                UpdatedAt = x.UpdatedAt.AsUtc()
            })
            .FirstOrDefaultAsync(ct);

        if (job is null) return NotFound("Job not found.");

        return Ok(job);
    }

    // GET: api/jobs/5/activities-log
    [Authorize(Policy = Permissions.Jobs.Read)]
    [HttpGet("{id:int}/activities-log")]
    public async Task<IActionResult> GetActivitiesLog(int id, CancellationToken ct)
    {
        var jobExists = await _db.Jobs.AnyAsync(x => x.Id == id && x.IsActive, ct);
        if (!jobExists) return NotFound("Job not found.");

        var items = await _db.JobActivities
            .AsNoTracking()
            .Where(x => x.JobId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                activityId = x.Id,
                jobId = x.JobId,
                type = x.Type,
                message = x.Message,
                metaJson = x.MetaJson,
                performedByUserId = x.PerformedByUserId,
                createdAt = x.CreatedAt.AsUtc()
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("{id:int}/timeline")]
    [Authorize(Policy = Permissions.Jobs.Read)]
    public async Task<IActionResult> GetTimeline(int id, CancellationToken ct)
    {
        var jobExists = await _jobRepository.GetByIdAsync(id, ct);
        if (jobExists is null || !jobExists.IsActive)
            return NotFound();

        var timeline = await _db.JobActivities
            .AsNoTracking()
            .Where(x => x.JobId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.JobId,
                x.Type,
                x.Message,
                x.MetaJson,
                x.PerformedByUserId,
                createdAt = x.CreatedAt.AsUtc()
            })
            .ToListAsync(ct);

        return Ok(timeline);
    }

    // POST: api/jobs
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
            return Unauthorized();

        var job = new Job
        {
            CustomerId = request.CustomerId,
            Title = request.Title,
            Description = request.Description,
            AssignedToUserId = request.AssignedToUserId,
            CreatedByUserId = userId.Value,
            Priority = request.Priority ?? "Medium",
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsCompleted = false,
            Status = JobStatus.Open
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(
            job.AssignedToUserId != null ? "Yeni İş Atandı" : "Yeni İş Oluşturuldu",
            job.AssignedToUserId != null
                ? $"Size yeni bir iş atandı: {job.Title}"
                : $"Yeni bir iş oluşturuldu: {job.Title}",
            job.AssignedToUserId
        );

        return Ok(new
        {
            message = "İş başarıyla oluşturuldu.",
            job.Id
        });
    }

    // PUT: api/jobs/5
    [Authorize(Policy = Permissions.Jobs.Update)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJobRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var job = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
        if (job is null) return NotFound("Job not found.");

        var now = DateTime.UtcNow;
        var changed = false;

        string? statusChangeMessage = null;
        string? statusChangeMeta = null;

        if (!string.IsNullOrWhiteSpace(req.Title) && req.Title.Trim() != job.Title)
        {
            job.Title = req.Title.Trim();
            changed = true;
        }

        if (req.Description != job.Description)
        {
            job.Description = req.Description;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(req.Priority) && req.Priority.Trim() != job.Priority)
        {
            job.Priority = req.Priority.Trim();
            changed = true;
        }

        if (req.DueDate != job.DueDate)
        {
            job.DueDate = req.DueDate;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            var parsedStatus = ParseStatus(req.Status);
            if (!parsedStatus.HasValue)
                return BadRequest("Invalid status. Use open, inprogress, completed or cancelled.");

            if (parsedStatus.Value != job.Status)
            {
                var from = job.Status;
                var to = parsedStatus.Value;

                job.Status = to;
                job.IsCompleted = to == JobStatus.Completed;
                changed = true;

                statusChangeMessage = $"User({userId}) changed status: {from} -> {to}";
                statusChangeMeta = JsonSerializer.Serialize(new
                {
                    from = from.ToString(),
                    to = to.ToString()
                });
            }
        }

        if (!changed) return NoContent();

        job.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        if (statusChangeMessage is not null)
        {
            await _jobActivityService.AddAsync(
                job.Id,
                JobActivityTypes.StatusChanged,
                statusChangeMessage,
                statusChangeMeta,
                userId,
                ct);
        }

        await _jobActivityService.AddAsync(
            job.Id,
            JobActivityTypes.Updated,
            $"User({userId}) updated job.",
            null,
            userId,
            ct);

        return NoContent();
    }

    // DELETE: api/jobs/5
    [Authorize(Policy = Permissions.Jobs.Delete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var job = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
        if (job is null) return NotFound("Job not found.");

        var now = DateTime.UtcNow;

        job.IsActive = false;
        job.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        await _jobActivityService.AddAsync(
            job.Id,
            JobActivityTypes.Deleted,
            $"User({userId}) deleted job.",
            null,
            userId,
            ct);

        return NoContent();
    }

    // POST: api/jobs/5/assign
    [Authorize(Policy = Permissions.Jobs.Assign)]
    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignJobRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var job = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
        if (job is null) return NotFound("Job not found.");

        var userExists = await _db.Users.AnyAsync(u => u.Id == req.AssignedToUserId, ct);
        if (!userExists) return BadRequest("Target user not found.");

        var now = DateTime.UtcNow;

        var from = job.AssignedToUserId;
        job.AssignedToUserId = req.AssignedToUserId;
        job.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        await _jobActivityService.AddAsync(
      job.Id,
    JobActivityTypes.Assigned,
    $"User({userId}) assigned job: {from?.ToString() ?? "null"} -> {req.AssignedToUserId}",
    JsonSerializer.Serialize(new
    {
        from,
        to = req.AssignedToUserId
    }),
    userId,
    ct);

        await _notificationService.CreateJobAssignedAsync(
            req.AssignedToUserId,
            job.Title
        );

        return NoContent();
    }



    // POST: api/jobs/5/status
    [Authorize(Policy = Permissions.Jobs.UpdateStatus)]
    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateJobStatusRequest req, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var job = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
        if (job is null) return NotFound("Job not found.");

        var parsedStatus = ParseStatus(req.Status);
        if (!parsedStatus.HasValue)
            return BadRequest("Invalid status. Use open, inprogress, completed or cancelled.");

        if (job.Status == parsedStatus.Value)
            return NoContent();

        var now = DateTime.UtcNow;
        var from = job.Status;
        var to = parsedStatus.Value;

        job.Status = to;
        job.IsCompleted = to == JobStatus.Completed;
        job.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);

        await _jobActivityService.AddAsync(
            job.Id,
            JobActivityTypes.StatusChanged,
            $"User({userId}) changed status: {from} -> {to}",
            $"{{\"from\":\"{from}\",\"to\":\"{to}\"}}",
            userId,
            ct);

        return NoContent();
    }

    private static JobStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        var s = status.Trim().ToLowerInvariant();

        return s switch
        {
            "open" => JobStatus.Open,
            "inprogress" => JobStatus.InProgress,
            "completed" => JobStatus.Completed,
            "cancelled" => JobStatus.Cancelled,
            _ => null
        };
    }
}