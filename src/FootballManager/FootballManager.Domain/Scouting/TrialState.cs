namespace FootballManager.Domain.Scouting;

/// <summary>Where a player stands with a trial at the user's club, as his menu in the game shows it.</summary>
public enum TrialState
{
    /// <summary>Neither on trial nor waiting on an offer: the menu offers a trial, or nothing about one.</summary>
    None,

    /// <summary>An offer waits for his answer (or his club's): the menu offers to withdraw it.</summary>
    OfferWaiting,

    /// <summary>On trial at the club: the menu offers to end it.</summary>
    OnTrial,

    /// <summary>
    /// His trial was ended: he leaves the club at the game's next processing. Until then the trial still shows, with its
    /// end greyed out in the menu.
    /// </summary>
    Leaving
}
