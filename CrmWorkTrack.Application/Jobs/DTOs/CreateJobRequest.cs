namespace CrmWorkTrack.Application.Jobs.DTOs;

public record CreateJobRequest(
    int CustomerId,
    string Title,
    string? Description,
    string Status,
    string? Priority,
    DateTime? DueDate,
    int? AssignedToUserId   //  EKLEDİK
);