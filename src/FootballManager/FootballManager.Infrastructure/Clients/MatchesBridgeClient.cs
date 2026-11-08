using FootballManager.Application.Interfaces;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Tactics;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Dtos.Tactics;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the user's matches through the MCPFM plugin, the way the game's fixtures and match screens do. A match is
// asked about by its place in the season's fixture list, which the list itself gives.
internal sealed class MatchesBridgeClient(GameBridge bridge) : IMatchesGameClient
{
    public async Task<TeamFixtures> GetFixturesAsync(CancellationToken cancellationToken = default)
    {
        var team = TeamAsync(cancellationToken);
        var fixtures = FixturesAsync(cancellationToken);

        await Task.WhenAll(team, fixtures);

        return new TeamFixtures
        {
            TeamName = (await team).TeamName ?? string.Empty,
            Fixtures = Summaries(await fixtures, (await team).TeamId)
        };
    }

    public async Task<MatchDetail?> GetMatchAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var team = TeamAsync(cancellationToken);
        var fixtures = FixturesAsync(cancellationToken);
        var squad = SquadAsync(cancellationToken);

        await Task.WhenAll(team, fixtures, squad);

        var summaries = Summaries(await fixtures, (await team).TeamId);
        var index = summaries.FindIndex(match => match.Date == date);

        if (index < 0)
        {
            return null;
        }

        var summary = summaries[index];

        if (!summary.IsPlayed)
        {
            return new MatchDetail { Summary = summary };
        }

        var isHome = summary.Venue == Venue.Home;
        var ours = SideAsync(index, isHome, withPlayers: true, cancellationToken);
        var theirs = SideAsync(index, !isHome, withPlayers: false, cancellationToken);
        var outOfPossession = LineupAsync(index, inPossession: false, cancellationToken);
        var events = EventsAsync(index, cancellationToken);

        await Task.WhenAll(ours, theirs, outOfPossession, events);

        var members = await squad;
        var withBall = (await fixtures).Items[index].Values.Lineup ?? [];

