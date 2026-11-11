namespace FootballManager.Domain.Scouting;

/// <summary>A rule the player search applies, with the game's labels (in its display language).</summary>
public class SearchRule
{
    public SearchRuleKind Kind { get; set; }

    /// <summary>The rule's name in the game's language, e.g. "Age between (years)".</summary>
    public string? Name { get; set; }

    /// <summary>How the value is compared, e.g. "Entre", "Au moins", "Naturel".</summary>
    public string? Comparison { get; set; }

    /// <summary>The value, e.g. "20-24", "Defender (Left)", "B".</summary>
    public string? Value { get; set; }
}
