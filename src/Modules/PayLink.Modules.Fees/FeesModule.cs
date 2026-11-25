using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayLink.SharedKernel.Modules;

namespace PayLink.Modules.Fees;

/// <summary>
/// Fees and Commissions module.
/// Responsibility: fee calculation (fixed, percentage, tiered, promotion) and agent commission.
/// </summary>
public sealed class FeesModule : IModule
{
    public string Name => "fees";

    public string DisplayName => "Fees and Commissions";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2: register this module's services here.
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Phase 2: map endpoints under /api/fees, tagged "Fees and Commissions" for OpenAPI.
        return endpoints;
    }
}
