using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Application.Services;
using FootballManager.Domain.Competitions;
using FootballManager.Domain.Scouting;
using FootballManager.Domain.Tactics;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FootballManager.Tests;

public class CompetitionsServiceTests
{
    private const int UserTeam = 1;
    private const int OtherTeam = 2;
    private const int SouthId = 29236870;
    private const int NorthId = 29236869;

    private static readonly CompetitionInfo South = new() { Id = SouthId, Name = "Novira Cymru South", Nation = "Pays de Galles", Reputation = 32, IsUserLeague = true };
    private static readonly CompetitionInfo North = new() { Id = NorthId, Name = "Novira Cymru North", Nation = "Pays de Galles", Reputation = 32 };
    private static readonly CompetitionInfo Premier = new() { Id = 130672, Name = "Novira Cymru Premier", Nation = "Pays de Galles", Reputation = 64 };

    private readonly ICompetitionsGameClient _game = Substitute.For<ICompetitionsGameClient>();
    private readonly CompetitionsService _competitions;

    public CompetitionsServiceTests()
    {
        _competitions = new CompetitionsService(_game);
        _game.GetUserLeagueAsync(Arg.Any<CancellationToken>()).Returns(South);
        _game.GetUserTeamIdAsync(Arg.Any<CancellationToken>()).Returns(UserTeam);
        _game.GetCompetitionsAsync(Arg.Any<CancellationToken>()).Returns([South, Premier, North]);
        _game.GetStatTableAsync(Arg.Any<int>(), CompetitionStat.TacklesWon, Arg.Any<CancellationToken>()).Returns(Table(
            CompetitionStat.TacklesWon,
            Row(10, "Cameron Gauci", 26, OtherTeam, 2491, 33, 1, [PitchPosition.DL, PitchPosition.WBL]),
            Row(11, "Cian Williams", 23, UserTeam, 2365, 32, 2, [PitchPosition.DL]),
            Row(12, "Tom Davies", 27, OtherTeam, 2679, 32, 2, [PitchPosition.ML, PitchPosition.AML]),
            Row(13, "John Davies", 17, OtherTeam, 2971, 30, 4, [PitchPosition.AMR]),
            Row(14, "Sub", 30, OtherTeam, 400, 29, 5, [PitchPosition.DL])));
        _game.GetStatTableAsync(Arg.Any<int>(), CompetitionStat.Interceptions, Arg.Any<CancellationToken>()).Returns(Table(
            CompetitionStat.Interceptions,
            Row(13, "John Davies", 17, OtherTeam, 2971, 88, 1, [PitchPosition.AMR]),
            Row(10, "Cameron Gauci", 26, OtherTeam, 2491, 76, 2, [PitchPosition.DL, PitchPosition.WBL])));
        _game.GetPlayersAsync(Arg.Any<int>(), Arg.Any<IReadOnlyList<CompetitionRowReference>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IReadOnlyList<CompetitionRowReference>>()
                .Select(row => new CompetitionPlayerProfile { Player = new ScoutedPlayer { Id = row.RowIndex, Name = $"row {row.RowIndex}" } })
                .ToList());
    }

