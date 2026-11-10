namespace FootballManager.Domain.Database;

/// <summary>The game's search for a text, from its closest match down.</summary>
public class DatabaseSearch
{
    public string Query { get; set; } = string.Empty;

    /// <summary>How many results the game found. The search is fuzzy: past the close matches come loose ones.</summary>
    public int Total { get; set; }

    /// <summary>How many of them were looked through, from the top, for the results given.</summary>
    public int Read { get; set; }

    public List<DatabaseRecord> Results { get; set; } = [];
}
