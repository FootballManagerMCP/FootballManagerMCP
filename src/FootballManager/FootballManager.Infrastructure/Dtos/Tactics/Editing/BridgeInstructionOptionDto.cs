namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>An option of a team instruction or player instruction group, from the plugin's catalogs.</summary>
internal sealed class BridgeInstructionOptionDto
{
    /// <summary>The game's identifier, e.g. "MuchHigherTempo" or "PI_SHOOT_MORE".</summary>
    public string? Option { get; set; }

    /// <summary>The value the tactic holds when it's chosen.</summary>
    public ulong? Id { get; set; }

    /// <summary>The game's label, in its display language.</summary>
    public string? Name { get; set; }
}
