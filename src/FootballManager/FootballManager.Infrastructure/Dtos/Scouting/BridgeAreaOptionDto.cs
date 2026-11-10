namespace FootballManager.Infrastructure.Dtos.Scouting;

/// <summary>
/// A row of the scouting area dropdown: an area ({DisplayValue, PropertyValue}) or a heading over more rows
/// ({RowData, ChildList}).
/// </summary>
internal sealed class BridgeAreaOptionDto
{
    public string? DisplayValue { get; set; }
    public BridgeAreaIdDto? PropertyValue { get; set; }
    public BridgeAreaOptionDto? RowData { get; set; }
    public List<BridgeAreaOptionDto>? ChildList { get; set; }
}
