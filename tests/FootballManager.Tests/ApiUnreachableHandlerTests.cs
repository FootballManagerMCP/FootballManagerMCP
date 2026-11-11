using System.Net;
using McpServer;

namespace FootballManager.Tests;

public class ApiUnreachableHandlerTests
{
    [Fact]
    public async Task An_API_that_isnt_running_answers_503_saying_what_to_start()
    {
        using var invoker = new HttpMessageInvoker(new ApiUnreachableHandler { InnerHandler = new RefusingHandler() });

        using var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost:5120/api/v1/squad"), CancellationToken.None);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("isn't answering on http://localhost:5120", body);
        Assert.Contains("docker compose up -d", body);
    }
}
