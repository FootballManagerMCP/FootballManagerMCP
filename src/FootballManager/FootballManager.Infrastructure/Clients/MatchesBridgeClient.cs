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
            Fixtures = Summaries(await fixtures, (await team).TeamId).Select(dated => dated.Summary).ToList()
        };
    }

    public async Task<MatchDetail?> GetMatchAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var team = TeamAsync(cancellationToken);
        var fixtures = FixturesAsync(cancellationToken);
        var squad = SquadAsync(cancellationToken);

        await Task.WhenAll(team, fixtures, squad);

        var summaries = Summaries(await fixtures, (await team).TeamId);
        var found = summaries.FindIndex(match => match.Summary.Date == date);

        if (found < 0)
        {
            return null;
        }

        // The per-match reads go by the fixture's place in the whole list.
        var (summary, index) = summaries[found];

        if (!summary.IsPlayed)
        {
            return new MatchDetail { Summary = summary };
        }

        var isHome = summary.Venue == Venue.Home;
        var ours = SideAsync(index, isHome, withPlayers: true, cancellationToken);
        var theirs = SideAsync(index, !isHome, withPlayers: false, cancellationToken);
        var outOfPossession = LineupAsync(index, inPossession: false, cancellationToken);
        var events = EventsAsync(index, cancellationToken);
        var incidents = IncidentsAsync(index, cancellationToken);
        var named = PeopleAsync(index, isHome, cancellationToken);

        await Task.WhenAll(ours, theirs, outOfPossession, events, incidents, named);

        var people = new MatchPeople(await squad, await named);
        var withBall = (await fixtures).Items[index].Values.Lineup ?? [];
        var lines = (await ours).Players ?? [];
        var rearranged = MatchSubstitutions.Rearranged(withBall, lines);
        var (lineup, substitutions) = await PlayersAsync(
            withBall, await outOfPossession, lines, people, await incidents, rearranged, cancellationToken);

        return new MatchDetail
        {
            Summary = summary,
            TeamStats = (await ours).Stats.Adapt<TeamMatchStats?>(),
            OpponentStats = (await theirs).Stats.Adapt<TeamMatchStats?>(),
            Goals = MatchIncidents.Goals(await incidents, isHome),
            RedCards = MatchIncidents.RedCards(await incidents, isHome),
            PenaltyShootout = MatchIncidents.Shootout(await incidents, isHome),
            Lineup = lineup,
            Substitutions = substitutions,
            LineupRearranged = rearranged,
            PlayerOfTheMatch = PlayerOfTheMatch(await events, people),
            Analysis = Analysis(await events)
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

    // Each dated fixture (one still to be arranged has no date to show) with its place in the fixture list.
    private static List<(MatchSummary Summary, int Index)> Summaries(BridgeListDto<BridgeFixtureDto> fixtures, int? teamId)
    {
        return fixtures.Items
            .Select((fixture, index) => (Fixture: fixture.Values, Index: index))
            .Where(row => row.Fixture.HasDate)
            .Select(row =>
            {
                var summary = row.Fixture.Adapt<MatchSummary>();
                summary.TeamId = teamId ?? 0;
                summary.Formation = summary.IsPlayed ? LineupShapes.Of(row.Fixture.Lineup ?? []) : null;
                return (summary, row.Index);
            })
            .ToList();
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
            Fixtures().Index(match).Props(MatchConstants.Events),
            cancellationToken);

        return events.Items.FirstOrDefault()?.Values ?? new BridgeMatchEventsDto();
    }

    // The match's goals, red cards and shoot-out penalties, oldest first, read as a list so each one's players can be
    // named, whichever side they play for. A goal no one set up leaves its assister's name unanswered.
    private async Task<List<BridgeIncidentDto>> IncidentsAsync(int match, CancellationToken cancellationToken)
    {
        var incidents = await bridge.GetListAsync<BridgeIncidentDto>(
            BridgeQuery.FromHuman()
                .List(MatchConstants.OfFixture(match, MatchConstants.Incidents))
                .FixtureWindow()
                .WithHumanAndTeam()
                .Context(MatchConstants.ShowAllEventTypes, true)
                .Props(MatchConstants.IncidentProps),
            cancellationToken,
            gapsExpected: true);

        return incidents.Items.Select(item => item.Values).ToList();
    }

    // The user's players in the match as the match names them, by database index: it still knows those who have since
    // left the club.
    private async Task<Dictionary<int, BridgeMatchPersonDto>> PeopleAsync(int match, bool isHome, CancellationToken cancellationToken)
    {
        var people = await bridge.GetListAsync<BridgeMatchPersonDto>(
            BridgeQuery.FromHuman()
                .List(MatchConstants.OfFixture(match, MatchConstants.PlayerStats))
                .FixtureWindow()
                .WithHumanAndTeam()
                .Context(MatchConstants.IsHomeTeam, isHome)
                .Props(MatchConstants.PlayerStatsPeople),
            cancellationToken);

        return people.Items
            .Select(item => item.Values)
            .Where(person => person.Player?.Index is not null)
            .GroupBy(person => person.Player!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First());
    }

    private static List<string> Analysis(BridgeMatchEventsDto events)
    {
        return (events.Analysis ?? [])
            .Select(point => point.AnalysisPointString)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text!)
            .ToList();
    }

    // The players who took part, with their positions in both phases (a substitute's those of the slot he took), when
    // they came on and off, and the substitutions.
    private async Task<(List<MatchLineupPlayer> Players, List<MatchSubstitution> Substitutions)> PlayersAsync(
        List<BridgeLineupEntryDto> withBall,
        List<BridgeLineupEntryDto> withoutBall,
        List<BridgePlayerMatchStatsDto> stats,
        MatchPeople people,
        List<BridgeIncidentDto> incidents,
        bool rearranged,
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
            var member = person is { } index ? people.Member(index) : null;
            var line = person is { } key ? statsByPlayer.GetValueOrDefault(key) : null;
            var isSubstitute = entry.MatchWasSubbedOn == true;
            var cameOn = isSubstitute ? changes.FirstOrDefault(change => change.OnPlayer == person) : null;

            // A substitute plays in the slot of the player he replaced. Where that isn't known, the slot he finished in
            // says it, unless the game renumbered the slots during the match.
            var slot = !isSubstitute
                ? entry.TeamSelectionIndex
                : cameOn is { Uncertain: false, Slot: { } replacedSlot }
                    ? replacedSlot
                    : (cameOn is null && !rearranged && line is { PlayerTeamSelectionIndex: >= 0 } ? line.PlayerTeamSelectionIndex : null);

            var withBallEntry = !isSubstitute
                ? entry
                : (slot is { } taken ? startersBySlot.GetValueOrDefault(taken) : null);
            var withoutBallEntry = slot is { } held ? outOfPossession.GetValueOrDefault(held) : null;

            // The slot's role is named from the replaced player's role table when the substitute's lacks it (a table
            // only lists the positions its player can play).
            var replaced = isSubstitute && withBallEntry?.Player?.Index is { } starter ? people.Member(starter) : null;

            var inPossessionPosition = PositionAsync(member, withBallEntry, inPossession: true, replaced, cancellationToken);
            var outOfPossessionPosition = PositionAsync(member, withoutBallEntry, inPossession: false, replaced, cancellationToken);

            var minuteOn = !isSubstitute
                ? null
                : (line is { IncidentType: MatchConstants.CameOnIncident } ? line.MatchTime : cameOn?.Minute);

            // A player sent off keeps his first booking's minute on his line: the red card has the right one.
            var minuteOff = line is not { PlayerTeamSelectionIndex: < 0 }
                ? null
                : (line.RedCardedInMatch == true && person is { } sentOff ? MatchIncidents.SentOffMinute(incidents, sentOff) : null) ?? line.MatchTime;

            return new MatchLineupPlayer
            {
                SelectionIndex = entry.TeamSelectionIndex ?? -1,
                PlayerId = person is { } id ? people.Id(id) : null,
                Name = person is { } named ? people.Name(named) : null,
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
        return (lineupPlayers, changes.Select(change => Substitution(change, startersBySlot, people, lineupPlayers)).ToList());
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
        MatchPeople people,
        List<MatchLineupPlayer> players)
    {
        var offId = people.Id(change.OffPlayer);
        var slot = change is { Uncertain: false, Slot: { } held } ? startersBySlot.GetValueOrDefault(held) : null;

        return new MatchSubstitution
        {
            Minute = change.Minute,
            OffPlayerId = offId,
            Off = people.Name(change.OffPlayer),
            OnPlayerId = people.Id(change.OnPlayer),
            On = people.Name(change.OnPlayer),
            Position = slot?.Position is { } mask ? PositionMasks.ToPosition(mask) : null,
            Injury = offId is { } id && players.Any(player => player.PlayerId == id && player.WentOffInjured),
            Uncertain = change.Uncertain
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

    private static PlayerOfTheMatch? PlayerOfTheMatch(BridgeMatchEventsDto events, MatchPeople people)
    {
        if (string.IsNullOrEmpty(events.PlayerOfTheMatchName))
        {
            return null;
        }

        return new PlayerOfTheMatch
        {
            Name = events.PlayerOfTheMatchName,
            Rating = events.PlayerOfTheMatchRating,
            IsOurs = events.PlayerOfTheMatchId is { } id && people.IsOurs(id)
        };
    }

    private sealed record SquadMember(int ListIndex, int? Id, string? Name);

    // The user's players by database index: as the match names them (it still knows those who have since left the
    // club), and as squad members where they're still at the club, which their role tables need.
    private sealed class MatchPeople(
        IReadOnlyDictionary<int, SquadMember> squad, IReadOnlyDictionary<int, BridgeMatchPersonDto> inMatch)
    {
        public SquadMember? Member(int person) => squad.GetValueOrDefault(person);

        public int? Id(int person) => inMatch.GetValueOrDefault(person)?.Id ?? squad.GetValueOrDefault(person)?.Id;

        public string? Name(int person) => inMatch.GetValueOrDefault(person)?.Name ?? squad.GetValueOrDefault(person)?.Name;

        public bool IsOurs(int id) => inMatch.Values.Any(person => person.Id == id) || squad.Values.Any(member => member.Id == id);
    }
}
