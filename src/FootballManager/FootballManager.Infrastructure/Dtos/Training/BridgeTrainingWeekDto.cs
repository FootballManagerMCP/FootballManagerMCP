namespace FootballManager.Infrastructure.Dtos.Training;

/// <summary>A week of the training calendar, by day name as the game keys it.</summary>
internal sealed class BridgeTrainingWeekDto
{
    public string? WeekStartDate { get; set; }
    public BridgeTrainingDayDto? Monday { get; set; }
    public BridgeTrainingDayDto? Tuesday { get; set; }
    public BridgeTrainingDayDto? Wednesday { get; set; }
    public BridgeTrainingDayDto? Thursday { get; set; }
    public BridgeTrainingDayDto? Friday { get; set; }
    public BridgeTrainingDayDto? Saturday { get; set; }
    public BridgeTrainingDayDto? Sunday { get; set; }

    /// <summary>The days in order, each with the name the game keys it by.</summary>
    public IEnumerable<(string Name, BridgeTrainingDayDto? Day)> Days()
    {
        yield return (nameof(Monday), Monday);
        yield return (nameof(Tuesday), Tuesday);
        yield return (nameof(Wednesday), Wednesday);
        yield return (nameof(Thursday), Thursday);
        yield return (nameof(Friday), Friday);
        yield return (nameof(Saturday), Saturday);
        yield return (nameof(Sunday), Sunday);
    }
}
