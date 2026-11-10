namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>The dialog the game shows before running a menu entry, as its screens ask the user to confirm.</summary>
internal sealed class BridgeConfirmationDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }

    /// <summary>The game shows it as an error: a warning against going ahead.</summary>
    public bool? IsErrorDialog { get; set; }
}
