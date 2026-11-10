using FootballManager.Domain.Scouting;

namespace FootballManager.Api.Responses.Scouting;

public class PlayerScoutingChangeResponse
{
    public bool Applied { get; set; }
    public int PlayerId { get; set; }
    public required string PlayerName { get; set; }
    public PlayerMenuAction Action { get; set; }

    /// <summary>The game's menu entry, level by level, in its display language.</summary>
    public required string Choice { get; set; }

    public bool? IsShortlisted { get; set; }
    public bool? IsBeingScouted { get; set; }
}
