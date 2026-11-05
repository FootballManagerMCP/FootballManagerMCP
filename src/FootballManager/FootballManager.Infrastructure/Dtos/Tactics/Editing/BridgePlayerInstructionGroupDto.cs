namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>A player instruction group as the plugin's /player-instructions describes it.</summary>
internal sealed class BridgePlayerInstructionGroupDto
{
    /// <summary>The game's identifier, e.g. "Shooting".</summary>
    public string? Group { get; set; }

    /// <summary>The PlayerInstructionGroupID context its option is read and written with.</summary>
    public int? GroupId { get; set; }

    public string? Name { get; set; }
    public List<BridgeInstructionOptionDto> Options { get; set; } = [];
}
