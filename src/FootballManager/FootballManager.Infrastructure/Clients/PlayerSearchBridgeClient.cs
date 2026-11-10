using System.Globalization;
using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Scouting;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Scouting;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// The game's player search, through the plugin, as its search screen works it: quick filters toggle on the search, the
// rules are edited in a filter session (stored, changed, then applied, which runs the search again), each rule kept
// bound while its event goes, as the screen's editor keeps the rules it shows.
internal sealed class PlayerSearchBridgeClient(GameBridge bridge) : IPlayerSearchGameClient
{
    public async Task<PlayerSearchFilters> GetFiltersAsync(CancellationToken cancellationToken = default)
    {
        var state = bridge.GetRecordAsync<BridgeSearchStateDto>(
            OnSearch().Props([PlayerSearchConstants.InterestSelection, PlayerSearchConstants.EditFilterLabel]), cancellationToken);
        var quickFilters = Enum.GetValues<SearchQuickFilter>()
            .Select(async filter => (Filter: filter, On: await QuickFilterOnAsync(filter, cancellationToken)))
            .ToList();
        var rules = Enum.GetValues<SearchRuleKind>()
            .Select(kind => ReadRuleAsync(kind, PlayerSearchConstants.RuleState, cancellationToken))
            .ToList();

        await Task.WhenAll(Task.WhenAll(quickFilters), Task.WhenAll(rules), state);

        return new BridgeSearchFiltersDto
        {
            State = (await state).Values,
            QuickFilters = quickFilters.Select(read => read.Result).Where(read => read.On).Select(read => read.Filter).ToList(),
            Rules = rules.Select(read => read.Result).Where(rule => rule.Enabled == true).ToList()
        }.Adapt<PlayerSearchFilters>();
    }

    public async Task<PlayerSearchChoices> GetChoicesAsync(CancellationToken cancellationToken = default)
    {
        var age = ReadRuleAsync(SearchRuleKind.Age, [PlayerSearchConstants.MinValues], cancellationToken);
        var position = ReadRuleAsync(SearchRuleKind.Position, [PlayerSearchConstants.Values, PlayerSearchConstants.Comparisons], cancellationToken);
        var grade = ReadRuleAsync(SearchRuleKind.ScoutingGrade, [PlayerSearchConstants.MinValues], cancellationToken);

        await Task.WhenAll(age, position, grade);

        return new BridgeSearchChoicesDto { Age = await age, Position = await position, Grade = await grade }.Adapt<PlayerSearchChoices>();
    }

    public async Task RunAsync(PlayerSearchCriteria criteria, PlayerSearchFilters current, CancellationToken cancellationToken = default)
    {
        await SearchEventAsync(PlayerSearchConstants.StoreEvent, null, cancellationToken);

        try
        {
            // A reset clears the rules and quick filters, so each rule asked for is switched on from there.
            await SearchEventAsync(PlayerSearchConstants.ResetEvent, null, cancellationToken);
            await InterestAsync(criteria.InterestType, criteria.InterestLevel, current, cancellationToken);
            await RulesAsync(criteria, cancellationToken);
            await SearchEventAsync(PlayerSearchConstants.RestoreEvent, "true", cancellationToken);

            // The quick filters last, each read before it's toggled: sent straight after the reset, a toggle can land
            // before the game has cleared them, and the reset then switches it back off.
            await QuickFiltersAsync(criteria.QuickFilters, cancellationToken);
        }
        catch
        {
            await PutBackAsync(current);
            throw;
        }
    }

    // Toggles the quick filters that differ from those wanted, then checks the game took it.
    private async Task QuickFiltersAsync(IReadOnlyCollection<SearchQuickFilter> wanted, CancellationToken cancellationToken)
    {
        foreach (var filter in Enum.GetValues<SearchQuickFilter>())
        {
            if (await QuickFilterOnAsync(filter, cancellationToken) != wanted.Contains(filter))
            {
                await ToggleAsync(filter, cancellationToken);
            }
        }

        var reads = Enum.GetValues<SearchQuickFilter>()
            .Select(async filter => (Filter: filter, On: await QuickFilterOnAsync(filter, cancellationToken)))
            .ToList();
        var wrong = (await Task.WhenAll(reads)).Where(read => read.On != wanted.Contains(read.Filter)).Select(read => read.Filter).ToList();

        if (wrong.Count > 0)
        {
            throw new GameRejectedException($"The game didn't take the quick filters {string.Join(", ", wrong)}.");
        }
    }

