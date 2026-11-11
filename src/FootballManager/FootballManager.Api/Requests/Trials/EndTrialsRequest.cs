namespace FootballManager.Api.Requests.Trials;

public class EndTrialsRequest
{
    /// <summary>The players' FM ids: trialists from the squad, or players whose trial offer is still waiting.</summary>
    public List<int> PlayerIds { get; set; } = [];

    /// <summary>Needed to apply: how many players the dry run listed, as the user agreed to it.</summary>
    public int? ExpectedPlayers { get; set; }
}
