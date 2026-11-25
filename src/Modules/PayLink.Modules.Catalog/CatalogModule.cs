using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayLink.SharedKernel.Modules;

namespace PayLink.Modules.Catalog;

/// <summary>
/// Biller Catalog module.
/// Responsibility: billers, services, required input fields, min and max amounts, active or disabled.
/// </summary>
public sealed class CatalogModule : IModule
{
    public string Name => "catalog";

    public string DisplayName => "Biller Catalog";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2: register this module's services here.
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Phase 2: map endpoints under /api/catalog, tagged "Biller Catalog" for OpenAPI.
        return endpoints;
    }
}
