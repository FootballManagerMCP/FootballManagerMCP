using FootballManager.Application.Interfaces;
using FootballManager.Domain.Database;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Players;
using FootballManager.Domain.Teams;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Dtos.Teams;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads other teams through the MCPFM plugin. A team is reached through a list that holds it (the user's league, or a
// competition of one of the user's fixtures, from the user's manager; or the game's search results, from the game) and
// read like the user's own: its fixture list, each match's numbers and lineups, its squad. The team goes along as the
// "team" context, which decides whose lineup a past match gives.
internal sealed class TeamsBridgeClient(GameBridge bridge, GameVocabulary vocabulary, DatabaseSearchBridgeClient search)
    : ITeamsGameClient
{
    // What the user's scouting can tell of another team's players, and their season.
    private static readonly string[] SquadPaths =
    [
        GameConstants.UniqueId, GameConstants.Name, GameConstants.Age, GameConstants.Positions, GameConstants.BestRole,
        GameConstants.IsInjured, GameConstants.AverageRating, .. GameConstants.PlayerSeasonStats
    ];

    // Where each known team is, by id, once listed: its path, from the game root when found by the search.
    private readonly Dictionary<int, (string Path, string Name, bool OnGame)> _teams = [];

    public async Task<List<KnownTeam>> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        var league = bridge.GetRecordAsync<BridgeLeagueDto>(BridgeQuery.FromHuman().Props([TeamConstants.LeagueName]), cancellationToken);
        var leagueTeams = TeamListAsync(TeamConstants.LeagueTeams, cancellationToken);
        var fixtures = bridge.GetListAsync<BridgeFixtureDto>(
            BridgeQuery.FromHuman()
                .List(TeamConstants.OwnFixtures)
                .FixtureWindow()
                .Props([MatchConstants.Date, MatchConstants.Competition, MatchConstants.IsFriendly]),
            cancellationToken,
            gapsExpected: true);

        await Task.WhenAll(league, leagueTeams, fixtures);

        var leagueName = (await league).Values.Name ?? string.Empty;
        var known = new List<KnownTeam>();
        Add(known, await leagueTeams, TeamConstants.LeagueTeams, leagueName);

        // Every other competition the user plays in this season, through the first of its fixtures.
        var competitions = (await fixtures).Items
            .Select((fixture, index) => (fixture.Values, Path: TeamConstants.Item(TeamConstants.OwnFixtures, index)))
            .Where(fixture => fixture.Values.IsFriendly != true && fixture.Values.Competition is { } name && name != leagueName)
            .GroupBy(fixture => fixture.Values.Competition!)
            .Select(group => (Name: group.Key, TeamsPath: $"{group.First().Path}.{TeamConstants.CompetitionTeams}"))
            .ToList();

        var lists = await Task.WhenAll(competitions.Select(competition => TeamListAsync(competition.TeamsPath, cancellationToken)));

        for (var i = 0; i < competitions.Count; i++)
        {
            Add(known, lists[i], competitions[i].TeamsPath, competitions[i].Name);
        }

        return known;
    }

    public async Task<KnownTeam?> GetSearchedTeamAsync(int clubId, CancellationToken cancellationToken = default)
    {
        if (await search.LocateAsync(DatabaseRecordKind.Club, clubId, cancellationToken) is not { } club)
        {
            return null;
        }

        var path = $"{club}.{TeamConstants.MainTeam}";
        var team = await bridge.GetRecordAsync<BridgeSearchedTeamDto>(
            BridgeQuery.FromGame().Base(path).Props([GameConstants.UniqueId, GameConstants.Name, TeamConstants.DivisionName]),
            cancellationToken);

        if (team.Values is not { Id: { } id, Name: { } name })
        {
            return null;
        }

        _teams[id] = (path, name, OnGame: true);
        return new KnownTeam(id, name, team.Values.League ?? string.Empty);
    }

    /// <summary>
    /// Where a team is, for other clients to reach it (its squad): the root its path starts from, the path, and its name.
    /// Null for a team neither the user's competitions nor the game's search have listed.
    /// </summary>
    public async Task<(string Root, string Path, string Name)?> LocateAsync(int teamId, CancellationToken cancellationToken = default)
    {
        if (!_teams.ContainsKey(teamId))
        {
            await GetTeamsAsync(cancellationToken);
        }

        if (!_teams.ContainsKey(teamId))
        {
            await GetSearchedTeamAsync(teamId, cancellationToken);
        }

        return _teams.TryGetValue(teamId, out var team)
            ? (team.OnGame ? GameConstants.GameRoot : GameConstants.HumanRoot, team.Path, team.Name)
            : null;
    }

    public async Task<TeamSeason> GetSeasonAsync(int teamId, CancellationToken cancellationToken = default)
    {
        if (!_teams.ContainsKey(teamId))
        {
            await GetTeamsAsync(cancellationToken);
        }

        var team = _teams.TryGetValue(teamId, out var known)
            ? known
            : throw new ArgumentException($"Team {teamId} isn't in the user's league or competitions.", nameof(teamId));

        try
        {
            return await ReadSeasonAsync(teamId, team, cancellationToken);
        }
        finally
        {
            await OwnTeam.RestoreAsync(bridge);
        }
    }

    // Five reads in all, each over the whole fixture list (rows in the same order): the summaries with the lineup in
    // possession, the home side's numbers, the away side's, the lineup out of possession; and the squad.
    private async Task<TeamSeason> ReadSeasonAsync(
        int teamId, (string Path, string Name, bool OnGame) team, CancellationToken cancellationToken)
    {
        var (path, name, _) = team;
        var fixtureList = $"{path}.{TeamConstants.FixtureList}";

        var keys = vocabulary.KeysAsync(cancellationToken);
        var fixtures = bridge.GetListAsync<BridgeFixtureDto>(
            OnTeam(team).List(fixtureList).Context(TacticConstants.InPossession, true).Props(MatchConstants.Summary),
            cancellationToken,
            gapsExpected: true);
        var homeStats = SidesAsync(team, fixtureList, isHome: true, cancellationToken);
        var awayStats = SidesAsync(team, fixtureList, isHome: false, cancellationToken);
        var withoutBall = bridge.GetListAsync<BridgeMatchLineupDto>(
            OnTeam(team).List(fixtureList).Context(TacticConstants.InPossession, false).Props([MatchConstants.Lineup]),
            cancellationToken,
            gapsExpected: true);
        var squad = bridge.GetListAsync<BridgePlayerDto>(
            OnTeam(team).List($"{path}.{TeamConstants.Players}").Props(SquadPaths),
            cancellationToken,
            gapsExpected: true);

        await Task.WhenAll(keys, fixtures, homeStats, awayStats, withoutBall, squad);

        // A match names its players by their place in the game's database, which the squad's rows give.
        var players = (await squad).Items
            .Where(player => player.Source?.Index is not null)
            .GroupBy(player => player.Source!.Index!.Value)
            .ToDictionary(group => group.Key, group => group.First().Values.Adapt<Player>());

        var names = await keys;
        var home = (await homeStats).Items;
        var away = (await awayStats).Items;
        var outOfPossession = (await withoutBall).Items;

        // The stats lists follow the fixture list row for row: undated fixtures are left out after pairing.
        var matches = (await fixtures).Items
            .Select((fixture, index) => (Fixture: fixture.Values, Index: index))
            .Where(row => row.Fixture.HasDate)
            .Select(row => Match(
                teamId,
                row.Fixture,
                home.ElementAtOrDefault(row.Index)?.Values.Stats,
                away.ElementAtOrDefault(row.Index)?.Values.Stats,
                outOfPossession.ElementAtOrDefault(row.Index)?.Values.Entries ?? [],
                players,
                names));

        return new TeamSeason
        {
            TeamId = teamId,
            TeamName = name,
            Matches = matches.ToList(),
            Squad = players.Values.ToList()
        };
    }

    // The team as context, and the fixture list sized like the user's (its path may go through the user's fixtures).
    private static BridgeQuery OnTeam((string Path, string Name, bool OnGame) team)
    {
        return (team.OnGame ? BridgeQuery.FromGame() : BridgeQuery.FromHuman())
            .FixtureWindow()
            .Ref(TeamConstants.TeamContext, team.Path);
    }

    private Task<BridgeListDto<BridgeTeamDto>> TeamListAsync(string path, CancellationToken cancellationToken)
    {
        return bridge.GetListAsync<BridgeTeamDto>(
            BridgeQuery.FromHuman().FixtureWindow().List(path).Props([GameConstants.UniqueId, GameConstants.Name]),
            cancellationToken,
            gapsExpected: true);
    }

    // A team keeps the first list it's found in; it's reached by its place in that list.
    private void Add(List<KnownTeam> known, BridgeListDto<BridgeTeamDto> teams, string listPath, string competition)
    {
        for (var i = 0; i < teams.Items.Count; i++)
        {
            if (teams.Items[i].Values is { Id: { } id, Name: { } name } && known.All(team => team.Id != id))
            {
                known.Add(new KnownTeam(id, name, competition));
                _teams[id] = (TeamConstants.Item(listPath, i), name, OnGame: false);
            }
        }
    }

    private static TeamMatch Match(
        int teamId, BridgeFixtureDto fixture, BridgeTeamMatchStatsDto? home, BridgeTeamMatchStatsDto? away,
        List<BridgeLineupEntryDto> withoutBall, Dictionary<int, Player> players, TacticKeys keys)
    {
        var summary = fixture.Adapt<MatchSummary>();
        summary.TeamId = teamId;

        // Friendlies and matches to come carry no numbers worth reading.
        if (!summary.IsPlayed || summary.IsFriendly)
        {
            return new TeamMatch { Summary = summary };
        }

        var isHome = summary.Venue == Venue.Home;
        var withBall = fixture.Lineup ?? [];
        summary.Formation = LineupShapes.Of(withBall);
        summary.OutOfPossessionFormation = LineupShapes.Of(withoutBall);

        var stats = (isHome ? home : away).Adapt<TeamMatchStats?>();
        var opponentStats = (isHome ? away : home).Adapt<TeamMatchStats?>();

        // A result the game only simulated has no numbers: zero shots and zero xG on both sides.
        var simulated = Empty(stats) && Empty(opponentStats);

        return new TeamMatch
        {
            Summary = summary,
            Stats = simulated ? null : stats,
            OpponentStats = simulated ? null : opponentStats,
            Lineup = Lineup(withBall, withoutBall, players, keys)
        };
    }

    // One side's numbers in every fixture of the list.
    private Task<BridgeListDto<BridgeMatchSideDto>> SidesAsync(
        (string Path, string Name, bool OnGame) team, string fixtureList, bool isHome, CancellationToken cancellationToken)
    {
        return bridge.GetListAsync<BridgeMatchSideDto>(
            OnTeam(team).List(fixtureList).Context(MatchConstants.IsHomeTeam, isHome).Props([MatchConstants.TeamStats]),
            cancellationToken,
            gapsExpected: true);
    }

    // The match's players in selection order, each with his position and role in both phases (see LineupPairing).
    private static List<TeamLineupEntry> Lineup(
        List<BridgeLineupEntryDto> withBall, List<BridgeLineupEntryDto> withoutBall, Dictionary<int, Player> players, TacticKeys keys)
    {
        var outOfPossession = LineupPairing.OutOfPossessionBySlot(withBall, withoutBall);

        return withBall.OrderBy(entry => entry.TeamSelectionIndex).Select(entry =>
        {
            var player = entry.Player?.Index is { } index ? players.GetValueOrDefault(index) : null;
            var other = entry.TeamSelectionIndex is { } slot ? outOfPossession.GetValueOrDefault(slot) : null;

            return new TeamLineupEntry
            {
                PlayerId = player?.Id,
                Name = player?.Name,
                IsStarter = LineupShapes.IsStarter(entry),
                InPossessionPosition = entry.Position is { } mask ? PositionMasks.ToPosition(mask) : null,
                InPossessionRole = entry.PositionRole is { } role ? keys.InPossessionRoles.GetValueOrDefault(role) : null,
                OutOfPossessionPosition = other?.Position is { } otherMask ? PositionMasks.ToPosition(otherMask) : null,
                OutOfPossessionRole = other?.PositionRole is { } otherRole ? keys.OutOfPossessionRoles.GetValueOrDefault(otherRole) : null
            };
        }).ToList();
    }

    private static bool Empty(TeamMatchStats? stats)
    {
        return stats is null || ((stats.Shots ?? 0) == 0 && (stats.ExpectedGoals ?? 0) == 0);
    }
}
