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
internal sealed class MatchesBridgeClient(GameBridge bridge, GameVocabulary vocabulary) : IMatchesGameClient
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
        var (lineup, substitutions) = await PlayersAsync(withBall, await outOfPossession, (await ours).Players ?? [], members, cancellationToken);

        return new MatchDetail
        {
            Summary = summary,
            TeamStats = (await ours).Stats.Adapt<TeamMatchStats?>(),
            OpponentStats = (await theirs).Stats.Adapt<TeamMatchStats?>(),
            Goals = Goals(await events, isHome, members),
            Lineup = lineup,
            Substitutions = substitutions,
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

    // The players who took part, with their positions in both phases (a substitute's those of the slot he took), when
    // they came on and off, and the substitutions.
    private async Task<(List<MatchLineupPlayer> Players, List<MatchSubstitution> Substitutions)> PlayersAsync(
        List<BridgeLineupEntryDto> withBall,
        List<BridgeLineupEntryDto> withoutBall,
        List<BridgePlayerMatchStatsDto> stats,
        IReadOnlyDictionary<int, SquadMember> squad,
        CancellationToken cancellationToken)
    {
        var outOfPossession = LineupPairing.OutOfPossessionBySlot(withBall, withoutBall);

        var startersBySlot = withBall
            .Where(entry => entry.TeamSelectionIndex is not null && LineupShapes.IsStarter(entry))
            .GroupBy(entry => entry.TeamSelectionIndex!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        var statsByPlayer = stats
            .Where(line => line.Player?.Index is not null)
            .GroupBy(line => line.Player!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First());

        var lineup = WithLateSubstitutes(withBall, stats);
        var changes = MatchSubstitutions.Of(lineup, stats);

        var players = lineup.OrderBy(entry => entry.TeamSelectionIndex).Select(async entry =>
        {
            var person = entry.Player?.Index;
            var member = person is { } index ? squad.GetValueOrDefault(index) : null;
            var line = person is { } key ? statsByPlayer.GetValueOrDefault(key) : null;
            var isSubstitute = entry.MatchWasSubbedOn == true;
            var cameOn = isSubstitute ? changes.FirstOrDefault(change => change.OnPlayer == person) : null;

            // A substitute plays in the slot he took: where he finished, or the one he was traced to.
            var slot = !isSubstitute
                ? entry.TeamSelectionIndex
                : (line is { PlayerTeamSelectionIndex: >= 0 } ? line.PlayerTeamSelectionIndex : cameOn?.Slot);

            var withBallEntry = !isSubstitute
                ? entry
                : (slot is { } taken ? startersBySlot.GetValueOrDefault(taken) : null);
            var withoutBallEntry = slot is { } held ? outOfPossession.GetValueOrDefault(held) : null;

            // The slot's role is named from the replaced player's role table when the substitute's lacks it (a table
            // only lists the positions its player can play).
            var replaced = isSubstitute && withBallEntry?.Player?.Index is { } starter ? squad.GetValueOrDefault(starter) : null;

            var inPossessionPosition = PositionAsync(member, withBallEntry, inPossession: true, replaced, cancellationToken);
            var outOfPossessionPosition = PositionAsync(member, withoutBallEntry, inPossession: false, replaced, cancellationToken);

            var minuteOn = !isSubstitute
                ? null
                : (line is { IncidentType: MatchConstants.CameOnIncident } ? line.MatchTime : cameOn?.Minute);
            var minuteOff = line is { PlayerTeamSelectionIndex: < 0 } ? line.MatchTime : null;

            return new MatchLineupPlayer
            {
                SelectionIndex = entry.TeamSelectionIndex ?? -1,
                PlayerId = member?.Id,
                Name = member?.Name,
                CameOnAsSubstitute = isSubstitute,
                MinuteOn = minuteOn,
                MinuteOff = minuteOff,
                WentOffInjured = minuteOff is not null && (line?.IncidentType == MatchConstants.InjuredOffIncident || entry.MatchInjuryType > 0),
                YellowCard = line?.YellowCardedInMatch == true,
                RedCard = line?.RedCardedInMatch == true,
                Rating = entry.MatchRating ?? line?.TotalRating,
                InPossession = await inPossessionPosition,
                OutOfPossession = await outOfPossessionPosition,
                Stats = line.Adapt<PlayerMatchStats?>()
            };
        });

        var lineupPlayers = (await Task.WhenAll(players)).ToList();
        return (lineupPlayers, changes.Select(change => Substitution(change, startersBySlot, squad, lineupPlayers)).ToList());
    }

    // The lineup leaves out some substitutes (seen: two who came on in the 90th minute); their match lines have them.
    private static List<BridgeLineupEntryDto> WithLateSubstitutes(List<BridgeLineupEntryDto> lineup, List<BridgePlayerMatchStatsDto> stats)
    {
        var listed = lineup.Select(entry => entry.Player?.Index).OfType<int>().ToHashSet();
        var next = lineup.Select(entry => entry.TeamSelectionIndex ?? 0).DefaultIfEmpty(0).Max() + 1;

        var late = stats
            .Where(line => line.Player?.Index is { } person && !listed.Contains(person) && MatchSubstitutions.CameOn(line))
            .Select((line, offset) => new BridgeLineupEntryDto
            {
                Player = line.Player,
                TeamSelectionIndex = next + offset,
                MatchWasSubbedOn = true
            });

        return [.. lineup, .. late];
    }

    private static MatchSubstitution Substitution(
        SlotChange change,
        IReadOnlyDictionary<int, BridgeLineupEntryDto> startersBySlot,
        IReadOnlyDictionary<int, SquadMember> squad,
        List<MatchLineupPlayer> players)
    {
        var off = squad.GetValueOrDefault(change.OffPlayer);
        var on = squad.GetValueOrDefault(change.OnPlayer);

        return new MatchSubstitution
        {
            Minute = change.Minute,
            OffPlayerId = off?.Id,
            Off = off?.Name,
            OnPlayerId = on?.Id,
            On = on?.Name,
            Position = startersBySlot.GetValueOrDefault(change.Slot)?.Position is { } mask ? PositionMasks.ToPosition(mask) : null,
            Injury = off?.Id is { } id && players.Any(player => player.PlayerId == id && player.WentOffInjured)
        };
    }

    // A past match only keeps the role as a mask; its name comes from the player's role table at that position, or
    // failing that from another player's (the role is the slot's, whoever plays it).
    private async Task<MatchPosition> PositionAsync(
        SquadMember? member, BridgeLineupEntryDto? entry, bool inPossession, SquadMember? fallback, CancellationToken cancellationToken)
    {
        if (entry?.Position is not { } mask)
        {
            return new MatchPosition();
        }

        var keys = await vocabulary.KeysAsync(cancellationToken);
        var role = await RoleAsync(member, mask, entry.PositionRole, inPossession, cancellationToken);

        if (role is null && fallback is not null && fallback != member)
        {
            role = await RoleAsync(fallback, mask, entry.PositionRole, inPossession, cancellationToken);
        }

        return new MatchPosition
        {
            Position = PositionMasks.ToPosition(mask),
            RoleKey = entry.PositionRole is { } roleId ? keys.Roles(inPossession).GetValueOrDefault(roleId) : null,
            Role = role?.TacticsRoleName,
            RoleShortName = role?.ShortName
        };
    }

    private async Task<BridgeRoleRowDto?> RoleAsync(
        SquadMember? member, ulong mask, ulong? roleId, bool inPossession, CancellationToken cancellationToken)
    {
        if (member is null)
        {
            return null;
        }

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

        return table.Items.FirstOrDefault()?.Values.Roles?.FirstOrDefault(row => row.TacticsRoleId == roleId);
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
