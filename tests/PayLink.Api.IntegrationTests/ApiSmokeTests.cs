using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PayLink.Api.IntegrationTests;

/// <summary>
/// Starts the real API in memory (no Docker, no database) and checks it boots
/// and exposes its operational endpoints. Database-backed tests come in Phase 2.
/// </summary>
public sealed class ApiSmokeTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Liveness_endpoint_returns_healthy()
    {
        var response = await _client.GetAsync(new Uri("/health/live", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Healthy", json.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task OpenApi_document_is_published()
    {
        var response = await _client.GetAsync(new Uri("/swagger/v1/swagger.json", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("PayLink API", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task System_info_lists_all_eight_modules()
    {
        var response = await _client.GetAsync(new Uri("/api/system/info", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(8, json.RootElement.GetProperty("modules").GetArrayLength());
    }
}