    private async Task RulesAsync(PlayerSearchCriteria criteria, CancellationToken cancellationToken)
    {
        if (criteria.MinAge is not null || criteria.MaxAge is not null)
        {
            var comparison = criteria switch
            {
                { MinAge: not null, MaxAge: not null } => PlayerSearchConstants.Between,
                { MinAge: not null } => PlayerSearchConstants.AtLeast,
                _ => PlayerSearchConstants.AtMost
            };

            await RuleEventAsync(SearchRuleKind.Age, PlayerSearchConstants.EnableEvent, "true", cancellationToken);
            await RuleEventAsync(SearchRuleKind.Age, PlayerSearchConstants.ComparisonEvent, Unsigned(comparison), cancellationToken);

            if (criteria.MinAge is { } min)
            {
                await RuleEventAsync(SearchRuleKind.Age, PlayerSearchConstants.MinValueEvent, Item(PlayerSearchConstants.MinValues, min), cancellationToken);
            }

            if (criteria.MaxAge is { } max)
            {
                await RuleEventAsync(SearchRuleKind.Age, PlayerSearchConstants.MaxValueEvent, Item(PlayerSearchConstants.MaxValues, max), cancellationToken);
            }
        }

        if (criteria.Positions.Count > 0)
        {
            // Several positions go as one mask: the game then looks for any of them.
            var mask = criteria.Positions.Aggregate(0UL, (all, position) => all | PositionMasks.Of(position));
            var familiarity = PlayerSearchConstants.Familiarities[criteria.PositionFamiliarity ?? PositionFamiliarity.Natural];

            await RuleEventAsync(SearchRuleKind.Position, PlayerSearchConstants.EnableEvent, "true", cancellationToken);
            await RuleEventAsync(SearchRuleKind.Position, PlayerSearchConstants.ComparisonEvent, Unsigned(familiarity), cancellationToken);
            await RuleEventAsync(SearchRuleKind.Position, PlayerSearchConstants.ValueEvent, Unsigned(mask), cancellationToken);
        }

        if (criteria.MinScoutingGrade is { } grade)
        {
            await RuleEventAsync(SearchRuleKind.ScoutingGrade, PlayerSearchConstants.EnableEvent, "true", cancellationToken);
            await RuleEventAsync(SearchRuleKind.ScoutingGrade, PlayerSearchConstants.MinValueEvent,
                $"@{PlayerSearchConstants.MinValues}[{PlayerSearchConstants.Name}={grade}]", cancellationToken);
        }
    }

    // The type first: a new type takes the level the search had.
    private async Task InterestAsync(
        SearchInterestType? type, SearchInterestLevel? level, PlayerSearchFilters current, CancellationToken cancellationToken)
    {
        if (type is { } wanted && wanted != current.InterestType)
        {
            await SearchEventAsync(PlayerSearchConstants.InterestTypeEvent,
                $"@{PlayerSearchConstants.InterestTypes}[ID={Format(PlayerSearchConstants.InterestTypeIds[wanted])}]", cancellationToken);
        }

        if (level is { } at && (type ?? current.InterestType) != SearchInterestType.NotConsidered)
        {
            await SearchEventAsync(PlayerSearchConstants.InterestLevelEvent, Format(PlayerSearchConstants.InterestLevels[at]), cancellationToken);
        }
    }

    // After a failure: the stored rules back, then the quick filters and interest as they were.
    private async Task PutBackAsync(PlayerSearchFilters current)
    {
        await SearchEventAsync(PlayerSearchConstants.RestoreEvent, "false", CancellationToken.None);

        foreach (var filter in Enum.GetValues<SearchQuickFilter>())
        {
            if (await QuickFilterOnAsync(filter, CancellationToken.None) != current.QuickFilters.Contains(filter))
            {
                await ToggleAsync(filter, CancellationToken.None);
            }
        }

        var now = await GetFiltersAsync(CancellationToken.None);
        await InterestAsync(current.InterestType, current.InterestLevel, now, CancellationToken.None);
    }

    private async Task<bool> QuickFilterOnAsync(SearchQuickFilter filter, CancellationToken cancellationToken)
    {
        var state = await bridge.GetRecordAsync<BridgeQuickFilterStateDto>(
            OnSearch()
                .Context(PlayerSearchConstants.QuickFilterContext, PlayerSearchConstants.QuickFilterFlags[filter])
                .Props([PlayerSearchConstants.QuickFilterEnabled]),
            cancellationToken);

        return state.Values.Enabled == true;
    }

    // Each toggle flips the filter.
    private Task ToggleAsync(SearchQuickFilter filter, CancellationToken cancellationToken)
    {
        return SearchEventAsync(
            PlayerSearchConstants.ToggleQuickFilterEvent, Format(PlayerSearchConstants.QuickFilterFlags[filter]), cancellationToken);
    }

    private async Task<BridgeSearchRuleDto> ReadRuleAsync(SearchRuleKind kind, string[] paths, CancellationToken cancellationToken)
    {
        var rule = (await bridge.GetRecordAsync<BridgeSearchRuleDto>(OnRule(kind).Props(paths), cancellationToken)).Values;
        rule.Kind = kind;
        return rule;
    }

    // An event on the search: no data (null) or the given value.
    private Task SearchEventAsync(string searchEvent, string? data, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<JsonElement>(
            OnSearch().ToWrite("/native", ("event", searchEvent), data is null ? ("data", "null") : ("field", $"Data:{data}"), ("template", "none")),
            cancellationToken);
    }

    private Task RuleEventAsync(SearchRuleKind kind, string ruleEvent, string data, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<JsonElement>(
            OnRule(kind).ToWrite("/native",
                ("event", ruleEvent), ("field", $"Data:{data}"), ("template", "none"), ("hold", PlayerSearchConstants.HoldMs)),
            cancellationToken);
    }

    private static BridgeQuery OnSearch()
    {
        return ScoutingBridgeClient.InSearch(BridgeQuery.FromHuman(), ScoutingConstants.PlayerSearch)
            .Base(PlayerSearchConstants.Search)
            .WithHumanAndTeam();
    }

    private static BridgeQuery OnRule(SearchRuleKind kind)
    {
        return ScoutingBridgeClient.InSearch(BridgeQuery.FromHuman(), ScoutingConstants.PlayerSearch)
            .Base(PlayerSearchConstants.Rule(kind))
            .WithHumanAndTeam();
    }

    private static string Item(string list, int id)
    {
        return $"@{list}[ID={Format(id)}]";
    }

    private static string Unsigned(ulong value)
    {
        return $"u:{value.ToString(CultureInfo.InvariantCulture)}";
    }

    private static string Format(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
