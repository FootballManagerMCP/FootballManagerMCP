using System.Text.Json;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Domain.Training;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Training;
using FootballManager.Infrastructure.Mappings;
using Mapster;

namespace FootballManager.Infrastructure.Clients;

// Reads the training calendar and the training squad's own training, and changes them as the game's screens do: a
// player's focus and intensity through his dropdowns' events (the game's own rows as their data), a session through the
// calendar card's edit: begin, pick the session, confirm the new one, commit the week.
internal sealed class TrainingBridgeClient(GameBridge bridge, GameVocabulary vocabulary) : ITrainingGameClient
{
    private const string Selector = "PropertyValue";

    public async Task<TrainingOverview> GetTrainingAsync(CancellationToken cancellationToken = default)
    {
        var calendar = CalendarAsync(cancellationToken);
        var sessions = bridge.GetListAsync<BridgeSessionCategoryDto>(
            OnCalendar()
                .Each(TrainingConstants.CategoryContext, TrainingConstants.FirstCategory, TrainingConstants.LastCategory)
                .Props([TrainingConstants.CategorySessions]),
            cancellationToken);
        var players = bridge.GetListAsync<BridgePlayerTrainingDto>(
            BridgeQuery.FromHuman()
                .List(TrainingConstants.Squad)
                .WithHumanAndTeam()
                .Ref(TrainingConstants.JudgeContext, TrainingConstants.Manager)
                .Props(TrainingConstants.PlayerProperties),
            cancellationToken);
        var keys = vocabulary.KeysAsync(cancellationToken);

        await Task.WhenAll(calendar, sessions, players, keys);

        var squad = (await players).Items.Select(item => item.Values).ToList();
        var categoryLists = (await sessions).Items;

        // The category list and the per-category reads are both in category order.
        var categories = ((await calendar).Categories ?? []).Select((category, index) =>
        {
            var type = category.Adapt<TrainingSessionCategory>();
            type.Sessions = index < categoryLists.Count ? categoryLists[index].Values.Sessions.Adapt<List<TrainingOption>>() ?? [] : [];
            return type;
        }).ToList();

        return new TrainingOverview
        {
            Weeks = ((await calendar).Weeks ?? []).Adapt<List<TrainingWeek>>(),
            SessionTypes = categories,
            Schedule = (await calendar).Schedule,
            FocusOptions = Union(squad.Select(player => TrainingRows.Of(player.FocusChoices))),
            IntensityOptions = Union(squad.Select(player => TrainingRows.Of(player.IntensityChoices, TrainingConstants.Holidays))),
            Players = squad.BuildAdapter()
                .AddParameters(MappingConfig.RoleKeysParameter, (await keys).Roles(inPossession: true))
                .AdaptToType<List<PlayerTraining>>()
        };
    }

    public Task SetFocusAsync(int playerId, long focus, CancellationToken cancellationToken = default)
    {
        // The player's focus record with the new row, field by field in the game's order.
        var data = new Dictionary<string, string>();

        foreach (var (field, source) in TrainingConstants.FocusFields)
        {
            data[field] = $"@{TrainingConstants.FocusRecord}.{source}";
        }

        data[TrainingConstants.FocusRowField] = $"@{TrainingConstants.FocusRows}[{Selector}={focus}].RowData";
        data[TrainingConstants.FocusChoicesField] = $"@{TrainingConstants.FocusRecord}.{TrainingConstants.FocusChoicesField}";

        return bridge.WriteAsync<JsonElement>(
            OnPlayerTraining(playerId).ToWrite("/native",
                ("event", TrainingConstants.FocusEvent),
                ("data", JsonSerializer.Serialize(data)),
                ("template", "none")),
            cancellationToken);
    }

    public Task SetIntensityAsync(int playerId, long intensity, CancellationToken cancellationToken = default)
    {
        return bridge.WriteAsync<JsonElement>(
            OnPlayerTraining(playerId).ToWrite("/native",
                ("event", TrainingConstants.IntensityEvent),
                ("field", $"Data:@{TrainingConstants.IntensityRows}[{Selector}={intensity}].RowData"),
                ("template", "none")),
            cancellationToken);
    }

    public async Task SetSessionAsync(string date, int slot, int current, int session, CancellationToken cancellationToken = default)
    {
        // The card carries the day's own date, taken from the week that holds it.
        var weeks = (await CalendarAsync(cancellationToken)).Weeks ?? [];
        var day = weeks.SelectMany((week, index) => week.Days().Select(d => (Week: index, d.Name, d.Day)))
            .FirstOrDefault(d => d.Day?.Date == date);

        if (day.Day is null)
        {
            throw new GameRejectedException($"The training calendar doesn't show {date} in this week or the next.");
        }

        var card = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            [TrainingConstants.CardDate] = $"@{TrainingConstants.Weeks}[{day.Week}].{day.Name}.Date",
            ["SessionIndex"] = $"u:{slot}",
            ["SessionType"] = current
        });

        await SendAsync(TrainingConstants.BeginSessionEdit, ("data", card), cancellationToken);
        await SendAsync(TrainingConstants.SelectSession, ("data", card), cancellationToken);
        await SendAsync(TrainingConstants.ConfirmSession,
            ("data", JsonSerializer.Serialize(new Dictionary<string, object> { ["SessionType"] = session })), cancellationToken);
        await SendAsync(TrainingConstants.CommitSessionEdit, ("data", "null"), cancellationToken);
    }

    private Task<BridgeTrainingCalendarDto> CalendarAsync(CancellationToken cancellationToken)
    {
        return ValuesAsync<BridgeTrainingCalendarDto>(
            OnCalendar().Props([TrainingConstants.Weeks, TrainingConstants.Categories, TrainingConstants.Schedule]), cancellationToken);
    }

    private Task SendAsync(string nativeEvent, (string Name, string Value) data, CancellationToken cancellationToken)
    {
        return bridge.WriteAsync<JsonElement>(
            OnCalendar().ToWrite("/native", ("event", nativeEvent), data, ("template", "none")),
            cancellationToken);
    }

    private async Task<T> ValuesAsync<T>(BridgeQuery query, CancellationToken cancellationToken) where T : new()
    {
        return (await bridge.GetRecordAsync<T>(query, cancellationToken)).Values;
    }

    private static BridgeQuery OnCalendar()
    {
        return BridgeQuery.FromHuman()
            .Base(TrainingConstants.Calendar)
            .WithHumanAndTeam()
            .Ref(TrainingConstants.DateContext, TrainingConstants.Today);
    }

    private static BridgeQuery OnPlayerTraining(int playerId)
    {
        return BridgeQuery.FromHuman()
            .Base(TrainingConstants.PlayerTrainingOf(playerId))
            .WithHumanAndTeam()
            .Ref(TrainingConstants.JudgeContext, TrainingConstants.Manager);
    }

    // Every choice the squad's dropdowns offer, once each, by id.
    private static List<TrainingOption> Union(IEnumerable<IEnumerable<Dtos.BridgeOptionDto>> lists)
    {
        return lists.SelectMany(rows => rows)
            .GroupBy(row => row.PropertyValue)
            .Select(group => group.First())
            .OrderBy(row => row.PropertyValue)
            .ToList()
            .Adapt<List<TrainingOption>>();
    }
}
