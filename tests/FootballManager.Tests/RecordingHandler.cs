using System.Net;
using System.Text;

namespace FootballManager.Tests;

/// <summary>Answers every request with the given body and keeps what was sent, for checking how a client calls.</summary>
public sealed class RecordingHandler(string body = "{}") : HttpMessageHandler
{
    public List<(HttpMethod Method, Uri? Uri, Dictionary<string, string> Headers, long? ContentLength)> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add((
            request.Method,
            request.RequestUri,
            request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value)),
            request.Content?.Headers.ContentLength));

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
    }
}
