using FootballManager.Application.Services;
using FootballManager.Domain.Teams;

namespace FootballManager.Tests;

public class TeamLookupTests
{
    private static readonly List<KnownTeam> Teams =
    [
        new(74010191, "Cardiff Draconians", "League"),
        new(74015339, "Trethomas Bluebirds", "League"),
        new(1860, "Some Club", "Cup"),
        new(555, "1860", "Cup"),
        new(556, "Real Madrid", "Cup")
    ];

    [Fact]
    public void A_team_is_found_by_its_id()
    {
        Assert.Equal("Trethomas Bluebirds", TeamsService.Find("74015339", Teams).Value.Name);
    }

    [Fact]
    public void A_name_made_of_digits_is_a_name_first()
    {
        Assert.Equal(555, TeamsService.Find("1860", Teams).Value.Id);
    }

    [Theory]
    [InlineData("cardiff draconians")]
    [InlineData("Draconians")]
    public void A_team_is_found_by_name_ignoring_case_or_by_part_of_it(string name)
    {
        Assert.Equal(74010191, TeamsService.Find(name, Teams).Value.Id);
    }

    [Fact]
    public void An_unknown_id_fails()
    {
        Assert.True(TeamsService.Find("123456", Teams).IsFailed);
    }

    [Fact]
    public void A_name_shared_by_several_teams_asks_for_more()
    {
        var found = TeamsService.Find("r", [new KnownTeam(1, "Rangers", "L"), new KnownTeam(2, "Raith Rovers", "L")]);

        Assert.True(found.IsFailed);
    }
}
