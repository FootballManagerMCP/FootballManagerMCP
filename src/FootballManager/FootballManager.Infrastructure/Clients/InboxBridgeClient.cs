using FootballManager.Application.Interfaces;
using FootballManager.Domain.Messages;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Messages;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the user's inbox through the plugin, a page at a time, as the portal lists it.
internal sealed class InboxBridgeClient(GameBridge bridge) : IInboxGameClient
{
    public async Task<Inbox> GetInboxAsync(
        bool unreadOnly, int skip, int take, bool includeArticles, CancellationToken cancellationToken = default)
    {
        var list = unreadOnly
            ? BridgeQuery.FromHuman().List(InboxConstants.UnreadMessages)
            : BridgeQuery.FromHuman()
                .List(InboxConstants.Messages)
                .Context(InboxConstants.Tab, InboxConstants.AllMessagesTab)
                .Context(InboxConstants.SearchText, string.Empty);

        var paths = includeArticles ? [.. InboxConstants.MessageProperties, InboxConstants.Article] : InboxConstants.MessageProperties;

        // A message without a sender (a news round-up) or a source (one not forwarded) leaves those out.
        var messages = bridge.GetListAsync<BridgeInboxMessageDto>(
            list.WithHumanAndTeam().Skip(skip).Take(take).Props(paths), cancellationToken, gapsExpected: true);
        var unread = bridge.GetRecordAsync<BridgeUnreadCountDto>(
            BridgeQuery.FromHuman().WithHumanAndTeam().Props([InboxConstants.UnreadCount]), cancellationToken);

        await Task.WhenAll(messages, unread);

        var page = await messages;

        return new Inbox
        {
            Total = page.Total ?? page.Count,
            Unread = (await unread).Values.Unread,
            Messages = page.Items.Select(message => message.Values).ToList().Adapt<List<InboxMessage>>()
        };
    }
}
