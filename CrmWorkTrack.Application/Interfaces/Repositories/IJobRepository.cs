using CrmWorkTrack.Domain.Entities;

namespace CrmWorkTrack.Application.Interfaces.Repositories;

public interface IJobRepository : IGenericRepository<Job>
{
    Task<Job?> GetByIdWithRelationsAsync(int id, CancellationToken ct = default);

    Task<(List<Job> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool? isCompleted = null,
        int? customerId = null,
        int? assignedToUserId = null,
        int? createdByUserId = null,
        int? createdDepartmentId = null,
        int? assignedDepartmentId = null,
        string? priority = null,
        string? status = null,
        DateTime? dueDateFrom = null,
        DateTime? dueDateTo = null,
        string? q = null,
        CancellationToken ct = default);
}

