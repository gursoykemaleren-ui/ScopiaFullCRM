using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttachmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AttachmentsController(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("job/{jobId:int}")]
    public async Task<IActionResult> GetByJob(int jobId)
    {
        var files = await _db.Attachments
            .Where(x => x.JobId == jobId)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new
            {
                x.Id,
                x.OriginalFileName,
                x.ContentType,
                x.Size,
                x.UploadedAt
            })
            .ToListAsync();

        return Ok(files);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var files = await _db.Attachments
            .Include(x => x.Job)
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new
            {
                x.Id,
                x.OriginalFileName,
                x.ContentType,
                x.Size,
                x.UploadedAt,
                x.JobId,
                JobTitle = x.Job != null ? x.Job.Title : null
            })
            .ToListAsync();

        return Ok(files);
    }


    [HttpPost("job/{jobId:int}")]
    public async Task<IActionResult> UploadForJob(int jobId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya seçilmelidir.");
        var maxFileSize = 5 * 1024 * 1024; // 5 MB

        if (file.Length > maxFileSize)
            return BadRequest("Dosya boyutu en fazla 5 MB olabilir.");

        var allowedTypes = new[]
        {
    "application/pdf",
    "image/jpeg",
    "image/png",
    "image/webp",
    "application/msword",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
    "application/vnd.ms-excel",
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
};

        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest("Bu dosya türüne izin verilmiyor.");

        var jobExists = await _db.Jobs.AnyAsync(x => x.Id == jobId);
        if (!jobExists)
            return NotFound("Job bulunamadı.");

        var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads", "jobs", jobId.ToString());
        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsRoot, storedFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var attachment = new Attachment
        {
            JobId = jobId,
            OriginalFileName = file.FileName,
            FileName = storedFileName,
            ContentType = file.ContentType,
            Size = file.Length,
            FilePath = filePath,
            UploadedAt = DateTime.UtcNow
        };

        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Dosya başarıyla yüklendi.",
            attachment.Id
        });
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var attachment = await _db.Attachments.FindAsync(id);

        if (attachment == null)
            return NotFound("Dosya bulunamadı.");

        if (!System.IO.File.Exists(attachment.FilePath))
            return NotFound("Fiziksel dosya bulunamadı.");

        var bytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath);

        return File(bytes, attachment.ContentType, attachment.OriginalFileName);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var attachment = await _db.Attachments.FindAsync(id);

        if (attachment == null)
            return NotFound("Dosya bulunamadı.");

        if (System.IO.File.Exists(attachment.FilePath))
            System.IO.File.Delete(attachment.FilePath);

        _db.Attachments.Remove(attachment);
        await _db.SaveChangesAsync();

        return Ok("Dosya silindi.");
    }
}