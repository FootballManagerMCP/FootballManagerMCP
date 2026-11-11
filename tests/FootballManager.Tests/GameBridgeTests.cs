using System.Text.Json;
using FootballManager.Infrastructure.Clients;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FootballManager.Tests;

public class GameBridgeTests
{
    private readonly RecordingHandler _plugin = new();
    private readonly GameBridge _bridge;

    public GameBridgeTests()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(GameBridge.HttpClientName).Returns(_ => new HttpClient(_plugin) { BaseAddress = new Uri("http://localhost:5121") });
        _bridge = new GameBridge(factory, NullLogger<GameBridge>.Instance);
    }

    // A web page can't send a custom header to localhost without a CORS check the plugin never answers: writes carry
    // one, so only this client can change the game. HTTP.sys also refuses a POST that doesn't state its length.
    [Fact]
    public async Task Writes_go_as_POST_with_the_plugins_header_and_a_length()
    {
        await _bridge.WriteAsync<JsonElement>("/set?path=X&value=1&confirm=true", CancellationToken.None);

        var request = Assert.Single(_plugin.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("true", request.Headers["X-MCPFM-Write"]);
        Assert.Equal(0, request.ContentLength);
    }

    [Fact]
    public async Task Reads_go_as_plain_GETs()
    {
        await _bridge.GetJsonAsync<JsonElement>("/status", CancellationToken.None);

        var request = Assert.Single(_plugin.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.False(request.Headers.ContainsKey("X-MCPFM-Write"));
    }
}
