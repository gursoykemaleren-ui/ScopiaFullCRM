namespace CrmWorkTrack.Application.Dashboard.DTOs;

public record JobStatusDistributionDto(
    int Open,
    int InProgress,
    int Completed,
    int Cancelled
);
