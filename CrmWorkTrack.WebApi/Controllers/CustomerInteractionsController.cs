using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerInteractionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerInteractionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("by-customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(int customerId)
    {
        var list = await _context.CustomerInteractions
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.InteractionDate)
            .Select(x => new
            {
                x.Id,
                x.CustomerId,
                x.Title,
                x.Description,
                x.InteractionType,
                x.InteractionDate,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerInteractionRequest request)
    {
        var customerExists = await _context.Customers
            .AnyAsync(x => x.Id == request.CustomerId);

        if (!customerExists)
            return BadRequest("Geçerli bir müşteri bulunamadı.");

        var interaction = new CustomerInteraction
        {
            CustomerId = request.CustomerId,
            Title = request.Title,
            Description = request.Description,
            InteractionType = request.InteractionType,
            InteractionDate = request.InteractionDate ?? DateTime.Now,
            CreatedAt = DateTime.Now
        };

        _context.CustomerInteractions.Add(interaction);
        await _context.SaveChangesAsync();

        return Ok(interaction);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerInteractionRequest request)
    {
        var interaction = await _context.CustomerInteractions.FindAsync(id);

        if (interaction == null)
            return NotFound("Görüşme kaydı bulunamadı.");

        interaction.Title = request.Title;
        interaction.Description = request.Description;
        interaction.InteractionType = request.InteractionType;
        interaction.InteractionDate = request.InteractionDate ?? interaction.InteractionDate;

        await _context.SaveChangesAsync();

        return Ok(interaction);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var interaction = await _context.CustomerInteractions.FindAsync(id);

        if (interaction == null)
            return NotFound("Görüşme kaydı bulunamadı.");

        _context.CustomerInteractions.Remove(interaction);
        await _context.SaveChangesAsync();

        return Ok("Görüşme kaydı silindi.");
    }
}

public class CreateCustomerInteractionRequest
{
    public int CustomerId { get; set; }
    public string Title { get; set; } = "Görüşme Notu";
    public string Description { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "Note";
    public DateTime? InteractionDate { get; set; }
}

public class UpdateCustomerInteractionRequest
{
    public string Title { get; set; } = "Görüşme Notu";
    public string Description { get; set; } = string.Empty;
    public string InteractionType { get; set; } = "Note";
    public DateTime? InteractionDate { get; set; }
}