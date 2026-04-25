using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Application.Features.CustomerContacts.Dtos;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.WebApi.Auth.Authorization.Permissions;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
public class CustomerContactsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerContactsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("api/customers/{customerId:int}/contacts")]
    [Authorize(Policy = Permissions.CustomerContacts.Read)]
    public async Task<ActionResult<IEnumerable<CustomerContactResponse>>> GetByCustomerId(int customerId)
    {
        var customerExists = await _context.Customers.AnyAsync(x => x.Id == customerId);
        if (!customerExists)
            return NotFound(new { message = "Customer not found." });

        var items = await _context.CustomerContacts
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.FullName)
            .Select(x => new CustomerContactResponse(
                x.Id,
                x.CustomerId,
                x.Customer.CompanyName,
                x.FullName,
                x.Title,
                x.Email,
                x.Phone,
                x.MobilePhone,
                x.Notes,
                x.IsPrimary,
                x.CreatedAt,
                x.UpdatedAt
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("api/customer-contacts/{id:int}")]
    [Authorize(Policy = Permissions.CustomerContacts.Read)]
    public async Task<ActionResult<CustomerContactResponse>> GetById(int id)
    {
        var item = await _context.CustomerContacts
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CustomerContactResponse(
                x.Id,
                x.CustomerId,
                x.Customer.CompanyName,
                x.FullName,
                x.Title,
                x.Email,
                x.Phone,
                x.MobilePhone,
                x.Notes,
                x.IsPrimary,
                x.CreatedAt,
                x.UpdatedAt
            ))
            .FirstOrDefaultAsync();

        if (item is null)
            return NotFound(new { message = "Customer contact not found." });

        return Ok(item);
    }

    [HttpPost("api/customers/{customerId:int}/contacts")]
    [Authorize(Policy = Permissions.CustomerContacts.Create)]
    public async Task<ActionResult<CustomerContactResponse>> Create(int customerId, [FromBody] CreateCustomerContactRequest request)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == customerId);

        if (customer is null)
            return NotFound(new { message = "Customer not found." });

        if (request.IsPrimary)
        {
            var currentPrimaryContacts = await _context.CustomerContacts
                .Where(x => x.CustomerId == customerId && x.IsPrimary)
                .ToListAsync();

            foreach (var contact in currentPrimaryContacts)
                contact.IsPrimary = false;
        }

        var entity = new CustomerContact
        {
            CustomerId = customerId,
            FullName = request.FullName,
            Title = request.Title,
            Email = request.Email,
            Phone = request.Phone,
            MobilePhone = request.MobilePhone,
            Notes = request.Notes,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerContacts.Add(entity);
        await _context.SaveChangesAsync();

        _context.JobActivities.Add(new JobActivity
        {
            JobId = null,
            Type = "contact_created",
            Message = $"Contact created: {entity.FullName}",
            PerformedByUserId = 1,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var activity = new JobActivity
        {
            JobId = null,
            Type = "contact_created",
            Message = $"User({User.Identity?.Name ?? "system"}) created contact: {entity.FullName}",
            PerformedByUserId = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.JobActivities.Add(activity);
        await _context.SaveChangesAsync();

        var response = new CustomerContactResponse(
            entity.Id,
            entity.CustomerId,
            customer.CompanyName,
            entity.FullName,
            entity.Title,
            entity.Email,
            entity.Phone,
            entity.MobilePhone,
            entity.Notes,
            entity.IsPrimary,
            entity.CreatedAt,
            entity.UpdatedAt
        );

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("api/customer-contacts/{id:int}")]
    [Authorize(Policy = Permissions.CustomerContacts.Update)]
    public async Task<ActionResult<CustomerContactResponse>> Update(int id, [FromBody] UpdateCustomerContactRequest request)
    {
        var entity = await _context.CustomerContacts
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return NotFound(new { message = "Customer contact not found." });

        if (request.IsPrimary)
        {
            var currentPrimaryContacts = await _context.CustomerContacts
                .Where(x => x.CustomerId == entity.CustomerId && x.IsPrimary && x.Id != entity.Id)
                .ToListAsync();

            foreach (var contact in currentPrimaryContacts)
                contact.IsPrimary = false;
        }

        entity.FullName = request.FullName;
        entity.Title = request.Title;
        entity.Email = request.Email;
        entity.Phone = request.Phone;
        entity.MobilePhone = request.MobilePhone;
        entity.Notes = request.Notes;
        entity.IsPrimary = request.IsPrimary;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new CustomerContactResponse(
            entity.Id,
            entity.CustomerId,
            entity.Customer.CompanyName,
            entity.FullName,
            entity.Title,
            entity.Email,
            entity.Phone,
            entity.MobilePhone,
            entity.Notes,
            entity.IsPrimary,
            entity.CreatedAt,
            entity.UpdatedAt
        );

        return Ok(response);
    }

    [HttpDelete("api/customer-contacts/{id:int}")]
    [Authorize(Policy = Permissions.CustomerContacts.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.CustomerContacts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
            return NotFound(new { message = "Customer contact not found." });

        _context.CustomerContacts.Remove(entity);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}