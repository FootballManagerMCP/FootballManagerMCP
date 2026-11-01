namespace FootballManager.Domain.Matches;

public class PlayerOfTheMatch
{
    public required string Name { get; set; }
    public double? Rating { get; set; }
    public bool IsOurs { get; set; }
}
