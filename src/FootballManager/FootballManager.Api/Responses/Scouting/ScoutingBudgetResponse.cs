namespace FootballManager.Api.Responses.Scouting;

/// <summary>
/// The season's scouting budget and spending, in pounds sterling: the unit the game keeps money in, whatever currency
/// its screens show.
/// </summary>
public class ScoutingBudgetResponse
{
    public required string Currency { get; set; }
    public long? Budget { get; set; }
    public long? SpentThisSeason { get; set; }
    public long? SpentThisMonth { get; set; }
    public long? Remaining { get; set; }
    public int? SpentPercentage { get; set; }
}
