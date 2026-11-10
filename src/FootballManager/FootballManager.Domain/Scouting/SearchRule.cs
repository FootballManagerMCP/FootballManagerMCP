namespace FootballManager.Domain.Scouting;

/// <summary>A rule the player search applies, with the game's labels (in its display language).</summary>
public class SearchRule
{
    public SearchRuleKind Kind { get; set; }

    /// <summary>The rule's name, e.g. "Âge entre (années)".</summary>
    public string? Name { get; set; }

    /// <summary>How the value is compared, e.g. "Entre", "Au moins", "Naturel".</summary>
    public string? Comparison { get; set; }

    /// <summary>The value, e.g. "20-24", "Défenseur (Gauche)", "B".</summary>
    public string? Value { get; set; }
}
