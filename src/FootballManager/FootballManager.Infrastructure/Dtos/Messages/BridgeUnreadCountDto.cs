using System.Text.Json.Serialization;
using FootballManager.Infrastructure.Constants;

namespace FootballManager.Infrastructure.Dtos.Messages;

internal sealed class BridgeUnreadCountDto
{
    [JsonPropertyName(InboxConstants.UnreadCount)]
    public int? Unread { get; set; }
}
