using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayLink.SharedKernel.Modules;

namespace PayLink.Modules.Notifications;

/// <summary>
/// Notifications module.
/// Responsibility: SMS, email and push messages with retry; message templates.
/// </summary>
public sealed class NotificationsModule : IModule
{
    public string Name => "notifications";

    public string DisplayName => "Notifications";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2: register this module's services here.
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Phase 2: map endpoints under /api/notifications, tagged "Notifications" for OpenAPI.
        return endpoints;
    }
}
