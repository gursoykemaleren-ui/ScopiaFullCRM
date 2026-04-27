using CrmWorkTrack.Infrastructure.Services;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.WebApi.Auth.Authorization.Permissions;
using CrmWorkTrack.WebApi.Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/jobs/{jobId:int}/comments")]
public class JobCommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly NotificationService _notificationService;

    public JobCommentsController(
        AppDbContext db,
        NotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    [HttpGet]
    [Authorize(Policy = "perm:jobs.comments.read")]
    public async Task<IActionResult> Get(int jobId, CancellationToken ct)
    {
        var items = await _db.Set<JobComment>()
            .Where(x => x.JobId == jobId && x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.JobId,
                x.Text,
                x.CreatedByUserId,
                CreatedAt = x.CreatedAt.AsUtc(),
                UpdatedAt = x.UpdatedAt.AsUtc(),
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpPost]
    [Authorize(Policy = "perm:jobs.comments.create")]
    public async Task<IActionResult> Add(int jobId, [FromBody] AddJobCommentRequest req, CancellationToken ct)
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var job = await _db.Set<Job>()
            .FirstOrDefaultAsync(j => j.Id == jobId, ct);

        if (job == null)
            return NotFound("Job not found.");

        var text = (req.Text ?? "").Trim();

        if (text.Length == 0)
            return BadRequest("Text is required.");

        if (text.Length > 2000)
            return BadRequest("Text is too long.");

        var now = DateTime.UtcNow;

        var comment = new JobComment
        {
            JobId = jobId,
            CreatedByUserId = userId,
            Text = text,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = null
        };

        _db.Set<JobComment>().Add(comment);

        _db.Set<JobActivity>().Add(new JobActivity
        {
            JobId = jobId,
            Type = "comment_added",
            Message = $"User({userId}) added a comment.",
            MetaJson = null,
            PerformedByUserId = userId,
            CreatedAt = now
        });

        await _db.SaveChangesAsync(ct);

        await _notificationService.CreateAsync(
            "Yeni İş Yorumu Eklendi",
            $"'{job.Title}' işine yeni bir yorum eklendi."
        );

        return Ok(new { comment.Id });
    }
    [HttpPut("{id:int}")]
    [Authorize(Policy = "perm:jobs.comments.create")]
    public async Task<IActionResult> Update(int id, [FromBody] string message, CancellationToken ct)
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var comment = await _db.JobComments.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

        if (comment is null)
            return NotFound();

        var newText = (message ?? "").Trim();
        if (newText.Length == 0)
            return BadRequest("Text is required.");
        if (newText.Length > 2000)
            return BadRequest("Text is too long.");

        var oldText = comment.Text;
        comment.Text = newText;
        comment.UpdatedAt = DateTime.UtcNow;

        _db.Set<JobActivity>().Add(new JobActivity
        {
            JobId = comment.JobId,
            Type = "comment_updated",
            Message = $"User({userId}) updated comment #{comment.Id}.",
            MetaJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                commentId = comment.Id,
                oldText,
                newText
            }),
            PerformedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
    [HttpDelete("{id:int}")]
    [Authorize(Policy = "perm:jobs.comments.create")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var userIdStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var comment = await _db.JobComments.FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);

        if (comment is null)
            return NotFound();

        var deletedText = comment.Text;
        var jobId = comment.JobId;
        var now = DateTime.UtcNow;

        _db.Set<JobActivity>().Add(new JobActivity
        {
            JobId = jobId,
            Type = "comment_deleted",
            Message = $"User({userId}) deleted comment #{comment.Id}.",
            MetaJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                commentId = comment.Id,
                text = deletedText
            }),
            PerformedByUserId = userId,
            CreatedAt = now
        });

        _db.JobComments.Remove(comment);

        await _db.SaveChangesAsync(ct);

        return NoContent();
    }
}
public class AddJobCommentRequest
{
    public string Text { get; set; } = string.Empty;
}