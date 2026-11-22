using FootballManager.Domain.Matches;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Tests;

public class MatchIncidentsTests
{
    private const int Home = 0;
    private const int Away = 1;

    private readonly TypeAdapterConfig _config = new();

    // As at Ammanford away: our goal, an own goal by their defender that counts for us, our red card, their penalty.
    private static readonly List<BridgeIncidentDto> Ammanford =
    [
        Incident(MatchConstants.GoalIncident, Away, "33'", "Eddie Servuts", "Damar Palmer"),
        Incident(MatchConstants.OwnGoalIncident, Away, "41'", "Myles Hart"),
        Incident(MatchConstants.RedCardIncident, Away, "90+1'", "Joe Murphy", index: 269830),
        Incident(MatchConstants.PenaltyGoalIncident, Home, "90+2'", "Mateo Moon-Arnaez")
    ];

    public MatchIncidentsTests()
    {
        new MappingConfig().Register(_config);
    }

    [Fact]
    public void Penalties_and_own_goals_count_as_goals_for_the_side_they_were_scored_for()
    {
        var goals = MatchIncidents.Goals(Ammanford, isHome: false, _config);

        Assert.Equal(3, goals.Count);
        Assert.Equal([true, true, false], goals.Select(goal => goal.IsOurs));
        Assert.Equal([GoalKind.Goal, GoalKind.OwnGoal, GoalKind.Penalty], goals.Select(goal => goal.Kind));
        Assert.Equal("Myles Hart", goals[1].Scorer);
        Assert.Equal("Damar Palmer", goals[0].Assister);
    }

    [Fact]
    public void A_red_card_is_named_with_its_minute_and_side()
    {
        var card = Assert.Single(MatchIncidents.RedCards(Ammanford, isHome: false, _config));

        Assert.True(card.IsOurs);
        Assert.Equal("Joe Murphy", card.Player);
        Assert.Equal("90+1'", card.Minute);
        Assert.Equal(91, MatchIncidents.SentOffMinute(Ammanford, 269830));
    }

    [Fact]
    public void A_shoot_out_counts_each_sides_penalties_scored()
    {
        List<BridgeIncidentDto> incidents =
        [
            Incident(MatchConstants.ShootoutScoredIncident, Away, "90+7'", "Joe Murphy"),
            Incident(MatchConstants.ShootoutScoredIncident, Home, "90+7'", "Bartosz Urbański"),
            Incident(MatchConstants.ShootoutMissedIncident, Away, "90+7'", "Bradley Gibbings"),
            Incident(MatchConstants.ShootoutScoredIncident, Home, "90+7'", "Joel Giblin")
        ];

        var shootout = MatchIncidents.Shootout(incidents, isHome: false, _config);

        Assert.NotNull(shootout);
        Assert.Equal(1, shootout.Scored);
        Assert.Equal(2, shootout.Conceded);
        Assert.Equal(4, shootout.Kicks.Count);
        Assert.Empty(MatchIncidents.Goals(incidents, isHome: false, _config));
    }

    [Fact]
    public void A_match_without_a_shoot_out_has_none()
    {
        Assert.Null(MatchIncidents.Shootout(Ammanford, isHome: false, _config));
    }

    [Theory]
    [InlineData("21'", 21)]
    [InlineData("90+2'", 92)]
    [InlineData("45+1'", 46)]
    [InlineData(null, null)]
    [InlineData("", null)]
    public void Minutes_read_as_the_game_writes_them(string? text, int? minute)
    {
        Assert.Equal(minute, MatchIncidents.Minute(text));
    }

    private static BridgeIncidentDto Incident(int type, int side, string minute, string name, string? second = null, int? index = null)
    {
        return new BridgeIncidentDto
        {
            IncidentType = type,
            Side = side,
            Minute = minute,
            Player = new BridgeReferenceDto { Type = "Person", Index = index },
            PlayerName = name,
            SecondPlayerName = second
        };
    }
}