        return new MatchDetail
        {
            Summary = summary,
            TeamStats = (await ours).Stats.Adapt<TeamMatchStats?>(),
            OpponentStats = (await theirs).Stats.Adapt<TeamMatchStats?>(),
            Goals = Goals(await events, isHome, members),
            Lineup = await PlayersAsync(withBall, await outOfPossession, (await ours).Players ?? [], members, cancellationToken),
            PlayerOfTheMatch = PlayerOfTheMatch(await events, members)
        };
    }

    // The season's fixtures, oldest first, sized like the game's fixtures screen.
    private static BridgeQuery Fixtures()
    {
        return BridgeQuery.FromHuman()
            .List(MatchConstants.FixtureList)
            .FixtureWindow()
            .WithHumanAndTeam();
    }

    private async Task<BridgeHumanDto> TeamAsync(CancellationToken cancellationToken)
    {
        var human = await bridge.GetRecordAsync<BridgeHumanDto>(
            BridgeQuery.FromHuman().Props([GameConstants.TeamName, GameConstants.TeamId]), cancellationToken);

        return human.Values;
    }

    // Each fixture with the user's lineup in possession, which gives the shape they played; future ones have none.
    private Task<BridgeListDto<BridgeFixtureDto>> FixturesAsync(CancellationToken cancellationToken)
    {
        return bridge.GetListAsync<BridgeFixtureDto>(
            Fixtures().Context(TacticConstants.InPossession, true).Props(MatchConstants.Summary),
            cancellationToken,
            gapsExpected: true);
    }

    private static List<MatchSummary> Summaries(BridgeListDto<BridgeFixtureDto> fixtures, int? teamId)
    {
        return fixtures.Items.Select(fixture =>
        {
            var summary = fixture.Values.Adapt<MatchSummary>();
            summary.TeamId = teamId ?? 0;
            summary.Formation = summary.IsPlayed ? LineupShapes.Of(fixture.Values.Lineup ?? []) : null;
            return summary;
        }).ToList();
    }

    // The squad by database index, which is how a match refers to its players, with each one's place in the squad
    // list, which is how the game is asked about them.
    private async Task<Dictionary<int, SquadMember>> SquadAsync(CancellationToken cancellationToken)
    {
        var squad = await bridge.GetSquadAsync<BridgePlayerDto>([GameConstants.UniqueId, GameConstants.Name], cancellationToken);

        return squad.Items
            .Select((player, listIndex) => (player.Source?.Index, Member: new SquadMember(listIndex, player.Values.Id, player.Values.Name)))
            .Where(player => player.Index is not null)
            .GroupBy(player => player.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First().Member);
    }

    private async Task<BridgeMatchSideDto> SideAsync(int match, bool isHome, bool withPlayers, CancellationToken cancellationToken)
    {
        string[] paths = withPlayers ? [MatchConstants.TeamStats, MatchConstants.PlayerStats] : [MatchConstants.TeamStats];

        var side = await bridge.GetListAsync<BridgeMatchSideDto>(
            Fixtures().Index(match).Context(MatchConstants.IsHomeTeam, isHome).Props(paths), cancellationToken);

        return side.Items.FirstOrDefault()?.Values ?? new BridgeMatchSideDto();
    }

    private async Task<List<BridgeLineupEntryDto>> LineupAsync(int match, bool inPossession, CancellationToken cancellationToken)
    {
        var lineup = await bridge.GetListAsync<BridgeMatchLineupDto>(
            Fixtures().Index(match).Context(TacticConstants.InPossession, inPossession).Props([MatchConstants.Lineup]),
            cancellationToken);

        return lineup.Items.FirstOrDefault()?.Values.Entries ?? [];
    }

    private async Task<BridgeMatchEventsDto> EventsAsync(int match, CancellationToken cancellationToken)
    {
        var events = await bridge.GetListAsync<BridgeMatchEventsDto>(
            Fixtures().Index(match).Context(MatchConstants.ShowAllEventTypes, true).Props(MatchConstants.Events),
            cancellationToken);

        return events.Items.FirstOrDefault()?.Values ?? new BridgeMatchEventsDto();
    }

    private async Task<List<MatchLineupPlayer>> PlayersAsync(
        List<BridgeLineupEntryDto> withBall,
        List<BridgeLineupEntryDto> withoutBall,
        List<BridgePlayerMatchStatsDto> stats,
        IReadOnlyDictionary<int, SquadMember> squad,
        CancellationToken cancellationToken)
    {
        var outOfPossession = withoutBall
            .Where(entry => entry.TeamSelectionIndex is not null)
            .GroupBy(entry => entry.TeamSelectionIndex!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        var statsByPlayer = stats
            .Where(line => line.Player?.Index is not null)
            .GroupBy(line => line.Player!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        var players = withBall.OrderBy(entry => entry.TeamSelectionIndex).Select(async entry =>
        {
            var person = entry.Player?.Index;
            var member = person is { } index ? squad.GetValueOrDefault(index) : null;
            var withoutBallEntry = entry.TeamSelectionIndex is { } slot ? outOfPossession.GetValueOrDefault(slot) : null;

            var inPossessionPosition = PositionAsync(member, entry, inPossession: true, cancellationToken);
            var outOfPossessionPosition = PositionAsync(member, withoutBallEntry, inPossession: false, cancellationToken);

            return new MatchLineupPlayer
            {
                SelectionIndex = entry.TeamSelectionIndex ?? -1,
                PlayerId = member?.Id,
                Name = member?.Name,
                CameOnAsSubstitute = entry.MatchWasSubbedOn == true,
                Rating = entry.MatchRating,
                InPossession = await inPossessionPosition,
                OutOfPossession = await outOfPossessionPosition,
                Stats = person is { } key ? statsByPlayer.GetValueOrDefault(key).Adapt<PlayerMatchStats?>() : null
            };
        });

        return (await Task.WhenAll(players)).ToList();
    }

    // A past match only keeps the role as a mask; its name comes from the player's role table at that position.
    private async Task<MatchPosition> PositionAsync(
        SquadMember? member, BridgeLineupEntryDto? entry, bool inPossession, CancellationToken cancellationToken)
    {
        if (entry?.Position is not { } mask)
        {
            return new MatchPosition();
        }

        BridgeRoleRowDto? role = null;

        if (member is not null)
        {
            var table = await bridge.GetListAsync<BridgeRoleFitPlayerDto>(
                BridgeQuery.FromHuman()
                    .List(TacticConstants.SquadPath)
                    .Index(member.ListIndex)
                    .Context(TacticConstants.PositionMask, PositionMasks.WithoutSide(mask))
                    .Context(TacticConstants.InPossession, inPossession)
                    .WithHumanAndTeam()
                    .Props([TacticConstants.RoleTable]),
                cancellationToken,
                gapsExpected: true);

            role = table.Items.FirstOrDefault()?.Values.Roles?.FirstOrDefault(row => row.TacticsRoleId == entry.PositionRole);
        }

        return new MatchPosition
        {
            Position = PositionMasks.ToPosition(mask),
            Role = role?.TacticsRoleName,
            RoleShortName = role?.ShortName
        };
    }

    // An incident's side is 0 for the home team and 1 for the away team. Only the user's own players have names.
    private static List<MatchGoal> Goals(BridgeMatchEventsDto events, bool isHome, IReadOnlyDictionary<int, SquadMember> squad)
    {
        return (events.Incidents ?? [])
            .Where(incident => incident.IncidentType == MatchConstants.GoalIncident)
            .Select(incident =>
            {
                var isOurs = (incident.TeamSelectionIndex == 0) == isHome;

                return new MatchGoal
                {
                    Minute = incident.TimeString ?? string.Empty,
                    IsOurs = isOurs,
                    Scorer = isOurs ? NameOf(incident.Player, squad) : null,
                    Assister = isOurs ? NameOf(incident.Player2, squad) : null
                };
            })
            .ToList();
    }

    private static PlayerOfTheMatch? PlayerOfTheMatch(BridgeMatchEventsDto events, IReadOnlyDictionary<int, SquadMember> squad)
    {
        if (string.IsNullOrEmpty(events.PlayerOfTheMatchName))
        {
            return null;
        }

        return new PlayerOfTheMatch
        {
            Name = events.PlayerOfTheMatchName,
            Rating = events.PlayerOfTheMatchRating,
            IsOurs = squad.Values.Any(member => member.Id == events.PlayerOfTheMatchId)
        };
    }

    private static string? NameOf(BridgeReferenceDto? player, IReadOnlyDictionary<int, SquadMember> squad)
    {
        return player?.Index is { } index ? squad.GetValueOrDefault(index)?.Name : null;
    }

    private sealed record SquadMember(int ListIndex, int? Id, string? Name);
}
