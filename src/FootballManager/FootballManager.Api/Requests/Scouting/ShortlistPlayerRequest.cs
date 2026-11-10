using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Requests.Scouting;

public class ShortlistPlayerRequest
{
    public ShortlistDuration Duration { get; set; } = ShortlistDuration.Indefinitely;
}
