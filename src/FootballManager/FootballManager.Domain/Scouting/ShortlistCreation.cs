namespace FootballManager.Domain.Scouting;

/// <summary>A new shortlist, as planned or made, and the user's shortlists afterwards.</summary>
public class ShortlistCreation
{
    public bool Applied { get; set; }

    public required string Name { get; set; }

    /// <summary>True once the game lists the new shortlist; null on a dry run.</summary>
    public bool? Created { get; set; }

    public List<Shortlist> Shortlists { get; set; } = [];
}
