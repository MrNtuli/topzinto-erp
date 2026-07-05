using System.Net;
using System.Net.Http.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class LoginRateLimitTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoginRateLimitTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task Login_ExceedsRateLimit_ReturnsTooManyRequests()
    {
        for (var i = 0; i < 10; i++)
        {
            var attempt = await _client.PostAsJsonAsync("/api/auth/login", new
            {
                email = "admin@topzinto.com",
                password = "wrong-password",
            });
            Assert.NotEqual(HttpStatusCode.TooManyRequests, attempt.StatusCode);
        }

        var blocked = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@topzinto.com",
            password = "wrong-password",
        });

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
    }
}
