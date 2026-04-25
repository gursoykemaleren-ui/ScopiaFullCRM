namespace CrmWorkTrack.Application.Features.CustomerContacts.Dtos;

public record CustomerContactResponse(
    int Id,
    int CustomerId,
    string CustomerName,
    string FullName,
    string? Title,
    string? Email,
    string? Phone,
    string? MobilePhone,
    string? Notes,
    bool IsPrimary,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
