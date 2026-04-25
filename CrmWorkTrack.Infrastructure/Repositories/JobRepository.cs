using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Application.Interfaces.Repositories;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Domain.Enums;
using CrmWorkTrack.Infrastructure.Persistence;

namespace CrmWorkTrack.Infrastructure.Repositories;

public class JobRepository : EfRepository<Job>, IJobRepository
{
    public JobRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Job?> GetByIdWithRelationsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Jobs
            .Include(x => x.Customer)
            .Include(x => x.CreatedByUser)
            .Include(x => x.AssignedToUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, ct);
    }

    public async Task<(List<Job> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool? isCompleted = null,
        int? customerId = null,
        int? assignedToUserId = null,
        int? createdByUserId = null,
        string? priority = null,
        string? status = null,
        DateTime? dueDateFrom = null,
        DateTime? dueDateTo = null,
        string? q = null,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = _context.Jobs
            .Where(x => x.IsActive)
            .AsQueryable();

        if (isCompleted.HasValue)
            query = query.Where(x => x.IsCompleted == isCompleted.Value);

        if (customerId.HasValue)
            query = query.Where(x => x.CustomerId == customerId.Value);

        if (assignedToUserId.HasValue)
            query = query.Where(x => x.AssignedToUserId == assignedToUserId.Value);

        if (createdByUserId.HasValue)
            query = query.Where(x => x.CreatedByUserId == createdByUserId.Value);

        if (!string.IsNullOrWhiteSpace(priority))
        {
            priority = priority.Trim().ToLower();
            query = query.Where(x =>
                x.Priority != null &&
                x.Priority.ToLower() == priority);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<JobStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(x => x.Status == parsedStatus);
            }
            else
            {
                return (new List<Job>(), 0);
            }
        }

        if (dueDateFrom.HasValue)
            query = query.Where(x => x.DueDate.HasValue && x.DueDate.Value >= dueDateFrom.Value);

        if (dueDateTo.HasValue)
            query = query.Where(x => x.DueDate.HasValue && x.DueDate.Value <= dueDateTo.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x =>
                x.Title.Contains(q) ||
                (x.Description != null && x.Description.Contains(q)));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(x => x.Customer)
            .Include(x => x.CreatedByUser)
            .Include(x => x.AssignedToUser)
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}