using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class ComplianceRecordTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ComplianceRecordTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task CreateAndUpdateComplianceRecord_ReturnsExpectedData()
    {
        var token = await LoginAsync();
        var projectId = await GetFirstProjectIdAsync(token);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/compliance")
        {
            Content = JsonContent.Create(new
            {
                title = "CIPC Annual Return",
                type = "Certificate",
                entityType = "Company",
                entityId = (Guid?)null,
                projectId,
                issueDate = DateTime.UtcNow.Date,
                expiryDate = DateTime.UtcNow.Date.AddYears(1),
                status = "Valid",
                responsiblePerson = "HR Manager",
                notes = "Annual filing",
            }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();
        Assert.Equal("CIPC Annual Return", created.GetProperty("title").GetString());
        Assert.Equal("Certificate", created.GetProperty("type").GetString());

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/compliance/{id}")
        {
            Content = JsonContent.Create(new
            {
                title = "Updated CIPC Annual Return",
                type = "Certificate",
                entityType = "Company",
                entityId = (Guid?)null,
                projectId,
                issueDate = DateTime.UtcNow.Date,
                expiryDate = DateTime.UtcNow.Date.AddYears(1),
                status = "Expiring Soon",
                responsiblePerson = "HR Manager",
                notes = "Renewal due next month",
            }),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated CIPC Annual Return", updated.GetProperty("title").GetString());
        Assert.Equal("Expiring Soon", updated.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetComplianceRecords_ReturnsList()
    {
        var token = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/compliance");
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
