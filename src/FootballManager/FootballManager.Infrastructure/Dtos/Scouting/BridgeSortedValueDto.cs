namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>A value as a table column holds it: the number it sorts by, and the text it shows.</summary>
internal sealed class BridgeSortedValueDto
{
    public double? SortValue { get; set; }
    public string? DisplayValue { get; set; }
}
