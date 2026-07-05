using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class ProjectActivityTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectActivityTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetProjectActivity_ReturnsAuditItems()
    {
        var token = await LoginAsync();

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/projects")
        {
            Content = JsonContent.Create(new
            {
                code = $"ACT-{Guid.NewGuid():N}"[..12],
                name = "Activity Test Project",
                clientId = await GetFirstClientIdAsync(token),
                status = "Planned",
                progress = 0,
                contractValue = 100_000m,
                budget = 90_000m,
            }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var created = await (await _client.SendAsync(createRequest)).Content.ReadFromJsonAsync<JsonElement>();
        var newProjectId = created.GetProperty("id").GetGuid();

        var activityRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{newProjectId}/activity");
        activityRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(activityRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(items.GetArrayLength() >= 1);
        Assert.Equal("Create", items[0].GetProperty("action").GetString());
        Assert.Equal("Project", items[0].GetProperty("entityType").GetString());
        Assert.Contains("Activity Test Project", items[0].GetProperty("summary").GetString());
    }

    [Fact]
    public async Task GetProjectActivity_ForMissingProject_ReturnsNotFound()
    {
        var token = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{Guid.NewGuid()}/activity");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<Guid> GetFirstClientIdAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/clients");
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
