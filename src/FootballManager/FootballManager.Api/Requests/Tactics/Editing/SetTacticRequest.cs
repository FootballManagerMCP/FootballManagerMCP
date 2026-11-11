namespace FootballManager.Api.Requests.Tactics.Editing;

/// <summary>What to change in a tactic slot, by key (get_tactic_options lists them). Anything left out stays.</summary>
public class SetTacticRequest
{
    /// <summary>The slot to write, 1 to 3. Writing makes it the slot the team plays.</summary>
    public int Slot { get; set; }

    public string? Style { get; set; }
    public string? Mentality { get; set; }

    /// <summary>Team instruction key to option key.</summary>
    public Dictionary<string, string>? TeamInstructions { get; set; }

    public ByPhaseRequest<string>? Formation { get; set; }

    /// <summary>Players of the starting eleven to place; bringing a player in needs all eleven.</summary>
    public List<LineupChangeRequest>? Lineup { get; set; }

    /// <summary>The substitutes by FM id, in the bench's order; left out, the bench stays as it is.</summary>
    public List<int>? Bench { get; set; }
}
