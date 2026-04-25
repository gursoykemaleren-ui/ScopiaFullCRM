namespace CrmWorkTrack.Application.Customers.DTOs;

public record CustomerDto(
    int Id,
    string CompanyName,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Notes,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
