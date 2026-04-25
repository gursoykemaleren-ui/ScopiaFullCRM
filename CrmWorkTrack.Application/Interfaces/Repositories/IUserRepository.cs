using CrmWorkTrack.Domain.Entities;

namespace CrmWorkTrack.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    // Auth tarafında gerekecek:
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
