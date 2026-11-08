using FootballManager.Plugin.Data;

namespace FootballManager.Plugin.Writes;

// Writes change the user's game, so they need an explicit confirm and are refused while a match is being played.
internal static class WriteGuard
{
    private const string LiveMatchPath = "Team.NextFixture.IsLiveMatch";

    public static void EnsureConfirmed(bool confirmed)
    {
        if (!confirmed)
        {
            throw new ArgumentException("This changes the game. Repeat the request with confirm=true once the user has agreed.");
        }
    }

    public static async Task EnsureAllowedAsync(bool confirmed, TimeSpan timeout)
    {
        EnsureConfirmed(confirmed);

        var live = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(
                GameBindings.ValueOf(GameBindings.FindRoot(DataQueries.HumanRoot)), [LiveMatchPath], []),
            request => request.IsComplete,
            request =>
            {
                var value = request.PathBindings[0].Value;
                request.Dispose();
                return value;
            },
            timeout);

        if (live is true)
        {
            throw new ArgumentException("A match is being played; writes are refused until it's over.");
        }
    }
}
