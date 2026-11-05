using FM.GamePlugin;
using FootballManager.Plugin.Data;
using FootballManager.Plugin.Inspection;
using SI.Bindable;
using SI.Core;

namespace FootballManager.Plugin.Writes;

// Puts a player in a lineup position of the user's tactic, the way the position's player dropdown does ("PickPlayer"):
// the native EventFmxTeamSelectionPickPlayer on the tactic, with the dropdown row of the player picked. A player from
// the bench swaps places with the one in the position, as in the game.
//
// The row, as the game's own picks send it (DynamicReferences, every number signed):
//   {PropertyValue: 2 (TacticTeamSelectionDropdownOption.PickPlayer),
//    DisplayValue: {player, PlayerTeamSelectionIndex: where the player is now (-1 outside the matchday squad),
//                   Index: the player's database index, StarRange: {MaxValue, MinValue, IsScoutsOpinion},
//                   TeamSelectionIndex: the position filled}}
// StarRange is the dropdown's star rating, sent as 0 on the assumption that the game only displays it.
internal static class LineupPicker
{
    private const string TacticPath = "Team.Tactic";
    private const string TeamPath = "Team";
    private const string SquadPath = "TeamSelectionIndexDropdown";
    private const string PlayersPath = "Players";
    private const int LineupSize = 11;
    private const int PickPlayerOption = 2;
    private const int OutsideSquad = -1;

    private static readonly InteropEventIDs Event = InteropEventIDs.EventFmxTeamSelectionPickPlayer;

    /// <summary>Puts the player with database index <paramref name="player"/> in lineup position
    /// <paramref name="index"/>. Only describes what it would send unless told to send.</summary>
    public static async Task<string> PickAsync(int index, int player, bool send, TimeSpan timeout)
    {
        if (index is < 0 or >= LineupSize)
        {
            throw new ArgumentException($"Lineup positions are numbered 0 to {LineupSize - 1}.");
        }

        var tacticQuery = new DataQuery(DataQueries.HumanRoot, [], BasePath: TacticPath, Contexts: [new ContextLiteral("Boolean", true)]);
        var (tactic, tacticContexts) = await DataQueries.ResolveSourceAsync(tacticQuery, timeout);
        var (team, teamContexts) = await DataQueries.ResolveSourceAsync(new DataQuery(DataQueries.HumanRoot, [], BasePath: TeamPath), timeout);

        // The matchday squad by team selection index (0-10 the lineup, then the bench), and the whole squad for a
        // player outside it.
        var squad = await ReadAsync(
            tactic,
            [
                .. tacticContexts,
                new ContextLiteral("InPossession", true).ToRequestContext(),
                new ContextLiteral("TeamSelectionIndex", index).ToRequestContext()
            ],
            SquadPath, PropertyCapture.Tree, timeout);
        var players = await ReadAsync(team, teamContexts, PlayersPath, PropertyCapture.Items, timeout);

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var (person, from) = Find(player, squad.CapturedTree ?? [], players.CapturedItems ?? []);

            if (from == index)
            {
                throw new ArgumentException($"Player {player} is already in lineup position {index}.");
            }

            var stars = new DynamicReference();
            stars.Add(Id("MaxValue"), TypedValue.Create(0));
            stars.Add(Id("MinValue"), TypedValue.Create(0));
            stars.Add(Id("IsScoutsOpinion"), TypedValue.Create(false));

            var display = new DynamicReference();
            display.Add(Id("player"), TypedValue.CreateCopyFrom(person));
            display.Add(Id("PlayerTeamSelectionIndex"), TypedValue.Create(from));
            display.Add(Id("Index"), TypedValue.Create(player));
            display.Add(Id("StarRange"), TypedValue.Create<DynamicReference>(stars));
            display.Add(Id("TeamSelectionIndex"), TypedValue.Create(index));

            var row = new DynamicReference();
            row.Add(Id("PropertyValue"), TypedValue.Create(PickPlayerOption));
            row.Add(Id("DisplayValue"), TypedValue.Create<DynamicReference>(display));

            var data = TypedValue.Create<DynamicReference>(row);
            var tacticRecord = TypedValueReader.AsReference(tactic) ?? throw new InvalidOperationException("The tactic isn't a game record.");

            if (send)
            {
                Subsystems.Get<GameInteropSubsystem>().SendEvent((uint)Event, tacticRecord, data);
            }

            return Json.Serialize(new Dictionary<string, object?>
            {
                ["event"] = Event.ToString(),
                ["sent"] = send,
                ["index"] = index,
                ["player"] = player,
                ["from"] = from == OutsideSquad ? null : from,
                ["data"] = TypedValueReader.Read(data),
                ["shape"] = TypedValueShape.Describe(data)
            });
        });
    }

    private static async Task<PropertyBinding> ReadAsync(
        TypedValue source, IReadOnlyList<RequestContext> contexts, string path, PropertyCapture capture, TimeSpan timeout)
    {
        var bindings = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.Start(source, [path], contexts, new Dictionary<string, PropertyCapture> { [path] = capture }),
            request => request.IsComplete,
            request => request.FinishWithCaptures(),
            timeout);

        var binding = bindings[0];
        return binding.Error is null ? binding : throw new InvalidOperationException($"{path}: {binding.Error}");
    }

    // The player's record and team selection index: from the matchday squad's row when they're in it (the record the
    // dropdown itself holds), else from the whole squad, outside the matchday squad.
    private static (TypedValue Person, int From) Find(
        int player, List<(TypedValue Row, object? Decoded)> squad, List<TypedValue> players)
    {
        foreach (var (row, _) in squad)
        {
            var rowData = TypedValueReader.Field(row, "RowData");
            var person = TypedValueReader.Field(rowData, "player");

            if (TypedValueReader.DatabaseIndexOf(person) == player
                && TypedValueReader.Read(TypedValueReader.Field(rowData, "Index")) is { } from)
            {
                return (person!, Convert.ToInt32(from));
            }
        }

        var outside = players.FirstOrDefault(p => TypedValueReader.DatabaseIndexOf(p) == player)
            ?? throw new ArgumentException($"No player of the squad has database index {player} (get_squad gives each player's).");

        return (outside, OutsideSquad);
    }

    private static uint Id(string property)
    {
        return IdentifierCatalog.TryIdOf(property) ?? throw new InvalidOperationException($"The game has no property named {property}.");
    }
}
