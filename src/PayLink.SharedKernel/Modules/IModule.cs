using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PayLink.SharedKernel.Modules;

/// <summary>
/// Contract every PayLink module implements so the API host can plug it in
/// without knowing its internals (modular monolith).
/// </summary>
public interface IModule
{
    /// <summary>Short, URL-safe name, e.g. "accounts". Used as the route prefix /api/{Name}.</summary>
    string Name { get; }

    /// <summary>Human-readable name, e.g. "Accounts and Wallet". Used as the OpenAPI tag.</summary>
    string DisplayName { get; }

    /// <summary>Registers the module's services (repositories, handlers, adapters...).</summary>
    IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration);

    /// <summary>Maps the module's public REST endpoints.</summary>
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
