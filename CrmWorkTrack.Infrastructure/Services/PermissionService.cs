using CrmWorkTrack.Application.Interfaces.Auth;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.Infrastructure.Auth;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _db;

    public PermissionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken ct = default)
    {
        permissionCode = (permissionCode ?? string.Empty).Trim();
        if (permissionCode.Length == 0) return false;

        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.RolePermissions,
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp.PermissionId)
            .Join(_db.Permissions,
                permissionId => permissionId,
                p => p.Id,
                (permissionId, p) => p.Code)
            .AnyAsync(code => code == permissionCode, ct);
    }
}