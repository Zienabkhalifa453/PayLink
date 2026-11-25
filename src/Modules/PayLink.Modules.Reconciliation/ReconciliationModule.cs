using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayLink.SharedKernel.Modules;

namespace PayLink.Modules.Reconciliation;

/// <summary>
/// Reconciliation module.
/// Responsibility: nightly comparison with biller settlement files and a difference report.
/// </summary>
public sealed class ReconciliationModule : IModule
{
    public string Name => "reconciliation";

    public string DisplayName => "Reconciliation";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2: register this module's services here.
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Phase 2: map endpoints under /api/reconciliation, tagged "Reconciliation" for OpenAPI.
        return endpoints;
    }
}
