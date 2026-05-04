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

    public async Task<bool> HasPermissionAsync(
        int userId,
        string permissionCode,
        CancellationToken ct = default)
    {
        permissionCode = (permissionCode ?? string.Empty).Trim();

        if (permissionCode.Length == 0)
            return false;

        var roleNames = await _db.UserRoles
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .Join(
                _db.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name
            )
            .ToListAsync(ct);

        var isAdmin = roleNames.Any(role =>
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase));

        if (isAdmin)
            return true;

        if (IsUserManagementPermission(permissionCode))
            return false;

        return true;
    }

    private static bool IsUserManagementPermission(string permissionCode)
    {
        var code = permissionCode.Trim().ToLowerInvariant();

        return
            code.StartsWith("users.") ||
            code.StartsWith("roles.") ||
            code.StartsWith("permissions.") ||
            code.StartsWith("admin.") ||
            code.Contains("users.") ||
            code.Contains("roles.") ||
            code.Contains("permissions.") ||
            code.Contains("admin.");
    }
}