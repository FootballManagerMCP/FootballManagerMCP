namespace FootballManager.Infrastructure.Dtos.Training;

/// <summary>
/// One of a calendar day's events: a training session (with a session name) or a fixture (TrainingConstants.MatchEvent).
/// </summary>
internal sealed class BridgeCalendarEventDto
{
    public int? CalendarEvent { get; set; }
    public int? SessionIndex { get; set; }
    public int? SessionType { get; set; }
    public string? SessionName { get; set; }
    public int? SessionCategory { get; set; }
    public bool? CanEdit { get; set; }
    public string? MatchVenue { get; set; }
}
