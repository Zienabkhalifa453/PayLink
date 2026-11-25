using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayLink.SharedKernel.Modules;

namespace PayLink.Modules.Payments;

/// <summary>
/// Payment Processing module.
/// Responsibility: payment life cycle (State), idempotency, orchestration of the pay-a-bill steps.
/// </summary>
public sealed class PaymentsModule : IModule
{
    public string Name => "payments";

    public string DisplayName => "Payment Processing";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2: register this module's services here.
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Phase 2: map endpoints under /api/payments, tagged "Payment Processing" for OpenAPI.
        return endpoints;
    }
}
