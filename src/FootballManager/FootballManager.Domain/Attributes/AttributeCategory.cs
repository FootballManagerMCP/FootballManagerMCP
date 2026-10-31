namespace FootballManager.Domain.Attributes;

public enum AttributeCategory
{
    Technical,
    Mental,
    Physical,
    Goalkeeping,

    /// <summary>Attributes FM never shows the player (personality traits); see <see cref="PlayerAttributes.Hidden"/>.</summary>
    Hidden
}
