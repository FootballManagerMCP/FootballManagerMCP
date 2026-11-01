namespace FootballManager.Domain.Matches;

/// <summary>The result over the match itself; a penalty shoot-out after a draw still counts as a draw.</summary>
public enum MatchOutcome
{
    Win,
    Draw,
    Loss
}
