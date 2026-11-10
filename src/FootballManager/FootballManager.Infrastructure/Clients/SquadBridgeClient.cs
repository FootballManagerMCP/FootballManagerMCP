using FootballManager.Application.Interfaces;
using FootballManager.Domain.Players;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the squad through the MCPFM plugin. Which paths make up a player lives here, so new fields never need a plugin
// release.
internal sealed class SquadBridgeClient(GameBridge bridge, GameVocabulary vocabulary) : ISquadGameClient
{
    public async Task<Squad> GetSquadAsync(
        IReadOnlyCollection<AttributeDefinition> attributes,
        bool includeAbility,
        bool includeStats,
        CancellationToken cancellationToken = default)
    {
        var human = await bridge.GetRecordAsync<BridgeHumanDto>(
            BridgeQuery.FromHuman().Props([GameConstants.TeamName]), cancellationToken);

        var statusKeys = vocabulary.StatusKeysAsync(cancellationToken);
        var squad = await bridge.GetSquadAsync<BridgePlayerDto>(
            PlayerPaths(attributes, includeAbility, includeStats), cancellationToken);

        return new Squad
        {
            TeamName = human.Values.TeamName ?? string.Empty,
            Players = squad.Items
                .Select(p => p.Values)
                .ToList()
                .BuildAdapter()
                .AddParameters(MappingConfig.StatusKeysParameter, await statusKeys)
                .AdaptToType<List<Player>>()
        };
    }

    private static List<string> PlayerPaths(
        IReadOnlyCollection<AttributeDefinition> attributes, bool includeAbility, bool includeStats)
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

        if (includeStats)
        {
            paths.AddRange(GameConstants.PlayerSeasonStats);
        }

        return paths;
    }
}
