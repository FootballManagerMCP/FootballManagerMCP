using FootballManager.Domain.Attributes;

namespace FootballManager.Domain.Entities;

public class PlayerAttribute
{
    public required string Name { get; set; }
    public AttributeCategory Category { get; set; }

    /// <summary>
    /// FM shows an attribute as a range until the player is fully scouted (always exact for your own players).
    /// Both are null when the game masks the value, as it does for hidden attributes without the In-Game Editor.
    /// </summary>
    public int? Min { get; set; }
    public int? Max { get; set; }

    public bool IsKnown => Min is not null && Max is not null;

    /// <summary>As FM writes it: "12", or "11-14" while scouting is incomplete.</summary>
    public string? Display => !IsKnown ? null : Min == Max ? $"{Min}" : $"{Min}-{Max}";
}
