namespace FootballManager.Infrastructure.Constants;

// The user's inbox, on the user's manager with the user's manager and team as contexts. The whole inbox is the
// portal's message list, which takes the tab shown and the text it's searched for (none).
internal static class InboxConstants
{
    public const string Messages = "HumanPortalFilteredMessages";
    public const string UnreadMessages = "HumanPortalUnreadMessages";
    public const string UnreadCount = "NumberUnreadMessages";

    public const string Tab = "PortalMessagesTab";
    public const int AllMessagesTab = 0;
    public const string SearchText = "StringType";

    // On a message.
    public const string Date = "Date";
    public const string Time = "NewsItemTimeSent";
    public const string Sender = "NewsPersonSender.Name";
    public const string Title = "NewsTitleString";
    public const string Subtitle = "HeadlineSubTitle";
    public const string Body = "NewsBodyString";
    public const string IsRead = "NewsItemHasBeenRead";
    public const string IsImportant = "NewsItemIsImportant";
    public const string IsPriority = "NewsItemIsPriority";
    public const string CanReply = "CanRespond";
    public const string HasReplied = "NewsHasResponded";
    public const string IsForwarded = "IsForwarded";
    public const string Source = "NewsMediaSource.Name";

    // A forwarded message attaches the article; any other attaches itself.
    public const string Article = "AttachmentsNewsItem.NewsBodyString";

    public static readonly string[] MessageProperties =
    [
        GameConstants.UniqueId, Date, Time, Sender, Title, Subtitle, Body, IsRead, IsImportant, IsPriority, CanReply,
        HasReplied, IsForwarded, Source
    ];
}
