namespace FootballManager.Domain.Tactics;

/// <summary>
/// A score as the game rates it: a range while its knowledge is incomplete, a single value otherwise. Role ratings are
/// 0-20 (the game's stars, 20 being five), tactical effectiveness scores 0-100.
/// </summary>
public sealed record Rating(int Min, int Max)
{
    /// <summary>"17", or "15-17" for a range.</summary>
    public string Display => Min == Max ? $"{Max}" : $"{Min}-{Max}";
}
