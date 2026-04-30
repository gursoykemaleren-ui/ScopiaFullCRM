using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Api.DTOs;



namespace CrmWorkTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReturnRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReturnRequestsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/ReturnRequests
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _context.ReturnRequests
            .Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(data);
    }

    // POST: api/ReturnRequests
    [HttpPost]
    public async Task<IActionResult> CreateReturnRequest([FromBody] ReturnRequestCreateDto dto)
    {
        if (dto.CustomerId <= 0)
            return BadRequest("Geçerli bir müşteri seçilmelidir.");

        if (string.IsNullOrWhiteSpace(dto.Reason))
            return BadRequest("İade nedeni boş olamaz.");

        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == dto.CustomerId);

        if (!customerExists)
            return BadRequest("Seçilen müşteri bulunamadı.");

        var returnRequest = new ReturnRequest
        {
            CustomerId = dto.CustomerId,
            Reason = dto.Reason.Trim(),
            Description = dto.Description?.Trim(),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.ReturnRequests.Add(returnRequest);
        await _context.SaveChangesAsync();

        return Ok(returnRequest);
    }

    // PUT: api/ReturnRequests/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ReturnRequestStatusUpdateDto dto)
    {
        var item = await _context.ReturnRequests.FindAsync(id);

        if (item == null)
            return NotFound();

        if (dto.Status != "Approved" && dto.Status != "Rejected" && dto.Status != "Pending")
            return BadRequest("Geçersiz iade durumu.");

        item.Status = dto.Status;
        item.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(item);
    }
    public class ReturnRequestStatusUpdateDto
    {
        public string Status { get; set; } = string.Empty;
    }
}