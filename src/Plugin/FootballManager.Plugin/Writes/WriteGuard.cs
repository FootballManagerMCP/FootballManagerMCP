using FootballManager.Plugin.Data;

namespace FootballManager.Plugin.Writes;

// Writes change the user's game, so they need an explicit confirm and are refused while a match is being played.
//
// The game answers the next fixture's IsLiveMatch with true during its match and null otherwise (not false). Without
// a next fixture the path runs through an empty record, which the game never answers: the fixture itself, answered
// as empty, says there's no match then. Anything else unanswered refuses the write: it fails closed.
internal static class WriteGuard
{
    private const string NextFixturePath = "Team.NextFixture";
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

        var (noFixture, liveAnswered, live) = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(
                GameBindings.ValueOf(GameBindings.FindRoot(DataQueries.HumanRoot)), [NextFixturePath, LiveMatchPath], []),
            request => request.PathBindings[1].Received || NoFixture(request.PathBindings[0]),
            request =>
            {
                var answer = (NoFixture(request.PathBindings[0]), request.PathBindings[1].Received, request.PathBindings[1].Value);
                request.Dispose();
                return answer;
            },
            timeout);

        if (liveAnswered && live is true)
        {
            throw new ArgumentException("A match is being played; writes are refused until it's over.");
        }

        if (!liveAnswered && !noFixture)
        {
            throw new ArgumentException("The game didn't say whether a match is being played, so nothing was written. Try again.");
        }
    }

    private static bool NoFixture(PropertyBinding fixture)
    {
        return fixture.Received && fixture.Value is null;
    }
}
