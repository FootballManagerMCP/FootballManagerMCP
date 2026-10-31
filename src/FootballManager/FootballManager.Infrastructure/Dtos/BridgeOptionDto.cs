namespace FootballManager.Infrastructure.Dtos;

/// <summary>
/// An entry of a list the game builds for a screen (a dropdown option, a key attribute): a value and its label. Entries
/// shown after a separator come wrapped in RowData.
/// </summary>
internal sealed class BridgeOptionDto
{
    public long? PropertyValue { get; set; }
    public string? DisplayValue { get; set; }
    public BridgeOptionDto? RowData { get; set; }
}
