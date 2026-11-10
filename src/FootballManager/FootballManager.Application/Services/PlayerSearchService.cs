using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Services;

public interface IPlayerSearchService
{
    /// <summary>
    /// Checks a player search against what the game offers; runs it in the game's search screen only when
    /// <paramref name="apply"/> is set, replacing the filters it held.
    /// </summary>
    Task<Result<PlayerSearchChange>> RunAsync(PlayerSearchCriteria criteria, bool apply, CancellationToken cancellationToken = default);
}

// Runs the game's player search with the filters asked for, after checking each against the game's own options. The
// search screen is the user's own: a dry run reads what it holds, so the user knows what a run would replace.
public class PlayerSearchService(IPlayerSearchGameClient search, IScoutingGameClient scouting) : IPlayerSearchService
{
    public async Task<Result<PlayerSearchChange>> RunAsync(
        PlayerSearchCriteria criteria, bool apply, CancellationToken cancellationToken = default)
    {
        try
        {
            var choices = search.GetChoicesAsync(cancellationToken);
            var before = search.GetFiltersAsync(cancellationToken);

            await Task.WhenAll(choices, before);

            var errors = Check(criteria, await choices);

            if (errors.Count > 0)
            {
                return Result.Fail<PlayerSearchChange>(errors);
            }

            var change = new PlayerSearchChange { Applied = apply, Before = await before, Planned = criteria };

            if (!apply)
            {
                return Result.Ok(change);
            }

            await search.RunAsync(criteria, change.Before, cancellationToken);

            // Reading the results' count lets the search run to the end; its filters are read back after.
            var page = await scouting.GetPlayersAsync(
                ScoutingSource.Search, null, FocusPool.Recommended, [], false, false, 0, cancellationToken);
            var after = page.Search ?? new PlayerSearchStatus();
            after.Filters = await search.GetFiltersAsync(cancellationToken);

            change.After = after;
            change.Total = page.Total;
            change.Notes.AddRange(Differences(criteria, after.Filters));
            return Result.Ok(change);
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<PlayerSearchChange>(new GameUnavailableError(exception.Message));
        }
        catch (GameRejectedException exception)
        {
            return Result.Fail<PlayerSearchChange>(new ValidationError("game", [exception.Message]));
        }
    }

    private static List<IError> Check(PlayerSearchCriteria criteria, PlayerSearchChoices choices)
    {
        var errors = new List<IError>();

        void Add(string property, string message)
        {
            errors.Add(new ValidationError(property, [message]));
        }

        foreach (var (property, age) in new[] { ("minAge", criteria.MinAge), ("maxAge", criteria.MaxAge) })
        {
            if (age is { } value && !choices.Ages.Contains(value))
            {
                Add(property, $"The search's ages run {choices.Ages.FirstOrDefault()} to {choices.Ages.LastOrDefault()}.");
            }
        }

        if (criteria is { MinAge: { } from, MaxAge: { } to } && from > to)
        {
            Add("maxAge", "The age range ends before it starts.");
        }

        var unknown = criteria.Positions.Where(position => !choices.Positions.Contains(position)).ToList();

        if (unknown.Count > 0)
        {
            Add("positions", $"The search doesn't take {string.Join(", ", unknown)}. It takes: {string.Join(", ", choices.Positions)}.");
        }

        if (criteria.PositionFamiliarity is { } familiarity)
        {
            if (criteria.Positions.Count == 0)
            {
                Add("positionFamiliarity", "A familiarity goes with positions.");
            }
            else if (!choices.Familiarities.Contains(familiarity))
            {
                Add("positionFamiliarity", $"The search takes: {string.Join(", ", choices.Familiarities)}.");
            }
        }

        if (criteria.MinScoutingGrade is { } grade)
        {
            // As the game writes it: "b+" is "B+".
            var known = choices.Grades.FirstOrDefault(g => string.Equals(g, grade.Trim(), StringComparison.OrdinalIgnoreCase));

            if (known is null)
            {
                Add("minScoutingGrade", $"The scouts' grades are {string.Join(", ", choices.Grades)}.");
            }

            criteria.MinScoutingGrade = known ?? grade;
        }

        if (criteria is { InterestType: SearchInterestType.NotConsidered, InterestLevel: not null })
        {
            Add("interestLevel", "A level only goes with an interest that's considered.");
        }

        return errors;
    }

    // What the game holds that differs from what was asked: it can quietly leave a step out.
    private static IEnumerable<string> Differences(PlayerSearchCriteria criteria, PlayerSearchFilters after)
    {
        if (!criteria.QuickFilters.ToHashSet().SetEquals(after.QuickFilters))
        {
            yield return $"The game's quick filters differ from those asked for: on {Listed(after.QuickFilters)}.";
        }

        var wanted = new HashSet<SearchRuleKind>();

        if (criteria.MinAge is not null || criteria.MaxAge is not null)
        {
            wanted.Add(SearchRuleKind.Age);
        }

        if (criteria.Positions.Count > 0)
        {
            wanted.Add(SearchRuleKind.Position);
        }

        if (criteria.MinScoutingGrade is not null)
        {
            wanted.Add(SearchRuleKind.ScoutingGrade);
        }

        var on = after.Rules.Select(rule => rule.Kind).ToList();

        if (!wanted.SetEquals(on))
        {
            yield return $"The game's rules differ from those asked for: on {Listed(on)}.";
        }

        if (criteria.InterestType is { } type && after.InterestType != type)
        {
            yield return $"The game kept the interest at {after.InterestType}.";
        }

        if (criteria.InterestLevel is { } level && after.InterestLevel != level)
        {
            yield return $"The game kept the interest level at {after.InterestLevel}.";
        }
    }

    private static string Listed<T>(List<T> items)
    {
        return items.Count > 0 ? string.Join(", ", items) : "none";
    }
}
