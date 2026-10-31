namespace FootballManager.Domain.Attributes;

/// <summary>A player attribute by its FM name, e.g. "Finishing" or "Professionalism".</summary>
public sealed record AttributeDefinition(string Name, AttributeCategory Category)
{
    public bool IsHidden => Category == AttributeCategory.Hidden;
}
