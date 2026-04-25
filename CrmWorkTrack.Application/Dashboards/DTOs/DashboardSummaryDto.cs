public record DashboardSummaryDto(
    int TotalCustomers,
    int TotalJobs,
    int OpenJobs,
    int InProgressJobs,
    int CompletedJobs,
    int CancelledJobs,
    int MyAssignedJobs,
    int OverdueJobs,
    int DueTodayJobs
);
