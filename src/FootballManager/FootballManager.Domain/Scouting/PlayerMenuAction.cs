namespace FootballManager.Domain.Scouting;

/// <summary>The entries of a player's menu that scouting uses (the game's own menu actions).</summary>
public enum PlayerMenuAction
{
    /// <summary>Have a scout, or any scout, watch the player for a while.</summary>
    AssignScout,
    AddToShortlist,
    RemoveFromShortlist
}
