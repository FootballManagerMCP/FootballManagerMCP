namespace FootballManager.Domain.Database;

/// <summary>What a result of the game's search is, whatever the game's display language.</summary>
public enum DatabaseRecordKind
{
    Player,

    /// <summary>A coach, scout, physio or any other non-player, at a club or without one.</summary>
    Staff,

    Club,

    /// <summary>A nation's team (men's or women's, senior or youth).</summary>
    NationalTeam,

    Nation,
    Competition,

    /// <summary>Anything else the search finds: an award, a media outlet, a language.</summary>
    Other
}
