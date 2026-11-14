namespace FootballManager.Infrastructure.Dtos.Training;

/// <summary>A day of a training calendar week.</summary>
internal sealed class BridgeTrainingDayDto
{
    public string? Date { get; set; }
    public List<BridgeCalendarEventDto>? Events { get; set; }
}
