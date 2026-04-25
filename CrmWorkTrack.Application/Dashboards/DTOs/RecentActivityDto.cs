namespace CrmWorkTrack.Application.Dashboard.DTOs;

public record RecentActivityDto(
    int ActivityId,
    int? JobId,
    string Type,
    string? Message,
    int? PerformedByUserId,
    DateTime CreatedAt
);
