namespace FootballManager.Domain.Scouting;

/// <summary>
/// The club's scouting budget for the season and what scouting has cost so far, in pounds sterling: the unit the game
/// keeps money in, whatever currency its screens show. Scouting has no price up front: an assignment costs travel,
/// accommodation and expenses as the scout works, more for each match he watches.
/// </summary>
public class ScoutingBudget
{
    public const string Currency = "GBP";

    public long? Budget { get; set; }
    public long? SpentThisSeason { get; set; }
    public long? SpentThisMonth { get; set; }

    public long? Remaining => Budget - SpentThisSeason;

    /// <summary>The share of the season's budget spent so far, in percent.</summary>
    public int? SpentPercentage => Budget is > 0 && SpentThisSeason is { } spent
        ? (int)Math.Round(100.0 * spent / Budget.Value)
        : null;
}
