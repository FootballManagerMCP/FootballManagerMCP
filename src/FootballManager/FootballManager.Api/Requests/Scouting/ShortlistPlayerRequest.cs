using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Requests.Scouting;

public class ShortlistPlayerRequest
{
    /// <summary>The shortlist to add him to, by its name as get_scouting lists it; needed once there are several.</summary>
    public string? Shortlist { get; set; }

    public ShortlistDuration Duration { get; set; } = ShortlistDuration.Indefinitely;
}
