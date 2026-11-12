using System.ComponentModel;
using System.Net.Http.Json;
using ModelContextProtocol.Server;

namespace McpServer.Tools;

[McpServerToolType]
public static class TrainingTools
{
    [McpServerTool(Name = "get_training")]
    [Description("Get the user's team's training from their running Football Manager 27 save, read live from the " +
                 "game. schedule: the schedule this week follows. weeks: this week and the next, Monday to Sunday, " +
                 "each day's sessions (slot 0-2, the session's id and name, its category, canEdit when the user can change " +
                 "it: not past days, nor days the team can't train, travels or plays) and match (the venue, on a match " +
                 "day). sessionTypes: the sessions each category offers (tactical, match preparation, football " +
                 "development, physical and recovery, other), with the ids set_training takes. focusOptions and " +
                 "intensityOptions: the additional focuses and intensities (including rest periods, labelled with the " +
                 "\"no work\" choice they come under) players can take. players: each player of the training squad " +
                 "with the position he trains in (FM code) and the in-possession role he trains for (key and label; " +
                 "followsMatchRole when they're those he plays in matches, the default), his out-of-possession role " +
                 "when set, his additional focus, his intensity (intensityIsAutomatic when the game picks it), his " +
                 "training rating (0-10) and workload; focusLocked when the staff set his focus, intensityLocked when " +
                 "the medical staff set his intensity (he's injured; his focus can still take rehabilitation). Labels " +
                 "are in the game's language. This only reads.")]
    public static async Task<string> GetTraining(IHttpClientFactory httpFactory)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");
        var response = await http.GetAsync("/api/v1/training");
        return await response.ReadContentOrError();
    }

    [McpServerTool(Name = "set_training")]
    [Description("Change the user's team's training in the running game, as its training screens do: players' " +
                 "additional focus and intensity (or rest), and sessions of this week's or next week's schedule. Ids " +
                 "come from get_training. Each change is checked against what the game offers that player or session " +
                 "(a goalkeeper's focuses aren't an outfield player's; a past day, or a day the team can't train, " +
                 "can't be changed); a change to what's already set is left out with a note. By default a dry run " +
                 "listing each change from what it is to what it would be, in the game's language. Show the user the " +
                 "plan, and only call again with apply=true once they have agreed. Applying sends the changes one by " +
                 "one and reads each back (done: true when the game shows it); the first one the game doesn't take " +
                 "stops the rest, with a note. Training positions and roles, out-of-possession roles, match " +
                 "preparation and sending players on holiday can't be set yet. Refused while a match is being played.")]
    public static async Task<string> SetTraining(
        IHttpClientFactory httpFactory,
        [Description("Players whose own training changes (at most 50).")]
        List<PlayerTrainingArgument>? players = null,
        [Description("Sessions to replace.")]
        List<TrainingSessionArgument>? sessions = null,
        [Description("False (the default) only checks. True makes the changes: only once the user has agreed.")]
        bool apply = false)
    {
        var http = httpFactory.CreateClient("FootballManagerApi");

        var response = await http.PostAsJsonAsync(
            $"/api/v1/training?apply={apply.ToString().ToLowerInvariant()}",
            new { players, sessions });

        return await response.ReadContentOrError();
    }
}
