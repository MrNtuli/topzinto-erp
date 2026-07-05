using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Topzinto.Erp.Tests.Infrastructure;

namespace Topzinto.Erp.Tests.Api;

[Collection("Api")]
public class AttendanceRecordTests : IClassFixture<ErpWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AttendanceRecordTests(ErpWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task CreateAndUpdateAttendanceRecord_ReturnsExpectedData()
    {
        var token = await LoginAsync();
        var employeeId = await GetFirstEmployeeIdAsync(token);
        var projectId = await GetFirstProjectIdAsync(token);
        var workDate = DateTime.UtcNow.Date.AddDays(30);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/attendance")
        {
            Content = JsonContent.Create(new
            {
                employeeId,
                projectId,
                workDate,
                status = "Present",
                checkInTime = "07:00",
                checkOutTime = "16:00",
                hoursWorked = 8m,
                notes = "On site",
            }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();
        Assert.Equal("Present", created.GetProperty("status").GetString());
        Assert.Equal(8m, created.GetProperty("hoursWorked").GetDecimal());

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/attendance/{id}")
        {
            Content = JsonContent.Create(new
            {
                employeeId,
                projectId,
                workDate,
                status = "Late",
                checkInTime = "08:30",
                checkOutTime = "16:00",
                hoursWorked = 7.5m,
                notes = "Traffic delay",
            }),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Late", updated.GetProperty("status").GetString());
        Assert.Equal(7.5m, updated.GetProperty("hoursWorked").GetDecimal());
    }

    [Fact]
    public async Task GetAttendanceRecords_ReturnsList()
    {
        var token = await LoginAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/attendance");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }

    [Fact]
    public async Task CreateDuplicateAttendanceRecord_ReturnsBadRequest()
    {
        var token = await LoginAsync();
        var employeeId = await GetFirstEmployeeIdAsync(token);
        var workDate = DateTime.UtcNow.Date.AddDays(31);

        async Task<HttpResponseMessage> CreateAsync() =>
            await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/api/attendance")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
                Content = JsonContent.Create(new
                {
                    employeeId,
                    workDate,
                    status = "Present",
                }),
            });

        Assert.Equal(HttpStatusCode.Created, (await CreateAsync()).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await CreateAsync()).StatusCode);
    }

    private async Task<Guid> GetFirstEmployeeIdAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/employees");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var items = await (await _client.SendAsync(request)).Content.ReadFromJsonAsync<JsonElement>();
        return items[0].GetProperty("id").GetGuid();
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
