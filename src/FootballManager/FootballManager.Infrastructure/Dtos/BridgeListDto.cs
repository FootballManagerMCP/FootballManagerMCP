namespace FootballManager.Infrastructure.Dtos;

/// <summary>The plugin's answer when a request runs once per list item or context value: one record per run.</summary>
internal sealed class BridgeListDto<TValues> where TValues : new()
{
    public int Count { get; set; }
    public List<BridgeRecordDto<TValues>> Items { get; set; } = [];
}
