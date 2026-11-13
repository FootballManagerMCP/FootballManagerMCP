namespace FootballManager.Api.Responses.Scouting;

public class ShortlistResponse
{
    public required string Name { get; set; }
    public int? Players { get; set; }

    /// <summary>Only sent when true.</summary>
    public bool? IsDefault { get; set; }

    /// <summary>The list the shortlist screen shows, which get_scouted_players reads; only sent when true.</summary>
    public bool? IsSelected { get; set; }
}
