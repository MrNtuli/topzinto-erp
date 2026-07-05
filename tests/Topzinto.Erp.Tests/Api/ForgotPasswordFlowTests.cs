using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class ForgotPasswordFlowTests : IClassFixture<ErpWebApplicationFactory>
{
    private const string AdminEmail = "admin@topzinto.com";
    private const string OriginalPassword = "Topzinto@2024";

    private readonly HttpClient _client;

    public ForgotPasswordFlowTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_ReturnsGenericSuccessWithoutDevLink()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email = "nobody@example.com",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var message = json.GetProperty("message").GetString();
        Assert.Contains("If an account exists", message, StringComparison.OrdinalIgnoreCase);

        if (json.TryGetProperty("devResetLink", out var linkProp))
            Assert.True(linkProp.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                || string.IsNullOrWhiteSpace(linkProp.GetString()));
    }

    [Fact]
    public async Task ForgotPassword_WithKnownEmail_ReturnsGenericSuccessAndDevLink()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email = AdminEmail,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("If an account exists", json.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);

        var devLink = json.GetProperty("devResetLink").GetString();
        Assert.False(string.IsNullOrWhiteSpace(devLink));
        Assert.Contains("token=", devLink, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            email = AdminEmail,
            token = "invalid-token",
            newPassword = "NewSecure@2025",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("Invalid or expired", json.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResetPassword_WithWeakPassword_ReturnsValidationError()
    {
        var (_, token) = await RequestResetTokenAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            email = AdminEmail,
            token,
            newPassword = "weak",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_SucceedsAndAllowsLogin()
    {
        const string tempPassword = "TempReset@2025";

        var (_, token) = await RequestResetTokenAsync();

        var reset = await _client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            email = AdminEmail,
            token,
            newPassword = tempPassword,
        });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var loginWithNew = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AdminEmail,
            password = tempPassword,
        });
        Assert.Equal(HttpStatusCode.OK, loginWithNew.StatusCode);

        var (_, restoreToken) = await RequestResetTokenAsync();
        var restore = await _client.PostAsJsonAsync("/api/auth/reset-password", new
        {
            email = AdminEmail,
            token = restoreToken,
            newPassword = OriginalPassword,
        });
        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);

        var loginOriginal = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AdminEmail,
            password = OriginalPassword,
        });
        Assert.Equal(HttpStatusCode.OK, loginOriginal.StatusCode);
    }

    private async Task<(string Email, string Token)> RequestResetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new { email = AdminEmail });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var devLink = json.GetProperty("devResetLink").GetString()
            ?? throw new InvalidOperationException("Expected devResetLink in test environment.");

        var uri = new Uri(devLink);
        var query = QueryHelpers.ParseQuery(uri.Query);
        var email = query["email"].ToString();
        if (string.IsNullOrWhiteSpace(email))
            email = AdminEmail;
        var token = query["token"].ToString();
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Reset token missing from dev link.");
        return (email, token);
    }
}
