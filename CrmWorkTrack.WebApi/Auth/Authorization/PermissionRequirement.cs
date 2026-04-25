using Microsoft.AspNetCore.Authorization;

namespace CrmWorkTrack.WebApi.Auth.Authorization.Permissions ;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionCode { get; }

    public PermissionRequirement(string permissioncode)
    {
        PermissionCode = permissioncode;
    }
}

