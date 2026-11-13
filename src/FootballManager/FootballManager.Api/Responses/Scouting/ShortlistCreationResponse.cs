namespace FootballManager.Api.Responses.Scouting;

public class ShortlistCreationResponse
{
    public bool Applied { get; set; }
    public required string Name { get; set; }

    /// <summary>True once the game lists it; left out on a dry run.</summary>
    public bool? Created { get; set; }

    public List<ShortlistResponse> Shortlists { get; set; } = [];
}
