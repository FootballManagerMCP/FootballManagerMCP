namespace FootballManager.Api.Responses.Messages;

public class InboxResponse
{
    /// <summary>How many messages the list holds: the whole inbox, or the unread ones.</summary>
    public int Total { get; set; }

    public int? Unread { get; set; }

    /// <summary>The messages listed here.</summary>
    public int Count { get; set; }

    public List<InboxMessageResponse> Messages { get; set; } = [];
}
