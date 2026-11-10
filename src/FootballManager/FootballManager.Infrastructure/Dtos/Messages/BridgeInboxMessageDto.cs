using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Messages;

/// <summary>A message of the user's inbox, as the portal lists it.</summary>
internal sealed class BridgeInboxMessageDto
{
    [JsonPropertyName(GameConstants.UniqueId)]
    public int? Id { get; set; }

    [JsonPropertyName(InboxConstants.Date)]
    public string? Date { get; set; }

    [JsonPropertyName(InboxConstants.Time)]
    public string? Time { get; set; }

    [JsonPropertyName(InboxConstants.Sender)]
    public string? Sender { get; set; }

    [JsonPropertyName(InboxConstants.Title)]
    public string? Title { get; set; }

    [JsonPropertyName(InboxConstants.Subtitle)]
    public string? Subtitle { get; set; }

    [JsonPropertyName(InboxConstants.Body)]
    public string? Body { get; set; }

    [JsonPropertyName(InboxConstants.IsRead)]
    public bool? IsRead { get; set; }

    [JsonPropertyName(InboxConstants.IsImportant)]
    public bool? IsImportant { get; set; }

    [JsonPropertyName(InboxConstants.IsPriority)]
    public bool? IsPriority { get; set; }

    [JsonPropertyName(InboxConstants.CanReply)]
    public bool? CanReply { get; set; }

    [JsonPropertyName(InboxConstants.HasReplied)]
    public bool? HasReplied { get; set; }

    [JsonPropertyName(InboxConstants.IsForwarded)]
    public bool? IsForwarded { get; set; }

    [JsonPropertyName(InboxConstants.Source)]
    public string? Source { get; set; }

    [JsonPropertyName(InboxConstants.Article)]
    public string? Article { get; set; }
}
