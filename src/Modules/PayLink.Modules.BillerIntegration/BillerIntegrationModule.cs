using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayLink.SharedKernel.Modules;

namespace PayLink.Modules.BillerIntegration;

/// <summary>
/// Biller Integration module.
/// Responsibility: one adapter per biller hiding its protocol; timeout, retry and callback handling.
/// </summary>
public sealed class BillerIntegrationModule : IModule
{
    public string Name => "biller-integration";

    public string DisplayName => "Biller Integration";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2: register this module's services here.
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Phase 2: map endpoints under /api/biller-integration, tagged "Biller Integration" for OpenAPI.
        return endpoints;
    }
}
