namespace FootballManager.Api.Responses.Database;

public class DatabaseSearchResponse
{
    public required string Query { get; set; }

    /// <summary>How many results the game found, close and loose matches together.</summary>
    public int Total { get; set; }

    /// <summary>How many of them, from the top, were looked through for the results given.</summary>
    public int Read { get; set; }

    /// <summary>The results given here.</summary>
    public int Count { get; set; }

    public List<DatabaseRecordResponse> Results { get; set; } = [];
}
