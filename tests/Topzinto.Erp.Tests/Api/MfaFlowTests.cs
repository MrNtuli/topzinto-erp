using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class MfaFlowTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MfaFlowTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetMfaStatus_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/mfa/status");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMfaStatus_WithValidToken_ReturnsDisabledByDefault()
    {
        var token = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/mfa/status");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<MfaStatusResponse>();
        Assert.NotNull(json);
        Assert.False(json.Enabled);
    }

    [Fact]
    public async Task BeginMfaSetup_ReturnsSharedKeyAndUri()
    {
        var token = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/mfa/setup");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<MfaSetupResponse>();
        Assert.NotNull(json);
        Assert.False(string.IsNullOrWhiteSpace(json.SharedKey));
        Assert.Contains("otpauth://totp/", json.AuthenticatorUri);
    }

    private async Task<string> LoginAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@topzinto.com",
            password = "Topzinto@2024",
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<LoginTokenResponse>();
        return json!.AccessToken;
    }

    private sealed record MfaStatusResponse(bool Enabled);
    private sealed record MfaSetupResponse(string SharedKey, string AuthenticatorUri);
    private sealed record LoginTokenResponse(string AccessToken);
}
