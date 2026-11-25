using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayLink.SharedKernel.Modules;

namespace PayLink.Modules.Fraud;

/// <summary>
/// Fraud Checks module.
/// Responsibility: daily limit, velocity and blocked-account checks as a chain of rules.
/// </summary>
public sealed class FraudModule : IModule
{
    public string Name => "fraud";

    public string DisplayName => "Fraud Checks";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2: register this module's services here.
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Phase 2: map endpoints under /api/fraud, tagged "Fraud Checks" for OpenAPI.
        return endpoints;
    }
}
