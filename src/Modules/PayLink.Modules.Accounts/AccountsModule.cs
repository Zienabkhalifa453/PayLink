using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayLink.SharedKernel.Modules;

namespace PayLink.Modules.Accounts;

/// <summary>
/// Accounts and Wallet module.
/// Responsibility: customers, agents, verification levels, wallets, ledger entries, holds, top-ups, refunds.
/// </summary>
public sealed class AccountsModule : IModule
{
    public string Name => "accounts";

    public string DisplayName => "Accounts and Wallet";

    public IServiceCollection AddModule(IServiceCollection services, IConfiguration configuration)
    {
        // Phase 2: register this module's services here.
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Phase 2: map endpoints under /api/accounts, tagged "Accounts and Wallet" for OpenAPI.
        return endpoints;
    }
}
