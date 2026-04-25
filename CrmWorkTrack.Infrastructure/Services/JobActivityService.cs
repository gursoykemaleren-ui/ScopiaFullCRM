using CrmWorkTrack.Application.Interfaces;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;

namespace CrmWorkTrack.Infrastructure.Services;

public class JobActivityService : IJobActivityService
{
    private readonly AppDbContext _context;

    public JobActivityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        int jobId,
        string type,
        string? message = null,
        string? metaJson = null,
        int? performedByUserId = null,
        CancellationToken cancellationToken = default)
    {
        var activity = new JobActivity
        {
            JobId = jobId,
            Type = type,
            Message = message,
            MetaJson = metaJson,
            PerformedByUserId = performedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.JobActivities.Add(activity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
