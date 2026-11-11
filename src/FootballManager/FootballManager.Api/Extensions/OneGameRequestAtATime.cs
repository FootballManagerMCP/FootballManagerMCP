namespace FootballManager.Api.Extensions;

/// <summary>
/// Lets one API request reach the game at a time. Reading another team briefly makes it the game's current team (its
/// players, fixtures and tactic answer for it until the user's own is put back), so a request running alongside could
/// read that team's data, or write its tactic. Requests wait their turn instead; the health check doesn't.
/// </summary>
public sealed class OneGameRequestAtATime(RequestDelegate next)
{
    private const string ApiPrefix = "/api";

    private static readonly SemaphoreSlim Gate = new(1, 1);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments(ApiPrefix))
        {
            await next(context);
            return;
        }

        await Gate.WaitAsync(context.RequestAborted);

        try
        {
            await next(context);
        }
        finally
        {
            Gate.Release();
        }
    }
}
