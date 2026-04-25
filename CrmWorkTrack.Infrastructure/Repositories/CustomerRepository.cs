using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Application.Interfaces.Repositories;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;

namespace CrmWorkTrack.Infrastructure.Repositories;

public class CustomerRepository : EfRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email, ct);
    }
    public async Task<(IEnumerable<Customer> items, int totalCount)> GetPagedAsync(
    int page,
    int pageSize,
    bool? isActive,
    string? q,
    string? sortBy,
    string? sortDir,
    CancellationToken ct = default)
    {
        var query = _context.Customers.AsQueryable();

        // Filter - IsActive
        if (isActive.HasValue)
            query = query.Where(x => x.IsActive == isActive.Value);

        // Search
        if (!string.IsNullOrWhiteSpace(q))
        {
            var keyword = q.ToLower();

            query = query.Where(x =>
                x.CompanyName.ToLower().Contains(keyword) ||
                (x.ContactName ?? string.Empty).ToLower().Contains(keyword) ||
                (x.Email ?? string.Empty).ToLower().Contains(keyword));
        }

        // Sorting
        query = (sortBy?.ToLower(), sortDir?.ToLower()) switch
        {
            ("companyname", "asc") => query.OrderBy(x => x.CompanyName),
            ("companyname", "desc") => query.OrderByDescending(x => x.CompanyName),

            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            ("createdat", "desc") => query.OrderByDescending(x => x.CreatedAt),

            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }









    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _context.Customers
            .AnyAsync(x => x.Email == email, ct);
    }
}