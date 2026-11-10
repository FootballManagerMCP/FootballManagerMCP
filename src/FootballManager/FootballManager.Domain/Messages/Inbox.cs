namespace FootballManager.Domain.Messages;

/// <summary>A page of the user's inbox, in the game's order (newest first).</summary>
public class Inbox
{
    /// <summary>How many messages the list holds (the whole inbox, or the unread ones).</summary>
    public int Total { get; set; }

    public int? Unread { get; set; }

    public List<InboxMessage> Messages { get; set; } = [];
}
