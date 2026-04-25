using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CrmWorkTrack.WebApi.Auth.Authorization.Permissions;

public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public const string Prefix = "perm:";

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionCode = policyName.Substring(Prefix.Length).Trim();

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permissionCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return base.GetPolicyAsync(policyName);
    }
}