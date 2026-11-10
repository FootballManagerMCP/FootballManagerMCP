namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>An area's record: its kind in the top byte, its place in the game's database below.</summary>
internal sealed class BridgeAreaIdDto
{
    public long? DatabaseReferenceField { get; set; }
}
