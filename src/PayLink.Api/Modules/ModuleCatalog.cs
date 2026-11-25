using PayLink.Modules.Accounts;
using PayLink.Modules.BillerIntegration;
using PayLink.Modules.Catalog;
using PayLink.Modules.Fees;
using PayLink.Modules.Fraud;
using PayLink.Modules.Notifications;
using PayLink.Modules.Payments;
using PayLink.Modules.Reconciliation;
using PayLink.SharedKernel.Modules;

namespace PayLink.Api.Modules;

/// <summary>
/// The single place that lists the modules the host loads.
/// Adding a module = add a project + one line here.
/// </summary>
public static class ModuleCatalog
{
    public static IReadOnlyList<IModule> All { get; } =
    [
        new AccountsModule(),
        new CatalogModule(),
        new PaymentsModule(),
        new BillerIntegrationModule(),
        new FeesModule(),
        new NotificationsModule(),
        new FraudModule(),
        new ReconciliationModule(),
    ];

    public static IServiceCollection AddPayLinkModules(this IServiceCollection services, IConfiguration configuration)
    {
        foreach (var module in All)
        {
            module.AddModule(services, configuration);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapPayLinkModules(this IEndpointRouteBuilder endpoints)
    {
        foreach (var module in All)
        {
            module.MapEndpoints(endpoints);
        }

        return endpoints;
    }
}
