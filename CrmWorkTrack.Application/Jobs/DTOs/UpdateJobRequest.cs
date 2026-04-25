namespace CrmWorkTrack.Application.Jobs.DTOs;

public record UpdateJobRequest(
    string? Title,
    string? Description,
    string? Status,
    string? Priority,
    DateTime? DueDate
);