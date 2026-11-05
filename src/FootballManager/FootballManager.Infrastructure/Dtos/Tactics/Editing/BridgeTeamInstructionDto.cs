namespace FootballManager.Infrastructure.Dtos.Tactics.Editing;

/// <summary>A team instruction as the plugin's /instructions describes it, from the tactics screen's own data.</summary>
internal sealed class BridgeTeamInstructionDto
{
    /// <summary>The game's identifier, e.g. "Tempo".</summary>
    public string? Instruction { get; set; }

    /// <summary>The TeamInstructionID context its option is read and written with.</summary>
    public int? TeamInstructionId { get; set; }

    /// <summary>The game's label, in its display language.</summary>
    public string? Name { get; set; }

    public string? Phase { get; set; }
    public List<BridgeInstructionOptionDto> Options { get; set; } = [];
}
