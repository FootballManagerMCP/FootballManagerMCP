namespace FootballManager.Api.Responses;

public class PositionAssignmentResponse
{
    /// <summary>FM's abbreviation, with L/R for the side of a central line: "DC", "DCL", "MCR", ...</summary>
    public string? Position { get; set; }

    public string? Role { get; set; }
    public string? RoleShortName { get; set; }

    /// <summary>0-20, the game's stars (20 = five).</summary>
    public string? Rating { get; set; }

    public List<RoleRatingResponse> RoleOptions { get; set; } = [];
}
