namespace CrmWorkTrack.Application.Features.CustomerContacts.Dtos;

public record UpdateCustomerContactRequest(
    string FullName,
    string? Title,
    string? Email,
    string? Phone,
    string? MobilePhone,
    string? Notes,
    bool IsPrimary
);
