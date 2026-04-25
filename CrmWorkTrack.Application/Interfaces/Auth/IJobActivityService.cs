namespace CrmWorkTrack.Application.Interfaces;

public interface IJobActivityService
{
    Task AddAsync(
        int jobId,
        string type,
        string? message = null,
        string? metaJson = null,
        int? performedByUserId = null,
        CancellationToken cancellationToken = default);
}
