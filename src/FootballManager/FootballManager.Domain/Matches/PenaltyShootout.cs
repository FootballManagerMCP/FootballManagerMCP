namespace FootballManager.Domain.Matches;

/// <summary>A match decided on penalties after a draw: each side's penalties scored, and every kick in order.</summary>
public class PenaltyShootout
{
    public int Scored { get; set; }
    public int Conceded { get; set; }
    public List<PenaltyKick> Kicks { get; set; } = [];
}