    [Fact]
    public async Task No_stat_is_refused_without_asking_the_game()
    {
        var result = await _competitions.GetPlayerStatsAsync(new CompetitionStatsQuery());

        Assert.True(result.IsFailed);
        Assert.True(result.HasError<ValidationError>());
        await _game.DidNotReceiveWithAnyArgs().GetStatTableAsync(default, default);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task A_limit_out_of_range_is_refused(int limit)
    {
        var result = await _competitions.GetPlayerStatsAsync(Query(limit: limit));

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task More_than_ten_stats_are_refused()
    {
        var result = await _competitions.GetPlayerStatsAsync(new CompetitionStatsQuery
        {
            Stats = Enum.GetValues<CompetitionStat>().Take(CompetitionsService.MaxStats + 1).ToList()
        });

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task A_minimum_age_above_the_maximum_is_refused()
    {
        var result = await _competitions.GetPlayerStatsAsync(Query(minAge: 25, maxAge: 20));

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task The_user_league_is_read_by_default_without_listing_the_others()
    {
        var stats = (await _competitions.GetPlayerStatsAsync(Query())).Value;

        Assert.Equal(SouthId, stats.Competition.Id);
        await _game.DidNotReceiveWithAnyArgs().GetCompetitionsAsync(default);
        await _game.Received().GetStatTableAsync(SouthId, CompetitionStat.TacklesWon, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("novira cymru north")]
    [InlineData("north")]
    [InlineData("29236869")]
    public async Task Another_league_is_found_by_name_part_of_it_or_id(string competition)
    {
        var stats = (await _competitions.GetPlayerStatsAsync(Query(competition: competition))).Value;

        Assert.Equal(NorthId, stats.Competition.Id);
    }

    [Fact]
    public void A_name_matching_several_leagues_is_refused()
    {
        var found = CompetitionsService.Find("cymru", [South, Premier, North]);

        Assert.True(found.IsFailed);
        Assert.Contains("several", Assert.IsType<ValidationError>(found.Errors[0]).ErrorMessages.Single());
    }

    [Fact]
    public void An_unknown_league_lists_those_with_stats()
    {
        var found = CompetitionsService.Find("Ligue 1", [South, North]);

        var message = Assert.IsType<ValidationError>(found.Errors[0]).ErrorMessages.Single();
        Assert.Contains("Novira Cymru North", message);
        Assert.Contains("reputation 32", message);
    }

    [Fact]
    public async Task Players_come_in_the_sort_tables_order_without_the_users_own()
    {
        var stats = (await _competitions.GetPlayerStatsAsync(Query())).Value;

        Assert.Equal(["row 0", "row 2", "row 3", "row 4"], stats.Players.Select(player => player.Player.Name));
        Assert.Equal(4, stats.Matched);
        Assert.Equal(CompetitionStat.TacklesWon, stats.SortedBy);
    }

    [Fact]
    public async Task The_users_own_players_are_listed_when_asked_for()
    {
        var stats = (await _competitions.GetPlayerStatsAsync(Query(includeOwnPlayers: true))).Value;

        Assert.Equal(5, stats.Matched);
        await _game.DidNotReceiveWithAnyArgs().GetUserTeamIdAsync(default);
    }

    [Fact]
    public async Task Filters_narrow_the_table_down_and_the_limit_cuts_it()
    {
        var stats = (await _competitions.GetPlayerStatsAsync(Query(positions: [PitchPosition.DL], minMinutes: 1000, maxAge: 30, limit: 1))).Value;

        var gauci = Assert.Single(stats.Players);
        Assert.Equal(1, stats.Matched);
        Assert.Equal(["DL", "WBL"], gauci.Positions.Select(position => position.ToString()));
        Assert.Equal(2491, gauci.Minutes);
    }

    [Fact]
    public async Task Only_the_players_kept_are_read_in_full_from_the_sort_table()
    {
        await _competitions.GetPlayerStatsAsync(Query(minAge: 18, maxAge: 26));

        await _game.Received().GetPlayersAsync(
            SouthId,
            Arg.Is<IReadOnlyList<CompetitionRowReference>>(rows =>
                rows.SequenceEqual(new[] { new CompetitionRowReference(CompetitionStat.TacklesWon, 0) })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Other_stats_are_added_where_the_player_is_in_their_table()
    {
        var stats = (await _competitions.GetPlayerStatsAsync(Query(stats: [CompetitionStat.TacklesWon, CompetitionStat.Interceptions]))).Value;

        var gauci = stats.Players[0];
        Assert.Equal([33.0, 76.0], gauci.Stats.Select(stat => stat.Value));
        Assert.Equal(2, gauci.Stats[1].Rank);
        var tom = stats.Players[1];
        Assert.Equal(CompetitionStat.TacklesWon, Assert.Single(tom.Stats).Stat);
    }

    [Fact]
    public async Task The_sort_stat_leads_even_when_named_after_the_others()
    {
        var stats = (await _competitions.GetPlayerStatsAsync(Query(stats: [CompetitionStat.TacklesWon], sortBy: CompetitionStat.Interceptions))).Value;

        Assert.Equal(CompetitionStat.Interceptions, stats.SortedBy);
        Assert.Equal([CompetitionStat.Interceptions, CompetitionStat.TacklesWon], stats.Stats.Select(column => column.Stat));
        Assert.Equal(["row 0", "row 1"], stats.Players.Select(player => player.Player.Name));
    }

    [Fact]
    public async Task A_game_that_cant_be_reached_is_reported()
    {
        _game.GetUserLeagueAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new GameUnavailableException("Football Manager isn't running."));

        var result = await _competitions.GetPlayerStatsAsync(Query());

        Assert.True(result.HasError<GameUnavailableError>());
    }

    private static CompetitionStatsQuery Query(
        List<CompetitionStat>? stats = null,
        CompetitionStat? sortBy = null,
        string? competition = null,
        List<PitchPosition>? positions = null,
        int? minAge = null,
        int? maxAge = null,
        int? minMinutes = null,
        bool includeOwnPlayers = false,
        int limit = 25)
    {
        return new CompetitionStatsQuery
        {
            Stats = stats ?? [CompetitionStat.TacklesWon],
            SortBy = sortBy,
            Competition = competition,
            Positions = positions ?? [],
            MinAge = minAge,
            MaxAge = maxAge,
            MinMinutes = minMinutes,
            IncludeOwnPlayers = includeOwnPlayers,
            Limit = limit
        };
    }

    private static CompetitionStatTable Table(CompetitionStat stat, params CompetitionStatRow[] rows)
    {
        for (var index = 0; index < rows.Length; index++)
        {
            rows[index].RowIndex = index;
        }

        return new CompetitionStatTable { Stat = stat, Label = stat.ToString(), Rows = rows.ToList() };
    }

    private static CompetitionStatRow Row(int id, string name, int age, int team, double minutes, double value, int rank, List<PitchPosition> positions)
    {
        return new CompetitionStatRow
        {
            PlayerId = id,
            Name = name,
            Age = age,
            TeamId = team,
            Minutes = minutes,
            Value = value,
            Rank = rank,
            Positions = positions
        };
    }
}
