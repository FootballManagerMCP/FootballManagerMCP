namespace FootballManager.Domain.Tactics.Editing;

/// <summary>A tactic edit: the steps it takes, in order, and what they did or would do.</summary>
public class TacticEdit
{
    public int Slot { get; set; }

    /// <summary>False for a dry run, which only plans.</summary>
    public bool Applied { get; set; }

    public List<TacticEditStep> Steps { get; set; } = [];

    /// <summary>What the reader should know about the edit's effects, e.g. a style resetting the instructions.</summary>
    public List<string> Notes { get; set; } = [];
}
