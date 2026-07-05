using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class UserProfileFlowTests : IClassFixture<ErpWebApplicationFactory>
{
    private const string AdminEmail = "admin@topzinto.com";
    private const string OriginalPassword = "Topzinto@2024";

    private readonly HttpClient _client;

    public UserProfileFlowTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetMyProfile_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfile_WithValidToken_ReturnsProfile()
    {
        var token = await LoginAsync(AdminEmail, OriginalPassword);

        var response = await GetMyProfileAsync(token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(AdminEmail, json.GetProperty("email").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("firstName").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("role").GetString()));
    }

    [Fact]
    public async Task UpdateMyProfile_WithValidData_UpdatesNameAndPhone()
    {
        var token = await LoginAsync(AdminEmail, OriginalPassword);

        var original = await responseToJson(await GetMyProfileAsync(token));
        var originalFirst = original.GetProperty("firstName").GetString()!;
        var originalLast = original.GetProperty("lastName").GetString()!;

        const string updatedPhone = "+27 31 555 0100";
        var patch = await PatchMyProfileAsync(token, new
        {
            firstName = "Profile",
            lastName = "Tester",
            phone = updatedPhone,
        });

        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var updated = await responseToJson(patch);
        Assert.Equal("Profile", updated.GetProperty("firstName").GetString());
        Assert.Equal("Tester", updated.GetProperty("lastName").GetString());
        Assert.Equal(updatedPhone, updated.GetProperty("phone").GetString());

        var restore = await PatchMyProfileAsync(token, new
        {
            firstName = originalFirst,
            lastName = originalLast,
            phone = (string?)null,
        });
        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_CannotChangeEmailViaApi()
    {
        var token = await LoginAsync(AdminEmail, OriginalPassword);

        var original = await responseToJson(await GetMyProfileAsync(token));
        var originalFirst = original.GetProperty("firstName").GetString()!;
        var originalLast = original.GetProperty("lastName").GetString()!;
        var originalPhone = original.TryGetProperty("phone", out var phoneEl) && phoneEl.ValueKind != JsonValueKind.Null
            ? phoneEl.GetString()
            : null;

        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/users/me")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    firstName = originalFirst,
                    lastName = originalLast,
                    email = "hacker@evil.com",
                    role = "SuperAdmin",
                }),
                Encoding.UTF8,
                "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(AdminEmail, json.GetProperty("email").GetString());
        Assert.NotEqual("hacker@evil.com", json.GetProperty("email").GetString());
        Assert.NotEqual("SuperAdmin", json.GetProperty("role").GetString());

        var restore = await PatchMyProfileAsync(token, new
        {
            firstName = originalFirst,
            lastName = originalLast,
            phone = originalPhone,
        });
        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_WithEmptyName_ReturnsBadRequest()
    {
        var token = await LoginAsync(AdminEmail, OriginalPassword);

        var response = await PatchMyProfileAsync(token, new
        {
            firstName = "",
            lastName = "User",
            phone = (string?)null,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Expected accessToken in login response.");
    }

    private async Task<HttpResponseMessage> GetMyProfileAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> PatchMyProfileAsync(string token, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, "/api/users/me")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }

    private static async Task<JsonElement> responseToJson(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (json.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException("Expected JSON body.");
        return json;
    }
}
