namespace FootballManager.Domain.Scouting;

/// <summary>The entries of a player's menu that recruitment uses (the game's own menu actions).</summary>
public enum PlayerMenuAction
{
    /// <summary>Have a scout, or any scout, watch the player for a while.</summary>
    AssignScout,
    AddToShortlist,
    RemoveFromShortlist,

    /// <summary>Offer the player a trial at the club, for one to four weeks.</summary>
    OfferTrial,

    /// <summary>Withdraw an offer made to the player that he hasn't answered yet, a trial offer among them.</summary>
    WithdrawOffer,

    /// <summary>End the trial of a player on trial at the club: he leaves it.</summary>
    EndTrial
}
