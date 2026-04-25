using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Application.Interfaces.Repositories;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;

namespace CrmWorkTrack.Infrastructure.Repositories;

public class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(x => x.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Email == email, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _context.Users.AnyAsync(x => x.Email == email, ct);
}