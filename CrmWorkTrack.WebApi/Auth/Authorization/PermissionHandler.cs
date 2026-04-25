using Microsoft.AspNetCore.Authorization;

namespace CrmWorkTrack.WebApi.Auth.Authorization.Permissions;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims.Any(c =>
            c.Type == "perm" &&
            string.Equals(c.Value, requirement.PermissionCode, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}