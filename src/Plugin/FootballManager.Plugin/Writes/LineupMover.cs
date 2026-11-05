using FM.GamePlugin;
using FootballManager.Plugin.Data;
using FootballManager.Plugin.Inspection;
using SI.Core;

namespace FootballManager.Plugin.Writes;

// Moves a lineup position of the user's tactic, or swaps two, the way dragging on the tactics pitch does: the native
// EventFmxSwapPlayers on the tactic, with a record of the dragged position and the spot it's dropped on. Dropped on
// another lineup position, the two players change places (each position keeps its spot); dropped on an empty spot,
// the dragged position moves there with its player.
//
// The record, as the game's own drags send it (plain Records, fields in this order):
//   {DragData: {DragData: mask, DragDropExtraData1: inPossession, DragDropExtraData3: 1u, DragDropExtraData2: lineup index},
//    DropData: {DropData: mask, + the same three extras when a lineup position is there}}
// DragDropExtraData3 came with game version 27.0.0.2493426 and was 1 in every drag recorded on slot 2. Its meaning
// is unknown; it may be the tactic slot index.
internal static class LineupMover
{
    private const string TacticPath = "Team.Tactic";
    private const string PositionPath = "PositionValue";
    private const int LineupSize = 11;

    private const string DragDataField = "DragData";
    private const string DropDataField = "DropData";
    private const string InPossessionField = "DragDropExtraData1";
    private const string IndexField = "DragDropExtraData2";
    private const string ExtraField = "DragDropExtraData3";
    private const uint ExtraValue = 1;

    private static readonly InteropEventIDs Event = InteropEventIDs.EventFmxSwapPlayers;

    /// <summary>Moves lineup position <paramref name="index"/> to the spot <paramref name="to"/> (a position mask), or
    /// swaps it with lineup position <paramref name="with"/>. Only describes what it would send unless told to send.</summary>
    public static async Task<string> MoveAsync(int index, uint? to, int? with, bool inPossession, bool send, TimeSpan timeout)
    {
        if (index is < 0 or >= LineupSize || with is < 0 or >= LineupSize)
        {
            throw new ArgumentException($"Lineup positions are numbered 0 to {LineupSize - 1}.");
        }

        if ((to is null) == (with is null))
        {
            throw new ArgumentException("Say where it goes: to=<position mask> (an empty spot or another position's), or with=<lineup index> to swap.");
        }

        if (with == index)
        {
            throw new ArgumentException("A position can't swap with itself.");
        }

        var tacticQuery = new DataQuery(DataQueries.HumanRoot, [], BasePath: TacticPath, Contexts: [new ContextLiteral("Boolean", true)]);
        var (tactic, contexts) = await DataQueries.ResolveSourceAsync(tacticQuery, timeout);
        var masks = await ReadMasksAsync(tactic, contexts, inPossession, timeout);

        var dragged = masks[index];
        var target = to ?? masks[with!.Value].Mask;
        var targetIndex = with ?? masks.FindIndex(m => m.Mask == target);

        if (targetIndex == index)
        {
            throw new ArgumentException($"Lineup position {index} is already at {target}.");
        }

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var drag = new Record();
            drag.Add(Id(DragDataField), Number(dragged.Mask, dragged.Kind));
            AddLineupPosition(drag, index, inPossession);

            var drop = new Record();
            drop.Add(Id(DropDataField), Number(target, dragged.Kind));

            if (targetIndex >= 0)
            {
                AddLineupPosition(drop, targetIndex, inPossession);
            }

            var record = new Record();
            record.Add(Id(DragDataField), TypedValue.Create<Record>(drag));
            record.Add(Id(DropDataField), TypedValue.Create<Record>(drop));

            var data = TypedValue.Create<Record>(record);
            var tacticRecord = TypedValueReader.AsReference(tactic) ?? throw new InvalidOperationException("The tactic isn't a game record.");

            if (send)
            {
                Subsystems.Get<GameInteropSubsystem>().SendEvent((uint)Event, tacticRecord, data);
            }

            return Json.Serialize(new Dictionary<string, object?>
            {
                ["event"] = Event.ToString(),
                ["sent"] = send,
                ["action"] = targetIndex >= 0 ? "swap" : "move",
                ["index"] = index,
                ["with"] = targetIndex >= 0 ? targetIndex : null,
                ["inPossession"] = inPossession,
                ["from"] = dragged.Mask,
                ["to"] = target,
                ["lineup"] = masks.Select(m => m.Mask).ToList(),
                ["data"] = TypedValueReader.Read(data),
                ["shape"] = TypedValueShape.Describe(data)
            });
        });
    }

    // Each lineup position's mask, with the kind of number the game holds it as (the record is built the same way).
    private static async Task<List<(long Mask, DynamicNumber.NumberKind Kind)>> ReadMasksAsync(
        TypedValue tactic, IReadOnlyList<RequestContext> contexts, bool inPossession, TimeSpan timeout)
    {
        var captures = new Dictionary<string, PropertyCapture> { [PositionPath] = PropertyCapture.Value };

        var masks = await MainThreadDispatcher.RunAcrossFramesAsync(
            () => PropertyRequest.StartAll(
                Enumerable.Range(0, LineupSize).Select(i => (tactic, (IReadOnlyList<RequestContext>)
                [
                    .. contexts,
                    new ContextLiteral("InPossession", inPossession).ToRequestContext(),
                    new ContextLiteral("TeamSelectionIndex", i).ToRequestContext()
                ])),
                [PositionPath],
                captures),
            requests => requests.All(r => r.IsComplete),
            requests => requests.Select(r =>
            {
                var binding = r.FinishWithCaptures()[0];
                var number = binding.CapturedValue?.TryCast<NumericTypedValue>();

                return number is null || binding.Value is null
                    ? ((long Mask, DynamicNumber.NumberKind Kind)?)null
                    : (Convert.ToInt64(binding.Value), TypedValueReader.NumberKindOf(number));
            }).ToList(),
            timeout);

        var missing = Enumerable.Range(0, LineupSize).Where(i => masks[i] is null).ToList();

        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"The game didn't give the position of lineup slot(s) {string.Join(", ", missing)}.");
        }

        return masks.Select(m => m!.Value).ToList();
    }

    private static void AddLineupPosition(Record record, int index, bool inPossession)
    {
        record.Add(Id(InPossessionField), TypedValue.Create(inPossession));
        record.Add(Id(ExtraField), TypedValue.Create(ExtraValue));
        record.Add(Id(IndexField), TypedValue.Create(index));
    }

    private static TypedValue Number(long value, DynamicNumber.NumberKind kind)
    {
        return kind == DynamicNumber.NumberKind.UnsignedWhole ? TypedValue.Create((uint)value) : TypedValue.Create((int)value);
    }

    private static uint Id(string property)
    {
        return IdentifierCatalog.TryIdOf(property) ?? throw new InvalidOperationException($"The game has no property named {property}.");
    }
}
