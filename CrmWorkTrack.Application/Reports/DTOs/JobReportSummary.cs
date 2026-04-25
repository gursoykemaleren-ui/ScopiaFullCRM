namespace CrmWorkTrack.Application.Reports.DTOs;

public record JobReportSummaryDto(
    DateTime? StartDate,
    DateTime? EndDate,
    int TotalJobs,
    int OpenJobs,
    int InProgressJobs,
    int CompletedJobs,
    int CancelledJobs
);
