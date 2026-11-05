using FM.UI;
using SI.Core;

namespace FootballManager.Plugin.Writes;

// Listens to what the game's UI sends while the user edits a tactic by hand, so its exact contents are known before
// the plugin sends any itself: the tactics events raised on the UI event system here, and the native events and
// binding writes below it through InteropRecorder. Off until started; it only watches, the game handles everything as
// usual. Main thread only.
internal static class EventRecorder
{
    // Recorded to learn their contents; the sender doesn't build these yet.
    public const string SwapType = "swap";
    public const string SelectPlayerType = "selectplayer";
    public const string PositionType = "position";
    public const string PitchType = "pitch";

    // One click raises several things (UI events, the native event they turn into, binding writes). The screens' data
    // traffic comes in bursts of hundreds a frame, so it has its own buffer and never pushes those out.
    private const int Capacity = 1000;
    private const int DataCapacity = 5000;

    private static readonly List<RecordedEvent> Recorded = [];
    private static readonly List<RecordedEvent> DataRecorded = [];

    // Kept referenced while registered: the game holds only the IL2CPP side of the delegates.
    private static readonly List<Il2CppSystem.Object> Callbacks = [];
    private static Il2CppSystem.Object? _owner;

    public static bool IsRecording => _owner is not null;

    public static int DataCount => DataRecorded.Count;

    public static string Start(RecorderOptions options)
    {
        if (_owner is not null)
        {
            return $"Already recording. {InteropRecorder.Configure(options)}";
        }

        var events = Subsystems.Get<EventSubsystem>();
        var owner = new Il2CppSystem.Object();

        Il2CppSystem.Action<RoleOptionEvent> role = new Action<RoleOptionEvent>(OnRole);
        Il2CppSystem.Action<FormationOptionEvent> formation = new Action<FormationOptionEvent>(OnFormation);
        Il2CppSystem.Action<TacticTeamSelectionDropdownEvent> selection = new Action<TacticTeamSelectionDropdownEvent>(OnSelection);
        Il2CppSystem.Action<ModifyTacticEvent> modify = new Action<ModifyTacticEvent>(OnModify);
        Il2CppSystem.Action<SwapPlayersEvent> swap = new Action<SwapPlayersEvent>(OnSwap);
        Il2CppSystem.Action<SelectPlayerEvent> selectPlayer = new Action<SelectPlayerEvent>(OnSelectPlayer);
        Il2CppSystem.Action<TacticsPositionButtonEvent> position = new Action<TacticsPositionButtonEvent>(OnPosition);
        Il2CppSystem.Action<TacticsPitchPositionEvent> pitch = new Action<TacticsPitchPositionEvent>(OnPitch);

        events.Register(owner, role);
        events.Register(owner, formation);
        events.Register(owner, selection);
        events.Register(owner, modify);
        events.Register(owner, swap);
        events.Register(owner, selectPlayer);
        events.Register(owner, position);
        events.Register(owner, pitch);

        Callbacks.AddRange([role, formation, selection, modify, swap, selectPlayer, position, pitch]);
        _owner = owner;

        return $"Recording tactics events. {InteropRecorder.Configure(options)} Change the tactic in the game, then read /events.";
    }

    public static string Stop()
    {
        if (_owner is null)
        {
            return "Not recording.";
        }

        Subsystems.Get<EventSubsystem>().UnregisterAll(_owner);
        Callbacks.Clear();
        _owner = null;

        return "Stopped recording.";
    }

    public static void Clear()
    {
        Recorded.Clear();
        DataRecorded.Clear();
    }

    public static List<Dictionary<string, object?>> Describe()
    {
        return Recorded.Select(Describe).ToList();
    }

    // The data stream, each entry with the binding path behind its data key when a node still points at it.
    public static List<Dictionary<string, object?>> DescribeData()
    {
        var paths = DataRecorded.Count == 0 ? [] : InteropRecorder.PathsByDataKey();

        return DataRecorded.Select(e =>
        {
            var description = Describe(e);

            if (e.Description.GetValueOrDefault("dataKey") is ulong key && paths.TryGetValue(key, out var path))
            {
                description["path"] = path;
            }

            return description;
        }).ToList();
    }

    private static Dictionary<string, object?> Describe(RecordedEvent e)
    {
        return new Dictionary<string, object?>
        {
            ["type"] = e.Type,
            ["key"] = e.Key,
            ["utc"] = e.Utc.ToString("HH:mm:ss.fff"),
            ["fields"] = e.Description
        };
    }

    public static RecordedEvent? Last(string type, string? key = null)
    {
        return Recorded.LastOrDefault(e => e.Type == type && (key is null || e.Key == key) && e.Error is null);
    }

    // The game calls these from its event dispatch: an exception escaping into native code would take it down.
    private static void OnRole(RoleOptionEvent evt)
    {
        Record(EventSender.RoleType, recorded =>
        {
            recorded.AddReference("Tactic", evt._Tactic_k__BackingField);
            recorded.AddValue("InPossession", evt._InPossession_k__BackingField);
            recorded.AddValue("SelectionIndex", evt._SelectionIndex_k__BackingField);
            recorded.AddValue("PositionMask", evt._PositionMask_k__BackingField);
            recorded.AddValue("Person", evt._Person_k__BackingField);
            recorded.AddValue("Data", evt._Data_k__BackingField);
        });
    }

