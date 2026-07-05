using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class FuelLogFlowTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public FuelLogFlowTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task CreateFuelLog_ForExistingVehicle_ReturnsOk()
    {
        var token = await LoginAsync();

        var listRequest = new HttpRequestMessage(HttpMethod.Get, "/api/fleet");
        listRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var listResponse = await _client.SendAsync(listRequest);
        var vehicles = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var vehicleId = vehicles[0].GetProperty("id").GetGuid();

        var createRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/fleet/{vehicleId}/fuel-logs")
        {
            Content = JsonContent.Create(new
            {
                logDate = DateTime.UtcNow.Date,
                litres = 45.5m,
                cost = 950.00m,
                odometerReading = 125000m,
                notes = "Test fuel log",
            }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(createRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(45.5m, json.GetProperty("litres").GetDecimal());
        Assert.Equal(950.00m, json.GetProperty("cost").GetDecimal());

        var detailRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/fleet/{vehicleId}");
        detailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var detailResponse = await _client.SendAsync(detailRequest);
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>();
        var fuelLogs = detail.GetProperty("fuelLogs");
        Assert.True(fuelLogs.GetArrayLength() > 0);
        Assert.Equal("Test fuel log", fuelLogs[0].GetProperty("notes").GetString());
    }

    [Fact]
    public async Task CreateFuelLog_ForMissingVehicle_ReturnsNotFound()
    {
        var token = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/fleet/{Guid.NewGuid()}/fuel-logs")
        {
            Content = JsonContent.Create(new
            {
                logDate = DateTime.UtcNow.Date,
                litres = 10m,
                cost = 200m,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<string> LoginAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@topzinto.com",
            password = "Topzinto@2024",
        });
        return (await login.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
    }
}
