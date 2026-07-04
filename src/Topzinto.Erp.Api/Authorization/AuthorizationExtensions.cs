using Microsoft.AspNetCore.Authorization;

namespace Topzinto.Erp.Api.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddErpModuleAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, ModuleAccessHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var module in ErpModules.All)
            {
                options.AddPolicy(ErpModules.Policy(module), policy =>
                    policy.Requirements.Add(new ModuleAccessRequirement(module)));
            }
        });

        return services;
    }
}
