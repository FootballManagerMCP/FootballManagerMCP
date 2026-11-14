namespace FootballManager.Api.Requests.Scouting;

public class CreateShortlistRequest
{
    /// <summary>1 to 100 characters, not already a shortlist's name.</summary>
    public string Name { get; set; } = string.Empty;
}
