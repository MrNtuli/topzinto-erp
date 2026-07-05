using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class ChangePasswordFlowTests : IClassFixture<ErpWebApplicationFactory>
{
    private const string AdminEmail = "admin@topzinto.com";
    private const string OriginalPassword = "Topzinto@2024";

    private readonly HttpClient _client;

    public ChangePasswordFlowTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task ChangePassword_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = OriginalPassword,
            newPassword = "NewSecure@2025",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ReturnsBadRequest()
    {
        var token = await LoginAsync(AdminEmail, OriginalPassword);

        var response = await PostChangePasswordAsync(token, "WrongPassword@1", "NewSecure@2025");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_ReturnsValidationError()
    {
        var token = await LoginAsync(AdminEmail, OriginalPassword);

        var response = await PostChangePasswordAsync(token, OriginalPassword, "weak");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task ChangePassword_WithValidCredentials_SucceedsAndAllowsLoginWithNewPassword()
    {
        const string tempPassword = "TempChange@2025";

        var token = await LoginAsync(AdminEmail, OriginalPassword);

        var change = await PostChangePasswordAsync(token, OriginalPassword, tempPassword);
        Assert.Equal(HttpStatusCode.OK, change.StatusCode);

        var json = await change.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("updated", json.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);

        var loginWithNew = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AdminEmail,
            password = tempPassword,
        });
        Assert.Equal(HttpStatusCode.OK, loginWithNew.StatusCode);

        var restoreToken = await LoginAsync(AdminEmail, tempPassword);
        var restore = await PostChangePasswordAsync(restoreToken, tempPassword, OriginalPassword);
        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);

        var loginOriginal = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AdminEmail,
            password = OriginalPassword,
        });
        Assert.Equal(HttpStatusCode.OK, loginOriginal.StatusCode);
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Expected accessToken in login response.");
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
}
