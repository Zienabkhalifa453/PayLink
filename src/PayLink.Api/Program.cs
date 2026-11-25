using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using PayLink.Api.Health;
using PayLink.Api.Modules;

var builder = WebApplication.CreateBuilder(args);

// --- Cross-cutting services -------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PayLink API",
        Version = "v1",
        Description = "Bill-payment and wallet backend.",
    }));

builder.Services
    .AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres", tags: ["ready"]);

// --- Modules (modular monolith) ---------------------------------------------
builder.Services.AddPayLinkModules(builder.Configuration);

var app = builder.Build();

// --- HTTP pipeline ----------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(options => options.DocumentTitle = "PayLink API");

// Liveness: the process is up (no dependencies checked).
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthResponseWriter.WriteJson,
});

// Readiness: the process can serve traffic (database reachable).
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthResponseWriter.WriteJson,
});

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapGet("/api/system/info", () => Results.Ok(new
{
    service = "PayLink API",
    version = typeof(Program).Assembly.GetName().Version?.ToString(),
    environment = app.Environment.EnvironmentName,
    modules = ModuleCatalog.All.Select(m => new { m.Name, m.DisplayName }),
}))
.WithTags("System")
.WithName("GetSystemInfo");

app.MapPayLinkModules();

app.Run();

/// <summary>Exposed so integration tests can start the app with WebApplicationFactory.</summary>
public partial class Program
{
}
