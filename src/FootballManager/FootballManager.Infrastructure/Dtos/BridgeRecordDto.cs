namespace FootballManager.Infrastructure.Dtos;

/// <summary>
/// One record's answer from the plugin: the requested property paths with their values, the paths the game didn't
/// answer in time, and the ones it rejected.
/// </summary>
internal sealed class BridgeRecordDto<TValues> where TValues : new()
{
    public TValues Values { get; set; } = new();
    public List<string>? Pending { get; set; }
    public Dictionary<string, string>? Errors { get; set; }
}
