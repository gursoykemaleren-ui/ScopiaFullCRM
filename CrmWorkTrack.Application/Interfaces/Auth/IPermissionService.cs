namespace CrmWorkTrack.Application.Interfaces.Auth;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken ct = default);
}