namespace FootballManager.Api.Responses.Messages;

public class InboxMessageResponse
{
    public int Id { get; set; }
    public string? Date { get; set; }
    public string? Time { get; set; }
    public string? Sender { get; set; }
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Body { get; set; }
    public bool IsRead { get; set; }

    /// <summary>Only sent when true.</summary>
    public bool? IsImportant { get; set; }
    public bool? IsPriority { get; set; }
    public bool? CanReply { get; set; }
    public bool? HasReplied { get; set; }
    public bool? IsForwarded { get; set; }

    /// <summary>For a forwarded article: where it was published, and its text when asked for.</summary>
    public string? Source { get; set; }
    public string? Article { get; set; }
}
