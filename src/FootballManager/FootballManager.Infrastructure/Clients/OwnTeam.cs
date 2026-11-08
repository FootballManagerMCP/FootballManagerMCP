using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Teams;

namespace FootballManager.Infrastructure.Clients;

// Reading another team's lineups passes it as the game's "team" context, which makes it the user's current team until
// another is passed (found 2026-11-07: Human.Team then read Dundee United). This passes the club's main team back.
internal static class OwnTeam
{
    public static async Task RestoreAsync(GameBridge bridge)
    {
        // Not cancellable: the user's team has to come back whatever happened to the request.
        await bridge.GetRecordAsync<BridgeTeamDto>(
            BridgeQuery.FromHuman().Ref(TeamConstants.TeamContext, TeamConstants.OwnTeam).Props([GameConstants.Name]),
            CancellationToken.None);
    }
}
