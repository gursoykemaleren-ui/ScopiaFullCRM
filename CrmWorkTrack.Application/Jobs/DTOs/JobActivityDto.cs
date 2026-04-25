namespace CrmWorkTrack.Application.Features.Jobs.Dtos;

public record JobActivityDto(
    int Id,
    int JobId,
    string Type,
    string? Message,
    string? MetaJson,
    int? PerformedByUserId,
    DateTime CreatedAt
);
