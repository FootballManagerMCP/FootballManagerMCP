using System.ComponentModel;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class InboxTools
{
    [McpServerTool(Name = "get_inbox")]
    [Description("Get the user's inbox from their running Football Manager 27 save, read live from the game, newest " +
                 "first: staff reports (injuries, fitness tests, training, scouting, opposition reports), the board, " +
                 "players asking to talk, press conferences, and press articles the staff forward. For each message: " +
                 "its id, date and time, sender, title, subtitle (what it's about), body, whether it's been read, and " +
                 "flags the game sets: isImportant, isPriority, canReply (the game offers answers to pick in the " +
                 "message), hasReplied, isForwarded (a press article; source is where it was published). Also the " +
                 "inbox's total and how many are unread. Text is in the game's display language. This only reads: " +
                 "it doesn't mark anything as read. Fails with a 503 when the game isn't running or no save is loaded.")]
    public static async Task<string> GetInbox(
        IHttpClientFactory httpFactory,
        [Description("Only the unread messages.")]
        bool unreadOnly = false,
        [Description("At most this many messages (1-100), 20 by default. A message is about 300 bytes, more with articles.")]
        int? limit = null,
        [Description("Skip this many of the newest, to page back through older messages.")]
        int skip = 0,
        [Description("Include the text of the press articles staff forward (about 1 KB each).")]
        bool includeArticles = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var query = $"unreadOnly={unreadOnly.ToString().ToLowerInvariant()}" +
                    (limit is { } max ? $"&limit={max}" : string.Empty) +
                    $"&skip={skip}" +
                    $"&includeArticles={includeArticles.ToString().ToLowerInvariant()}";

        var response = await http.GetAsync($"/api/v1/inbox?{query}");
        return await response.ReadContentOrError();
    }
}
