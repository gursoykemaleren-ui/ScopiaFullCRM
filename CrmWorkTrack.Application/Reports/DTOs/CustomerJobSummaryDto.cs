namespace CrmWorkTrack.Application.Reports.DTOs;

public record CustomerJobSummaryDto(
    int CustomerId,
    string CustomerName,
    int TotalJobs,
    int OpenJobs,
    int InProgressJobs,
    int CompletedJobs,
    int CancelledJobs
);
