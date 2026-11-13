using FootballManager.Application.Interfaces;
using FootballManager.Domain.Players;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the user's scouting through the MCPFM plugin: the scouting centre, the shortlist, the player search and the
// recruitment focuses with their players. Players are read with the user's manager and team as contexts, so the game
// answers with what the club's scouting knows of them, never more.
internal sealed class ScoutingBridgeClient(
    GameBridge bridge, GameVocabulary vocabulary, RemovedFocusLog removed, SearchPositions searchPositions) : IScoutingGameClient
{
    private const int SearchSteps = 10;
    private static readonly TimeSpan SearchStepPause = TimeSpan.FromMilliseconds(300);

    public async Task<ScoutingOverview> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var counts = bridge.GetRecordAsync<BridgeScoutingCountsDto>(
            BridgeQuery.FromHuman()
                .WithHumanAndTeam()
                .Context(ScoutingConstants.PlayerRecruitment, true)
                .Props([
                    ScoutingConstants.CentreCounts, ScoutingConstants.ShortlistCount, ScoutingConstants.ScoutedPlayersCount,
                    ScoutingConstants.FocusListTypeFromHuman
                ]),
            cancellationToken);
        var scouts = bridge.GetListAsync<BridgeScoutDto>(
            BridgeQuery.FromHuman().List(ScoutingConstants.Scouts).WithHumanAndTeam().Props(ScoutingConstants.ScoutProperties),
            cancellationToken);
        var focuses = GetFocusesAsync(cancellationToken);
        var budget = GetBudgetAsync(cancellationToken);
        var shortlists = GetShortlistsAsync(cancellationToken);

        await Task.WhenAll(counts, scouts, focuses, budget, shortlists);

        var overview = (await counts).Values.Adapt<ScoutingOverview>();
        overview.Budget = await budget;
        overview.Scouts = (await scouts).Items.Select(scout => scout.Values.Adapt<Scout>()).ToList();
        overview.Focuses = await focuses;
        overview.Shortlists = await shortlists;
        return overview;
    }

    public async Task<List<Shortlist>> GetShortlistsAsync(CancellationToken cancellationToken = default)
    {
        var lists = await bridge.GetRecordAsync<BridgeShortlistsDto>(
            BridgeQuery.FromHuman()
                .WithHumanAndTeam()
                .Context(ScoutingConstants.PlayerRecruitment, true)
                .Props([ScoutingConstants.Shortlists, ScoutingConstants.SelectedShortlist]),
            cancellationToken);

        var selected = lists.Values.Selected?.Index;

        return (lists.Values.Shortlists ?? []).Select(list =>
        {
            var shortlist = list.Adapt<Shortlist>();
            shortlist.IsSelected = list.Index == selected;
            return shortlist;
        }).ToList();
    }

    public async Task<ScoutingBudget> GetBudgetAsync(CancellationToken cancellationToken = default)
    {
        var budget = await bridge.GetRecordAsync<BridgeScoutingBudgetDto>(
            BridgeQuery.FromHuman()
                .WithHumanAndTeam()
                .Props([ScoutingConstants.Budget, ScoutingConstants.CostsThisSeason, ScoutingConstants.CostsThisMonth]),
            cancellationToken);

        return budget.Values.Adapt<ScoutingBudget>();
    }

    public async Task<List<RecruitmentFocus>> GetFocusesAsync(CancellationToken cancellationToken = default)
    {
        var focuses = await bridge.GetListAsync<BridgeRecruitmentFocusDto>(
            BridgeQuery.FromHuman().List(ScoutingConstants.Focuses).WithHumanAndTeam().Props(ScoutingConstants.FocusProperties),
            cancellationToken);

        var mapped = focuses.Items.Select(focus => focus.Values.Adapt<RecruitmentFocus>()).ToList();
        removed.Flag(mapped);
        return mapped;
    }

    public async Task<ScoutedPlayerPage> GetPlayersAsync(
        ScoutingSource source,
        int? focusId,
        FocusPool pool,
        IReadOnlyCollection<AttributeDefinition> attributes,
        bool includeAbility,
        bool includeReport,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        var query = List(source, focusId, pool).WithHumanAndTeam().Props(PlayerPaths(attributes, includeAbility, includeReport));

        if (source == ScoutingSource.Search)
        {
            await FinishSearchAsync(cancellationToken);
        }

        var keys = vocabulary.KeysAsync(cancellationToken);
        var statusKeys = vocabulary.StatusKeysAsync(cancellationToken);
        var players = bridge.GetListAsync<BridgeScoutedPlayerDto>(
            take is { } count ? query.Take(count) : query,
            cancellationToken,
            gapsExpected: true);
        var search = source == ScoutingSource.Search
            ? SearchStatusAsync(cancellationToken)
            : Task.FromResult<PlayerSearchStatus?>(null);

        await Task.WhenAll(keys, statusKeys, players, search);

        var list = await players;

        if (source == ScoutingSource.Search)
        {
            // In the game's order: a player's place in the results, for acting on him next.
            searchPositions.Remember(list.Items.Select(player => player.Values.Id).ToList());
        }

        return new ScoutedPlayerPage
        {
            Players = list.Items
                .Select(player => player.Values)
                .ToList()
                .BuildAdapter()
                .AddParameters(MappingConfig.TacticKeysParameter, await keys)
                .AddParameters(MappingConfig.StatusKeysParameter, await statusKeys)
                .AdaptToType<List<ScoutedPlayer>>(),
            Total = list.Total ?? list.Count,
            Search = await search
        };
    }

    // The game runs a search in steps, as its screen reads the results: each read of the list takes it further. Reading
    // its size (no players) until that holds steady lets a search left half run (after a load) finish first.
    private async Task FinishSearchAsync(CancellationToken cancellationToken)
    {
        var size = InSearch(BridgeQuery.FromHuman(), ScoutingConstants.PlayerSearch)
            .List(ScoutingConstants.PersonSearch)
            .WithHumanAndTeam()
            .Take(0)
            .Props([GameConstants.UniqueId]);
        int? previous = null;

        for (var read = 0; read < SearchSteps; read++)
        {
            var total = (await bridge.GetListAsync<BridgeScoutedPlayerDto>(size, cancellationToken, gapsExpected: true)).Total;

            if (total == previous)
            {
                return;
            }

            previous = total;
            await Task.Delay(SearchStepPause, cancellationToken);
        }
    }

    private async Task<PlayerSearchStatus?> SearchStatusAsync(CancellationToken cancellationToken)
    {
        var status = await bridge.GetRecordAsync<BridgeSearchStatusDto>(
            InSearch(BridgeQuery.FromHuman(), ScoutingConstants.PlayerSearch)
                .WithHumanAndTeam()
                .Props([ScoutingConstants.SearchStatus, ScoutingConstants.SearchInterest, ScoutingConstants.SearchAttributes]),
            cancellationToken);

        return status.Values.Adapt<PlayerSearchStatus>();
    }

    private static BridgeQuery List(ScoutingSource source, int? focusId, FocusPool pool)
    {
        return source switch
        {
            ScoutingSource.Shortlist => InSearch(BridgeQuery.FromHuman(), ScoutingConstants.ShortlistSearch)
                .List(ScoutingConstants.PersonSearch),
            ScoutingSource.Search => InSearch(BridgeQuery.FromHuman(), ScoutingConstants.PlayerSearch)
                .List(ScoutingConstants.PersonSearch),
            ScoutingSource.Focus => BridgeQuery.FromHuman().List(ScoutingConstants.FocusPlayers(
                focusId ?? throw new ArgumentNullException(nameof(focusId), "A focus's players need the focus's id."), pool)),
            _ => BridgeQuery.FromHuman().List(ScoutingConstants.Recommendations)
        };
    }

    /// <summary>
    /// The person search holding the shortlist (type 0) or the player search (type 1): the type goes along wherever a
    /// path goes through it.
    /// </summary>
    public static BridgeQuery InSearch(BridgeQuery query, int? searchType)
    {
        return searchType is { } type ? query.Context(ScoutingConstants.SearchType, type) : query;
    }

    private static List<string> PlayerPaths(IReadOnlyCollection<AttributeDefinition> attributes, bool includeAbility, bool includeReport)
    {
        var paths = new List<string>(ScoutingConstants.PlayerBasics);

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

        if (includeReport)
        {
            paths.AddRange(ScoutingConstants.PlayerReport);
        }

        return paths;
    }
}
