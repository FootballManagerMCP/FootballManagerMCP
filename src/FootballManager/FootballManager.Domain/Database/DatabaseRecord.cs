namespace FootballManager.Domain.Database;

/// <summary>A result of the game's search: a record of its database, with what the search shows of it.</summary>
public class DatabaseRecord
{
    public DatabaseRecordKind Kind { get; set; }

    /// <summary>The record's id in the game. A club's is its first team's too.</summary>
    public int? Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>A club's full name, when it differs from the name.</summary>
    public string? LongName { get; set; }

    /// <summary>The game's line under the name: a player's position and club, a staff member's job, a club's standing.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// What the game calls it, for a national team (men's or women's) and for <see cref="DatabaseRecordKind.Other"/>
    /// (an award, a media outlet...).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>A person's age.</summary>
    public int? Age { get; set; }

    /// <summary>A player's positions, as the game writes them.</summary>
    public string? Positions { get; set; }

    /// <summary>A staff member's job.</summary>
    public string? Job { get; set; }

    /// <summary>A person's nationality; a club's, competition's or national team's nation.</summary>
    public string? Nation { get; set; }

    /// <summary>A person's club.</summary>
    public string? Club { get; set; }

    public int? ClubId { get; set; }

    /// <summary>A person's club's nation.</summary>
    public string? ClubNation { get; set; }

    /// <summary>
    /// A club's league, or a person's club's. A person's is the league's short name, as the game shortens it, which
    /// only the club's nation tells apart.
    /// </summary>
    public string? League { get; set; }

    /// <summary>A nation's or national team's continent.</summary>
    public string? Continent { get; set; }
}
