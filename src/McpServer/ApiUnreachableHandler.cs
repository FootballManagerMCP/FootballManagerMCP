using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace McpServer;

/// <summary>
/// Turns "the API isn't running" into an answer the model can act on: a 503 saying what to start, rather than a
/// connection error that fails the tool call without a word on why.
/// </summary>
public sealed class ApiUnreachableHandler : DelegatingHandler
{
    // Plain text for the model to read: apostrophes as they are, not escaped as '.
    private static readonly JsonSerializerOptions Plain = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            var problem = JsonSerializer.Serialize(new
            {
                title = $"The Football Manager API isn't answering on {request.RequestUri?.GetLeftPart(UriPartial.Authority)}.",
                detail = "Start it (docker compose up -d in the repository, or dotnet run --project " +
                         "src/FootballManager/FootballManager.Api), then try again. " + exception.Message
            }, Plain);

            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                RequestMessage = request,
                Content = new StringContent(problem, Encoding.UTF8, "application/json")
            };
        }
    }
}
