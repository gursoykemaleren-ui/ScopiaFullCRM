using CrmWorkTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.Infrastructure.Persistence.Seed;

public static class AppDbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();
        //department
        var requiredDepartments = new[]
{
    "Proje",
    "İK",
    "Satış",
    "Destek",
    "Operasyon"
};

        var existingDepartmentNames = await context.Departments
            .Select(x => x.Name)
            .ToListAsync();

        var missingDepartments = requiredDepartments
            .Except(existingDepartmentNames, StringComparer.OrdinalIgnoreCase)
            .Select(name => new Department
            {
                Name = name,
                Description = $"{name} departmanı",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (missingDepartments.Count > 0)
        {
            context.Departments.AddRange(missingDepartments);
            await context.SaveChangesAsync();
        }

        var permissionCodes = new[]
        {
            "users.read","users.create","users.update","users.delete",
            "roles.manage","permissions.manage",

            "customers.read","customers.create","customers.update","customers.delete",

            "customers.contacts.read","customers.contacts.create","customers.contacts.update","customers.contacts.delete",

            "jobs.read","jobs.create","jobs.update","jobs.delete",
            "jobs.assign","jobs.updateStatus",
            "jobs.comments.read","jobs.comments.create",
            "jobs.activities.read"
        };

        // 1) Permissions - eksik olanları ekle
        var existingPermissionCodes = await context.Permissions
            .Select(x => x.Code)
            .ToListAsync();

        var missingPermissions = permissionCodes
            .Except(existingPermissionCodes, StringComparer.OrdinalIgnoreCase)
            .Select(code => new Permission
            {
                Code = code,
                Description = code
            })
            .ToList();

        if (missingPermissions.Count > 0)
        {
            context.Permissions.AddRange(missingPermissions);
            await context.SaveChangesAsync();
        }

        // 2) Roles - eksik olanları ekle
        var requiredRoles = new[] { "Admin", "Manager", "Employee" };

        var existingRoleNames = await context.Roles
            .Select(x => x.Name)
            .ToListAsync();

        var missingRoles = requiredRoles
            .Except(existingRoleNames, StringComparer.OrdinalIgnoreCase)
            .Select(roleName => new Role
            {
                Name = roleName
            })
            .ToList();

        if (missingRoles.Count > 0)
        {
            context.Roles.AddRange(missingRoles);
            await context.SaveChangesAsync();
        }

        // 3) Admin rolüne tüm permission'ları bağla (eksik olanları)
        var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
        var allPermissions = await context.Permissions.ToListAsync();

        var existingAdminPermissionIds = await context.RolePermissions
            .Where(rp => rp.RoleId == adminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var missingAdminRolePermissions = allPermissions
            .Where(p => !existingAdminPermissionIds.Contains(p.Id))
            .Select(p => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = p.Id
            })
            .ToList();

        if (missingAdminRolePermissions.Count > 0)
        {
            context.RolePermissions.AddRange(missingAdminRolePermissions);
            await context.SaveChangesAsync();
        }

        // 4) Admin user - yoksa oluştur, varsa dokunma
        var adminUser = await context.Users
            .FirstOrDefaultAsync(u => u.UserName == "admin");

        if (adminUser is null)
        {
            adminUser = new User
            {
                UserName = "admin",
                PasswordHash = "123456" // mevcut sistemine göre geçici/demo
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // 5) Admin user'a Admin rolünü bağla - eksikse ekle
        var adminUserHasAdminRole = await context.UserRoles
            .AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);

        if (!adminUserHasAdminRole)
        {
            context.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });

            await context.SaveChangesAsync();
        }
    }
}