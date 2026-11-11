namespace FootballManager.Tests;

/// <summary>Fails every request as an API that isn't running does: the connection is refused.</summary>
public sealed class RefusingHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new HttpRequestException("No connection could be made because the target machine actively refused it.");
    }
}
