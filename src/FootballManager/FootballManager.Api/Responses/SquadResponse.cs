namespace FootballManager.Api.Responses;

public class SquadResponse
{
    public required string TeamName { get; set; }
    public bool HiddenDataMasked { get; set; }
    public List<PlayerResponse> Players { get; set; } = [];
}
