using CrmWorkTrack.Domain.Entities;

namespace CrmWorkTrack.Application.Interfaces.Repositories;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);

    Task<(IEnumerable<Customer> items, int totalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool? isActive,
        string? q,
        string? sortBy,
        string? sortDir,
        CancellationToken ct = default);
}