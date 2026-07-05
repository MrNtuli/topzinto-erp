using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class EmailSettingsTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmailSettingsTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetEmailSettings_ReturnsDefaults()
    {
        var token = await LoginAsync();
        await ResetEmailSettingsAsync(token);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/email-settings");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(json.GetProperty("smtpEnabled").GetBoolean());
        Assert.False(json.GetProperty("hasPassword").GetBoolean());
    }

    [Fact]
    public async Task UpdateEmailSettings_PersistsValues()
    {
        var token = await LoginAsync();
        await ResetEmailSettingsAsync(token);

        var update = new HttpRequestMessage(HttpMethod.Put, "/api/admin/email-settings")
        {
            Content = JsonContent.Create(new
            {
                smtpEnabled = true,
                smtpHost = "smtp.example.com",
                smtpPort = 587,
                smtpUseSsl = true,
                smtpUsername = "user@example.com",
                smtpPassword = "secret",
                emailFromAddress = "noreply@topzinto.com",
                emailFromName = "TopZinto ERP",
            }),
        };
        update.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(update);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.GetProperty("smtpEnabled").GetBoolean());
        Assert.Equal("smtp.example.com", json.GetProperty("smtpHost").GetString());
        Assert.True(json.GetProperty("hasPassword").GetBoolean());
    }

    [Fact]
    public async Task TestEmail_WhenDisabled_ReturnsOkMessage()
    {
        var token = await LoginAsync();
        await ResetEmailSettingsAsync(token);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/email/test")
        {
            Content = JsonContent.Create(new { toEmail = "test@example.com" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("disabled", json.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task ResetEmailSettingsAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/admin/email-settings")
        {
            Content = JsonContent.Create(new
            {
                smtpEnabled = false,
                smtpHost = (string?)null,
                smtpPort = 587,
                smtpUseSsl = true,
                smtpUsername = (string?)null,
                smtpPassword = "",
                emailFromAddress = (string?)null,
                emailFromName = (string?)null,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.SendAsync(request);
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
