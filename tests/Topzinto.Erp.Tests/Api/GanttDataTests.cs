using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class GanttDataTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GanttDataTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetGanttData_ReturnsTasksAndMilestones()
    {
        var token = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/schedule/gantt");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("tasks", out var tasks));
        Assert.True(json.TryGetProperty("milestones", out var milestones));
        Assert.Equal(JsonValueKind.Array, tasks.ValueKind);
        Assert.Equal(JsonValueKind.Array, milestones.ValueKind);
    }

    [Fact]
    public async Task GetGanttData_WithProjectFilter_ReturnsOk()
    {
        var token = await LoginAsync();
        var projectId = await GetFirstProjectIdAsync(token);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/schedule/gantt?projectId={projectId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
