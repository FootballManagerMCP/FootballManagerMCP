namespace FootballManager.Api.Responses.Scouting;

public class ScoutResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Job { get; set; }
    public int? JudgingAbility { get; set; }
    public int? JudgingPotential { get; set; }
}
