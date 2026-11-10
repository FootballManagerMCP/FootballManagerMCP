namespace FootballManager.Domain.Messages;

/// <summary>A message in the user's inbox, as the game writes it (in its display language).</summary>
public class InboxMessage
{
    public int Id { get; set; }

    /// <summary>"2026-11-21".</summary>
    public string? Date { get; set; }

    /// <summary>"16:43".</summary>
    public string? Time { get; set; }

    public string? Sender { get; set; }
    public string Title { get; set; } = string.Empty;

    /// <summary>What it's about, e.g. "Celtic".</summary>
    public string? Subtitle { get; set; }

    public string? Body { get; set; }

    public bool IsRead { get; set; }
    public bool IsImportant { get; set; }
    public bool IsPriority { get; set; }

    /// <summary>The game offers replies to it (the answers it lists in the message).</summary>
    public bool CanReply { get; set; }

    public bool HasReplied { get; set; }

    /// <summary>A press article passed on by the club's staff.</summary>
    public bool IsForwarded { get; set; }

    /// <summary>The paper or site a forwarded article comes from.</summary>
    public string? Source { get; set; }

    /// <summary>A forwarded article's text, when asked for.</summary>
    public string? Article { get; set; }
}
