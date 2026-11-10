namespace FootballManager.Api.Responses.Matches;

public class MatchPositionResponse
{
    /// <summary>FM's abbreviation, with L/R for the side of a central line: "DC", "DCL", "MCR", ...</summary>
    public string? Position { get; set; }

    /// <summary>The game's English identifier, as tactic edits take it.</summary>
    public string? RoleKey { get; set; }

    public string? Role { get; set; }
    public string? RoleShortName { get; set; }
}
