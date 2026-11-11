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

        var (answered, live) = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(
                GameBindings.ValueOf(GameBindings.FindRoot(DataQueries.HumanRoot)), [LiveMatchPath], []),
            request => request.IsComplete,
            request =>
            {
                var binding = request.PathBindings[0];
                var answer = (binding.Received, binding.Value);
                request.Dispose();
                return answer;
            },
            timeout);

        // Fails closed: a write goes ahead only once the game has said no match is being played. Without a next fixture
        // the game answers null, which is no match.
        if (!answered)
        {
            throw new ArgumentException("The game didn't say whether a match is being played, so nothing was written. Try again.");
        }

        if (live is true)
        {
            throw new ArgumentException("A match is being played; writes are refused until it's over.");
        }
    }
}