    private static void OnFormation(FormationOptionEvent evt)
    {
        Record(EventSender.FormationType, recorded =>
        {
            recorded.AddReference("Target", evt._Target_k__BackingField);
            recorded.AddValue("InPossession", evt._InPossession_k__BackingField);
            recorded.AddValue("Data", evt._Data_k__BackingField);
        });
    }

    private static void OnSelection(TacticTeamSelectionDropdownEvent evt)
    {
        Record(EventSender.SelectionType, recorded =>
        {
            recorded.AddReference("Target", evt._Target_k__BackingField);
            recorded.AddReference("Team", evt._Team_k__BackingField);
            recorded.AddValue("Data", evt._Data_k__BackingField);
            recorded.AddValue("Position", evt._Position_k__BackingField);
            recorded.AddValue("IsInPossession", evt._IsInPossession_k__BackingField);
            recorded.AddValue("TeamSelectionIndex", evt._TeamSelectionIndex_k__BackingField);
        });
    }

    // Raised by the tactic menu, e.g. applying a preset from Config.
    private static void OnModify(ModifyTacticEvent evt)
    {
        Record(EventSender.ModifyType, recorded =>
        {
            recorded.AddReference("Target", evt._Target_k__BackingField);
            recorded.AddValue("Data", evt._Data_k__BackingField);
            recorded.AddValue("SecondaryData", evt._SecondaryData_k__BackingField);
            recorded.AddValue("Team", evt._Team_k__BackingField);
            recorded.AddPlain("IsAddingSlot", evt._IsAddingSlot_k__BackingField);
        });
    }

    // Dragging one lineup player onto another.
    private static void OnSwap(SwapPlayersEvent evt)
    {
        Record(SwapType, recorded =>
        {
            recorded.AddReference("EditSessionReference", evt._EditSessionReference_k__BackingField);
            recorded.AddPlain("Player1", evt._Player1_k__BackingField);
            recorded.AddPlain("Player2", evt._Player2_k__BackingField);
            recorded.AddPlain("SwapPosition", evt._SwapPosition_k__BackingField);
            recorded.AddPlain("IsInPossession", evt._IsInPossession_k__BackingField);
        });
    }

    private static void OnSelectPlayer(SelectPlayerEvent evt)
    {
        Record(SelectPlayerType, recorded =>
        {
            recorded.AddReference("EditSessionReference", evt._EditSessionReference_k__BackingField);
            recorded.AddReference("NewPlayerReference", evt._NewPlayerReference_k__BackingField);
            recorded.AddPlain("Position", evt._Position_k__BackingField);
        });
    }

    // A position marker on the pitch picked or moved.
    private static void OnPosition(TacticsPositionButtonEvent evt)
    {
        Record(PositionType, recorded =>
        {
            recorded.AddReference("EditSessionReference", evt._EditSessionReference_k__BackingField);
            recorded.AddPlain("OwnPosition", evt._OwnPosition_k__BackingField);
            recorded.AddPlain("SelectedPosition", evt._SelectedPosition_k__BackingField);
            recorded.AddPlain("OwnPersonIndex", evt._OwnPersonIndex_k__BackingField);
            recorded.AddPlain("SelectedPersonIndex", evt._SelectedPersonIndex_k__BackingField);
            recorded.AddPlain("IsInPossession", evt._IsInPossession_k__BackingField);
            recorded.AddPlain("ControllerSwappingMode", evt._ControllerSwappingMode_k__BackingField);
            recorded.AddNote("SelectedPositionKey", evt._SelectedPositionKey_k__BackingField.m_key);
            recorded.AddNote("SelectedPersonIndexKey", evt._SelectedPersonIndexKey_k__BackingField.m_key);
            recorded.AddNote("ControllerSwappingModeKey", evt._ControllerSwappingModeKey_k__BackingField.m_key);
        });
    }

    // A click on a position that opens one of its panels (role, instructions, player selection...).
    private static void OnPitch(TacticsPitchPositionEvent evt)
    {
        Record(PitchType, recorded =>
        {
            recorded.AddNote("PanelToOpen", evt._PanelToOpen_k__BackingField.ToString());
            recorded.AddPlain("TeamSelectionIndex", evt._TeamSelectionIndex_k__BackingField);
            recorded.AddPlain("InPossession", evt._InPossession_k__BackingField);
        });
    }

    private static void Record(string type, Action<RecordedEvent> fill)
    {
        Record(type, null, fill);
    }

    // Called from inside the game's own calls (event dispatch, InteropRecorder's hooks), so it never throws.
    public static void Record(string type, string? key, Action<RecordedEvent> fill, bool toDataStream = false)
    {
        try
        {
            var recorded = new RecordedEvent(type, key);

            try
            {
                fill(recorded);
            }
            catch (Exception exception)
            {
                // Kept with whatever was read before the failure, so /events shows what went wrong.
                recorded.Fail(exception.Message);
            }

            var buffer = toDataStream ? DataRecorded : Recorded;
            buffer.Add(recorded);

            if (buffer.Count > (toDataStream ? DataCapacity : Capacity))
            {
                buffer.RemoveAt(0);
            }
        }
        catch (Exception)
        {
            // Recording is best effort; the game's own handling of the event must never be disturbed.
        }
    }
}
