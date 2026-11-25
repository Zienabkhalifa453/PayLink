using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PayLink.Api.Health;

/// <summary>Writes health results as JSON so humans and CI can see which check failed.</summary>
public static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public static Task WriteJson(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var body = new
        {
            status = report.Status.ToString(),
            durationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds,
            }),
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(body, _jsonOptions));
    }
}
