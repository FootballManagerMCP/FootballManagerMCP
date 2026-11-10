namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>
/// The payload a menu entry sends. Only its confirmation dialog is read here: the payload itself is sent from the game's
/// own copy when the entry is picked.
/// </summary>
internal sealed class BridgeActionDataDto
{
    public BridgeConfirmationDto? ConfirmationDialogData { get; set; }
}
