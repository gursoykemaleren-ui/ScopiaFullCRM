using CrmWorkTrack.Infrastructure.Services;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly NotificationService _notificationService;

    public TicketsController(
        AppDbContext context,
        NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tickets = await _context.Tickets
            .Include(x => x.Customer)
            .Include(x => x.AssignedToUser)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                CustomerName = x.Customer.CompanyName,
                x.AssignedToUserId,
                AssignedToUserName = x.AssignedToUser != null ? x.AssignedToUser.UserName : null,
                x.Title,
                x.Description,
                x.Status,
                x.Priority,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ticket = await _context.Tickets
            .Include(x => x.Customer)
            .Include(x => x.AssignedToUser)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket == null)
            return NotFound("Ticket bulunamadı.");

        return Ok(new
        {
            ticket.Id,
            ticket.CustomerId,
            CustomerName = ticket.Customer.CompanyName,
            ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser != null ? ticket.AssignedToUser.UserName : null,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedAt,
            ticket.UpdatedAt
        });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var total = await _context.Tickets.CountAsync();
        var open = await _context.Tickets.CountAsync(x => x.Status == "Open");
        var inProgress = await _context.Tickets.CountAsync(x => x.Status == "InProgress");
        var resolved = await _context.Tickets.CountAsync(x => x.Status == "Resolved");
        var closed = await _context.Tickets.CountAsync(x => x.Status == "Closed");
        var critical = await _context.Tickets.CountAsync(x => x.Priority == "Critical");

        var latestTickets = await _context.Tickets
            .Include(x => x.Customer)
            .OrderByDescending(x => x.CreatedAt)
            .Take(5)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Status,
                x.Priority,
                CustomerName = x.Customer.CompanyName,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            total,
            open,
            inProgress,
            resolved,
            closed,
            critical,
            latestTickets
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId);

        if (customer == null)
            return BadRequest("Geçerli bir müşteri seçilmelidir.");

        var ticket = new Ticket
        {
            CustomerId = request.CustomerId,
            AssignedToUserId = request.AssignedToUserId,
            CreatedByUserId = request.CreatedByUserId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            "Yeni Destek Talebi",
            $"{customer.CompanyName} müşterisi için yeni destek talebi oluşturuldu: {ticket.Title}"
        );

        if (ticket.AssignedToUserId.HasValue)
        {
            await _notificationService.CreateAsync(
                "Destek Talebi Size Atandı",
                $"{customer.CompanyName} müşterisine ait destek talebi size atandı: {ticket.Title}",
                ticket.AssignedToUserId.Value
            );
        }

        return Ok(new
        {
            message = "Ticket başarıyla oluşturuldu.",
            ticket.Id
        });
    }

    [HttpGet("by-customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var tickets = await _context.Tickets
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Description,
                x.Status,
                x.Priority,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTicketRequest request)
    {
        var ticket = await _context.Tickets
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket == null)
            return NotFound("Ticket bulunamadı.");

        var oldAssignedToUserId = ticket.AssignedToUserId;

        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.Priority = request.Priority;
        ticket.Status = request.Status;
        ticket.AssignedToUserId = request.AssignedToUserId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (request.AssignedToUserId.HasValue &&
            oldAssignedToUserId != request.AssignedToUserId)
        {
            await _notificationService.CreateAsync(
                "Destek Talebi Size Atandı",
                $"{ticket.Customer.CompanyName} müşterisine ait destek talebi size atandı: {ticket.Title}",
                request.AssignedToUserId.Value
            );
        }

        return Ok("Ticket başarıyla güncellendi.");
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTicketStatusRequest request)
    {
        var ticket = await _context.Tickets
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket == null)
            return NotFound("Ticket bulunamadı.");

        ticket.Status = request.Status;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        if (ticket.AssignedToUserId.HasValue)
        {
            await _notificationService.CreateAsync(
                "Destek Talebi Durumu Güncellendi",
                $"{ticket.Customer.CompanyName} müşterisine ait destek talebinin durumu güncellendi: {ticket.Title}",
                ticket.AssignedToUserId.Value
            );
        }

        return Ok("Ticket durumu güncellendi.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);

        if (ticket == null)
            return NotFound("Ticket bulunamadı.");

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();

        return Ok("Ticket silindi.");
    }
}

public class CreateTicketRequest
{
    public int CustomerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? CreatedByUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
}

public class UpdateTicketRequest
{
    public int? AssignedToUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";
}

public class UpdateTicketStatusRequest
{
    public string Status { get; set; } = "Open";
}