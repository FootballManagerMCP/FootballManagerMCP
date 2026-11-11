namespace FootballManager.Api.Requests.Trials;

public class OfferTrialsRequest
{
    /// <summary>The players' FM ids, as the scouting lists, the player search or the database search give them.</summary>
    public List<int> PlayerIds { get; set; } = [];

    /// <summary>How long each trial lasts: 1 to 4 weeks.</summary>
    public int Weeks { get; set; }

    /// <summary>Needed to apply: how many players the dry run listed, as the user agreed to it.</summary>
    public int? ExpectedPlayers { get; set; }
}
