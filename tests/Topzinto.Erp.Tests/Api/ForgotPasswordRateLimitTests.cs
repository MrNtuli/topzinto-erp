using System.Net;
using System.Net.Http.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class ForgotPasswordRateLimitTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ForgotPasswordRateLimitTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task ForgotPassword_ExceedsRateLimit_ReturnsTooManyRequests()
    {
        for (var i = 0; i < 5; i++)
        {
            var attempt = await _client.PostAsJsonAsync("/api/auth/forgot-password", new
            {
                email = $"user{i}@example.com",
            });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, attempt.StatusCode);
        }

        var blocked = await _client.PostAsJsonAsync("/api/auth/forgot-password", new
        {
            email = "blocked@example.com",
        });

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
    }
}
