using Microsoft.EntityFrameworkCore;
using CrmWorkTrack.Infrastructure.Persistence;

namespace CrmWorkTrack.WebApi.Auth;

public interface IUserAuthQuery
{
    Task<List<string>> GetUserPermissionsAsync(int userId);
}

public class UserAuthQuery : IUserAuthQuery
{
    private readonly AppDbContext _context;

    public UserAuthQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetUserPermissionsAsync(int userId)
    {
        return await (
            from ur in _context.UserRoles
            join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
            join p in _context.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId
            select p.Code
        ).Distinct().ToListAsync();
    }
}

