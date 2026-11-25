using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace PayLink.Api.Health;

/// <summary>
/// Readiness check: the API is only "ready" when it can run a query on PostgreSQL.
/// </summary>
public sealed class PostgresHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public const string ConnectionStringName = "PayLink";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy($"Connection string '{ConnectionStringName}' is not configured.");
        }

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL is reachable.");
        }
        catch (Exception ex) when (ex is NpgsqlException or TimeoutException or InvalidOperationException)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL is not reachable.", ex);
        }
    }
}
