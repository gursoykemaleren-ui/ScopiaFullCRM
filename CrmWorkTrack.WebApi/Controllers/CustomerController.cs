using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmWorkTrack.Application.Customers.DTOs;
using CrmWorkTrack.Application.Interfaces.Repositories;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.WebApi.Auth.Authorization.Permissions;
using CrmWorkTrack.WebApi.Common.Extensions;

namespace CrmWorkTrack.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Customers.Read)]
    public async Task<IActionResult> GetAll(
    [FromQuery] CustomerQueryRequest request,
    CancellationToken ct)
    {
        var (items, totalCount) = await _customerRepository.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.IsActive,
            request.Q,
            request.SortBy,
            request.SortDir,
            ct);

        var result = items.Select(x => new CustomerDto(
            x.Id,
            x.CompanyName,
            x.ContactName,
            x.Email,
            x.Phone,
            x.Address,
            x.City,
            x.Notes,
            x.IsActive,
            x.CreatedAt.AsUtc(),
            x.UpdatedAt.AsUtc()
        ));

        return Ok(new
        {
            page = request.Page,
            pageSize = request.PageSize,
            totalCount,
            items = result
        });
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Permissions.Customers.Read)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(id, ct);

        if (customer is null || !customer.IsActive)
            return NotFound();

        var result = new CustomerDto(
            customer.Id,
            customer.CompanyName,
            customer.ContactName,
            customer.Email,
            customer.Phone,
            customer.Address,
            customer.City,
            customer.Notes,
            customer.IsActive,
            customer.CreatedAt.AsUtc(),
            customer.UpdatedAt.AsUtc()
        );

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Customers.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _customerRepository.EmailExistsAsync(request.Email, ct);
            if (emailExists)
                return Conflict(new { message = "A customer with this email already exists." });
        }

        var customer = new Customer
        {
            CompanyName = request.CompanyName,
            ContactName = request.ContactName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer, ct);
        await _customerRepository.SaveChangesAsync(ct);

        var result = new CustomerDto(
            customer.Id,
            customer.CompanyName,
            customer.ContactName,
            customer.Email,
            customer.Phone,
            customer.Address,
            customer.City,
            customer.Notes,
            customer.IsActive,
            customer.CreatedAt.AsUtc(),
            customer.UpdatedAt.AsUtc()
        );

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.Customers.Update)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(id, ct);

        if (customer is null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email, ct);
            if (existingCustomer is not null && existingCustomer.Id != id)
                return Conflict(new { message = "A customer with this email already exists." });
        }

        customer.CompanyName = request.CompanyName;
        customer.ContactName = request.ContactName;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.Address = request.Address;
        customer.City = request.City;
        customer.Notes = request.Notes;
        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.Customers.Delete)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(id, ct);

        if (customer is null || !customer.IsActive)
            return NotFound();

        customer.IsActive = false;
        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync(ct);

        return NoContent();
    }
    [HttpPost("{id:int}/restore")]
    [Authorize(Policy = Permissions.Customers.Update)]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
    {
        var customer = await _customerRepository.GetByIdAsync(id, ct);

        if (customer is null)
            return NotFound();

        if (customer.IsActive)
            return BadRequest(new { message = "Customer is already active." });

        customer.IsActive = true;
        customer.UpdatedAt = DateTime.UtcNow;

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync(ct);

        return NoContent();
    }
}