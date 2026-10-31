using FootballManager.Application.Interfaces;
using FootballManager.Domain.Attributes;
using FootballManager.Domain.Entities;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the squad through the MCPFM plugin. Which paths make up a player lives here, so new fields never need a plugin
// release.
internal sealed class GameBridgeClient(GameBridge bridge) : IGameClient
{
    public async Task<Squad> GetSquadAsync(
        IReadOnlyCollection<AttributeDefinition> attributes, bool includeAbility, CancellationToken cancellationToken = default)
    {
        var human = await bridge.GetRecordAsync<BridgeHumanDto>(
            BridgeQuery.FromHuman().Props([GameConstants.TeamName]), cancellationToken);

        var squad = await bridge.GetSquadAsync<BridgePlayerDto>(PlayerPaths(attributes, includeAbility), cancellationToken);

        return new Squad
        {
            TeamName = human.Values.TeamName ?? string.Empty,
            Players = squad.Items.Select(p => p.Values.Adapt<Player>()).ToList()
        };
    }

    private static List<string> PlayerPaths(IReadOnlyCollection<AttributeDefinition> attributes, bool includeAbility)
    {
        var paths = new List<string>(GameConstants.PlayerBasics);

        foreach (var attribute in attributes)
        {
            paths.Add(GameConstants.AttributeMin(attribute));
            paths.Add(GameConstants.AttributeMax(attribute));
        }

        if (includeAbility)
        {
            paths.Add(GameConstants.CurrentAbility);
            paths.Add(GameConstants.PotentialAbility);
        }

        return paths;
    }
}
