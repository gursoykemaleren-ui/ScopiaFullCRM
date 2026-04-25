namespace CrmWorkTrack.Application.Features.CustomerContacts.Dtos;

public record CreateCustomerContactRequest(
    string FullName,
    string? Title,
    string? Email,
    string? Phone,
    string? MobilePhone,
    string? Notes,
    bool IsPrimary
);
