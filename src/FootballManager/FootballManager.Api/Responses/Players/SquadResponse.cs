namespace FootballManager.Api.Responses.Players;

public class SquadResponse
{
    public required string TeamName { get; set; }
    public bool HiddenDataMasked { get; set; }
    public List<PlayerResponse> Players { get; set; } = [];
}
