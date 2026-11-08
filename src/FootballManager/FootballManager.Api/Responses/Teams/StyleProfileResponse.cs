namespace FootballManager.Api.Responses.Teams;

/// <summary>"for" is how the team played per match, "against" what its opponents managed against it.</summary>
public class StyleProfileResponse
{
    public int Matches { get; set; }
    public required StyleAveragesResponse For { get; set; }
    public required StyleAveragesResponse Against { get; set; }
}
