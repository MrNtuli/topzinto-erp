using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class RefreshTokenFlowTests : IClassFixture<ErpWebApplicationFactory>
{
    private const string AdminEmail = "admin@topzinto.com";
    private const string AdminPassword = "Topzinto@2024";

    private readonly HttpClient _client;

    public RefreshTokenFlowTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Login_ReturnsRefreshToken()
    {
        var json = await LoginAsync();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("refreshToken").GetString()));
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        var login = await LoginAsync();
        var refreshToken = login.GetProperty("refreshToken").GetString()!;

        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("refreshToken").GetString()));
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = "invalid-token" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_InvalidatesExistingAccessToken()
    {
        const string tempPassword = "TempRefresh@2025";
        var login = await LoginAsync();
        var accessToken = login.GetProperty("accessToken").GetString()!;

        var change = await PostChangePasswordAsync(accessToken, AdminPassword, tempPassword);
        Assert.Equal(HttpStatusCode.OK, change.StatusCode);

        var me = await GetMeAsync(accessToken);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);

        var restoreLogin = await LoginWithPasswordAsync(tempPassword);
        var restoreToken = restoreLogin.GetProperty("accessToken").GetString()!;
        var restore = await PostChangePasswordAsync(restoreToken, tempPassword, AdminPassword);
        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);
    }

    private async Task<JsonElement> LoginAsync() =>
        await LoginWithPasswordAsync(AdminPassword);

    private async Task<JsonElement> LoginWithPasswordAsync(string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AdminEmail,
            password,
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<HttpResponseMessage> PostChangePasswordAsync(string token, string currentPassword, string newPassword)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/change-password")
        {
            Content = JsonContent.Create(new { currentPassword, newPassword }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetMeAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _client.SendAsync(request);
    }
}
