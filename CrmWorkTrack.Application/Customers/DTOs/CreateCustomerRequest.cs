namespace CrmWorkTrack.Application.Customers.DTOs;

public record CreateCustomerRequest(
    string CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Notes
);
