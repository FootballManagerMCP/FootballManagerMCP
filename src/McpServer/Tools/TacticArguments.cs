using System.ComponentModel;

namespace McpServer.Tools;

[Description("A value for each phase of play; leave either out to keep it.")]
public sealed class PhaseValues
{
    [Description("With the ball.")]
    public string? InPossession { get; set; }

    [Description("Without the ball.")]
    public string? OutOfPossession { get; set; }
}

[Description("A list for each phase of play; leave either out to keep that phase as it is.")]
public sealed class PhaseLists
{
    [Description("With the ball.")]
    public List<string>? InPossession { get; set; }

    [Description("Without the ball.")]
    public List<string>? OutOfPossession { get; set; }
}

[Description("A player of the starting eleven and where and how they play. Anything left out stays as it is.")]
public sealed class LineupEntry
{
    [Description("The player's FM id, as get_squad and get_tactics list it.")]
    public int PlayerId { get; set; }

    [Description("FM's position codes, English, with L/R for the side of a central line: GK, DR, DCR, DC, DCL, DL, " +
                 "WBR, WBL, DMR, DM, DML, MR, MCR, MC, MCL, ML, AMR, AMCR, AMC, AMCL, AML, STR, ST, STL.")]
    public PhaseValues? Position { get; set; }

    [Description("Role keys, e.g. CentreForward in possession and TrackingCentreForward out of it. get_role_fit lists " +
                 "each position's roles with their keys and who suits them.")]
    public PhaseValues? Role { get; set; }

    [Description("Player instruction keys from get_tactic_options (at most one per group), e.g. ShootMore. A phase's " +
                 "list replaces all of that phase's instructions: an empty list clears them. Some groups are set by the " +
                 "role or position (a defensive midfielder always holds position): those keep their option, and asking " +
                 "for another fails, naming the one the role sets.")]
    public PhaseLists? Instructions { get; set; }
}
