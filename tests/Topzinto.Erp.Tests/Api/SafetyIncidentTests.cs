using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class SafetyIncidentTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SafetyIncidentTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task CreateAndUpdateSafetyIncident_ReturnsExpectedData()
    {
        var token = await LoginAsync();
        var projectId = await GetFirstProjectIdAsync(token);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/safety")
        {
            Content = JsonContent.Create(new
            {
                projectId,
                incidentDate = DateTime.UtcNow.Date,
                title = "Test safety incident",
                description = "Worker slipped on wet surface",
                severity = "High",
                status = "Reported",
                location = "Site entrance",
                reportedByName = "Site Supervisor",
                correctiveAction = "Install non-slip mats",
            }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();
        Assert.Equal("Test safety incident", created.GetProperty("title").GetString());
        Assert.Equal("High", created.GetProperty("severity").GetString());

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/safety/{id}")
        {
            Content = JsonContent.Create(new
            {
                projectId,
                incidentDate = DateTime.UtcNow.Date,
                title = "Updated safety incident",
                description = "Worker slipped on wet surface",
                severity = "Medium",
                status = "Investigating",
                location = "Site entrance",
                reportedByName = "Site Supervisor",
                correctiveAction = "Install non-slip mats",
            }),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated safety incident", updated.GetProperty("title").GetString());
        Assert.Equal("Investigating", updated.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetSafetyIncidents_ReturnsList()
    {
        var token = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/safety");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }

    private async Task<Guid> GetFirstProjectIdAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/projects");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var items = await (await _client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>();
        return items[0].GetProperty("id").GetGuid();
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
