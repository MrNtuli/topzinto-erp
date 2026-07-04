using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Topzinto.Erp.Api.Authorization;

public sealed class ModuleAccessRequirement(string module) : IAuthorizationRequirement
{
    public string Module { get; } = module;
}

public sealed class ModuleAccessHandler : AuthorizationHandler<ModuleAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ModuleAccessRequirement requirement)
    {
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        if (ModuleRoleMatrix.CanAccess(requirement.Module, roles))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
