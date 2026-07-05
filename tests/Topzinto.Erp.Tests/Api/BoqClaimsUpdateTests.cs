using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class BoqClaimsUpdateTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BoqClaimsUpdateTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task UpdateBoqItem_RecalculatesAmount()
    {
        var token = await LoginAsync();
        var boqId = await GetFirstBoqIdAsync(token);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/boq/{boqId}")
        {
            Content = JsonContent.Create(new
            {
                itemCode = "BOQ-001",
                description = "Updated excavation",
                category = "Earthworks",
                unit = "m³",
                quantity = 100m,
                rate = 200m,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(20_000m, json.GetProperty("amount").GetDecimal());
    }

    [Fact]
    public async Task UpdateClaim_ChangesTitle()
    {
        var token = await LoginAsync();
        var claimId = await GetFirstClaimIdAsync(token);
        var existing = await GetClaimAsync(token, claimId);

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/claims/{claimId}")
        {
            Content = JsonContent.Create(new
            {
                claimNumber = existing.GetProperty("claimNumber").GetString(),
                title = "Updated claim title",
                claimDate = existing.GetProperty("claimDate").GetString(),
                periodFrom = existing.TryGetProperty("periodFrom", out var from) ? from.GetString() : null,
                periodTo = existing.TryGetProperty("periodTo", out var to) ? to.GetString() : null,
                amount = existing.GetProperty("amount").GetDecimal(),
                status = existing.GetProperty("status").GetString(),
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Updated claim title", json.GetProperty("title").GetString());
    }

    private async Task<Guid> GetFirstBoqIdAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/boq");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var items = await (await _client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>();
        return items[0].GetProperty("id").GetGuid();
    }

    private async Task<Guid> GetFirstClaimIdAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/claims");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var items = await (await _client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>();
        return items[0].GetProperty("id").GetGuid();
    }

    private async Task<JsonElement> GetClaimAsync(string token, Guid id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/claims/{id}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (await (await _client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>())!;
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
