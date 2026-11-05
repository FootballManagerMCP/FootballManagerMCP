namespace FootballManager.Api.Requests.Tactics.Editing;

public class LineupChangeRequest
{
    /// <summary>The player's FM id, as get_squad lists it.</summary>
    public int PlayerId { get; set; }

    /// <summary>FM's position codes: GK, DR, DCL, DC, DCR, DL, WBR, DMR, DM, MR, MC, AMR, AMC, ST, ...</summary>
    public ByPhaseRequest<string>? Position { get; set; }

    /// <summary>Role keys, as get_role_fit and get_tactics list them.</summary>
    public ByPhaseRequest<string>? Role { get; set; }

    /// <summary>Player instruction keys; a phase's list replaces all of that phase's instructions.</summary>
    public ByPhaseRequest<List<string>>? Instructions { get; set; }
}
