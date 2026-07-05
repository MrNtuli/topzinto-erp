using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class HealthEndpointTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetHealth_ReturnsOkWithVersion()
    {
        var response = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("healthy", json.GetProperty("status").GetString());
        Assert.Equal("2.45", json.GetProperty("version").GetString());
    }
}
