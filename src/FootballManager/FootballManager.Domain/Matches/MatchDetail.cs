namespace FootballManager.Domain.Matches;

/// <summary>One match in depth, from the user's side. Only the summary is known before it's played.</summary>
public class MatchDetail
{
    public required MatchSummary Summary { get; set; }
    public TeamMatchStats? TeamStats { get; set; }
    public TeamMatchStats? OpponentStats { get; set; }

    /// <summary>Both sides' goals in order, own goals and penalties included.</summary>
    public List<MatchGoal> Goals { get; set; } = [];

    public List<MatchRedCard> RedCards { get; set; } = [];

    /// <summary>Only for a match decided on penalties.</summary>
    public PenaltyShootout? PenaltyShootout { get; set; }

    /// <summary>The user's players who took part, starters first, with their positions, roles and numbers.</summary>
    public List<MatchLineupPlayer> Lineup { get; set; } = [];

    /// <summary>The user's substitutions, in the order they were made.</summary>
    public List<MatchSubstitution> Substitutions { get; set; } = [];

    /// <summary>
    /// The game moved some of the user's starters to other lineup slots during the match: the shape was changed or
    /// players swapped places. The positions given are those they started in.
    /// </summary>
    public bool LineupRearranged { get; set; }

    public PlayerOfTheMatch? PlayerOfTheMatch { get; set; }

    /// <summary>The game's post-match analysis in its display language; it keeps it for the last five matches only.</summary>
    public List<string> Analysis { get; set; } = [];
}
