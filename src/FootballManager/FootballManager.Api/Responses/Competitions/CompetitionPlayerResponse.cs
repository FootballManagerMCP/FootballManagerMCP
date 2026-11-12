using FootballManager.Api.Responses.Players;
using FootballManager.Api.Responses.Scouting;

namespace FootballManager.Api.Responses.Competitions;

public class CompetitionPlayerResponse
{
    /// <summary>As the user's scouting knows him.</summary>
    public required ScoutedPlayerResponse Player { get; set; }

    public int? TeamId { get; set; }

    /// <summary>Every position he can play, as FM's English codes.</summary>
    public List<string> PositionCodes { get; set; } = [];

    /// <summary>His club season so far, all competitive matches together.</summary>
    public double? Minutes { get; set; }

    /// <summary>By stat: his value and his rank in its table; a stat he isn't in the top 100 for is left out.</summary>
    public Dictionary<string, CompetitionStatValueResponse> Stats { get; set; } = [];

    public PlayerContractResponse? Contract { get; set; }
}
