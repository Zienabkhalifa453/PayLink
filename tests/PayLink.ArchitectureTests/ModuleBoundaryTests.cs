using System.Reflection;
using PayLink.Api.Modules;
using PayLink.SharedKernel.Modules;

namespace PayLink.ArchitectureTests;

/// <summary>
/// Guards the modular-monolith rules. These back the Modifiability NFR:
/// "adding a biller touches only the Biller Integration module and configuration".
/// </summary>
public sealed class ModuleBoundaryTests
{
    private const string ModulePrefix = "PayLink.Modules.";

    private static IEnumerable<Assembly> ModuleAssemblies() =>
        ModuleCatalog.All.Select(m => m.GetType().Assembly).Distinct();

    [Fact]
    public void Modules_do_not_reference_each_other()
    {
        var violations = ModuleAssemblies()
            .SelectMany(assembly => assembly.GetReferencedAssemblies()
                .Where(r => r.Name!.StartsWith(ModulePrefix, StringComparison.Ordinal))
                .Select(r => $"{assembly.GetName().Name} -> {r.Name}"))
            .ToList();

        Assert.True(violations.Count == 0, "Module-to-module references found:\n" + string.Join('\n', violations));
    }

    [Fact]
    public void Shared_kernel_does_not_reference_any_module()
    {
        var references = typeof(IModule).Assembly.GetReferencedAssemblies()
            .Where(r => r.Name!.StartsWith(ModulePrefix, StringComparison.Ordinal))
            .Select(r => r.Name)
            .ToList();

        Assert.Empty(references);
    }

    [Fact]
    public void Every_module_has_its_own_assembly_and_unique_route_name()
    {
        var modules = ModuleCatalog.All;

        Assert.Equal(modules.Count, ModuleAssemblies().Count());
        Assert.Equal(modules.Count, modules.Select(m => m.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.All(modules, m => Assert.StartsWith(ModulePrefix, m.GetType().Assembly.GetName().Name, StringComparison.Ordinal));
    }

    [Fact]
    public void Catalog_contains_the_six_required_modules()
    {
        string[] required = ["accounts", "catalog", "payments", "biller-integration", "fees", "notifications"];

        var names = ModuleCatalog.All.Select(m => m.Name).ToHashSet(StringComparer.Ordinal);

        Assert.All(required, r => Assert.Contains(r, names));
    }
}
