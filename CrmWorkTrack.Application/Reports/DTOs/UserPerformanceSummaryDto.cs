namespace CrmWorkTrack.Application.Reports.DTOs;

public record UserPerformanceSummaryDto(
    int UserId,
    string UserName,
    int AssignedJobs,
    int OpenJobs,
    int InProgressJobs,
    int CompletedJobs,
    int CancelledJobs
);
