using System.Runtime.CompilerServices;
using FM.GamePlugin;
using FootballManager.Plugin.Data;
using HarmonyLib;
using SI.Bindable;
using SI.Core;
using SI.Interop;
using ChannelActions = Il2CppSystem.Collections.Generic.List<FM.GamePlugin.ChannelAction>;
using Time = UnityEngine.Time;

namespace FootballManager.Plugin.Writes;

// Records what the UI sends the native game below the tactics events the recorder listens to by type:
//   native events  (changing formation, the tactic creator's steps, drag and drop) through the interop subsystem
//                  and on into its plugin object;
//   binding writes (dropdowns) one by one through the same two, or batched through PerformChannelActions;
//   UI events      of any class, through the UI event system and its dispatcher;
//   data traffic   (SetData: the screens publishing their data) into a separate buffer, as it comes in bursts.
// Each method is patched with a prefix that copies its arguments and returns. Both layers are patched because the
// subsystem's methods are thin enough for the compiler to have inlined them into some callers; what both see in the
// same frame is recorded once.
//
// The patches go in the first time a recording asks for them and stay, doing nothing while the recorder is off:
// patching is the risky part, so it happens once, when the user starts a recording. The game makes these calls on the
// main thread.
internal static class InteropRecorder
{
    public const string NativeType = "native";
    public const string SicoreType = "sicore";
    public const string BindingType = "binding";
    public const string UiType = "ui";
    public const string DataType = "data";

    private const string HarmonyId = "mcpfm.bridge.recorder";
    private const int MaxPathDepth = 40;
    private const int MaxTreeNodes = 500_000;

    // The game's background chatter: an idle timer written every second, an audit heartbeat every minute.
    private static readonly HashSet<string> IgnoredProperties = ["IdleTime"];
    private static readonly HashSet<string> IgnoredEvents = ["EventFmxAuditHeartbeat"];

    private static readonly List<string> Hooked = [];
    private static readonly List<string> Failed = [];
    private static Harmony? _harmony;
    private static bool _eventsHooked;
    private static bool _writesHooked;
    private static bool _uiHooked;

    // What the current recording catches (see RecorderOptions).
    private static bool _events;
    private static bool _writes;
    private static bool _data;
    private static bool _ui;

    // What the upper layer last passed on, so the lower layer skips its own copy.
    private static (int Frame, uint EventId)? _lastSubsystemEvent;
    private static (int Frame, ulong Key)? _lastSubsystemWrite;
    private static (int Frame, IntPtr Event)? _lastUiEvent;

    public static string Configure(RecorderOptions options)
    {
        _events = options.Interop;
        _writes = options.Interop && options.Writes;
        _data = _writes && options.Data;
        _ui = options.Interop && options.Ui;

        if (!options.Interop)
        {
            return "Native events, binding writes and other UI events aren't recorded (interop=false).";
        }

        _harmony ??= new Harmony(HarmonyId);

        if (!_eventsHooked)
        {
            _eventsHooked = true;
            Patch(typeof(GameInteropSubsystem), nameof(GameInteropSubsystem.SendEvent),
                [typeof(uint), typeof(InteropReference), typeof(TypedValue)], nameof(OnSubsystemEvent));
            Patch(typeof(GameInteropSubsystem), nameof(GameInteropSubsystem.SendSicoreEvent), [typeof(uint)], nameof(OnSicoreEvent));
            Patch(typeof(GamePlugin), nameof(GamePlugin.SendEvent),
                [typeof(uint), typeof(InteropReference), typeof(TypedValue)], nameof(OnPluginEvent));
        }

        // These take struct arguments, the least proven kind of patch: writes=false leaves them out.
        if (_writes && !_writesHooked)
        {
            _writesHooked = true;
            Patch(typeof(GameInteropSubsystem), nameof(GameInteropSubsystem.UpdateChannelData),
                [typeof(Bindings.Key), typeof(TypedValue)], nameof(OnSubsystemWrite));
            Patch(typeof(GamePlugin), nameof(GamePlugin.UpdateChannelData),
                [typeof(ChannelAction), typeof(TypedValue)], nameof(OnPluginWrite));
            Patch(typeof(GamePlugin), nameof(GamePlugin.PerformChannelActions), null, nameof(OnBatch));
        }

        if (_ui && !_uiHooked)
        {
            _uiHooked = true;
            Patch(typeof(EventSubsystem), nameof(EventSubsystem.SendEvent), [typeof(EventBase)], nameof(OnUiEvent));
            Patch(typeof(EventDispatcher), nameof(EventDispatcher.SendEvent), [typeof(EventBase)], nameof(OnUiEvent));
        }

        var recording = new List<string> { "native events" };

        if (_writes)
        {
            recording.Add("binding writes");
        }

        if (_ui)
        {
            recording.Add("every UI event");
        }

        if (_data)
        {
            recording.Add("the screens' data traffic (read it with stream=data)");
        }

        var hooks = Hooked.Count == 0 ? "nothing" : string.Join(", ", Hooked);
        var failures = Failed.Count == 0 ? "" : $"; failed {string.Join("; ", Failed)}";
        return $"Also recording {string.Join(", ", recording)} (hooked {hooks}{failures}).";
    }

    public static string EventName(uint eventId)
    {
        return Enum.IsDefined(typeof(InteropEventIDs), eventId) ? ((InteropEventIDs)eventId).ToString() : $"#{eventId}";
    }

    // Data keys to the binding paths of the nodes that point at them, from the binding tree as it is now. Built when the
    // data stream is read rather than in the hooks, which see bursts of hundreds of writes a frame.
    public static Dictionary<ulong, string> PathsByDataKey()
    {
        var paths = new Dictionary<ulong, string>();
        var pending = new Stack<(Bindings.Node Node, string Path)>();

        for (var child = GameBindings.Get().m_root.m_firstChild; child is not null; child = child.m_nextSibling)
        {
            pending.Push((child, child.m_name));
        }

        for (var visited = 0; pending.Count > 0 && visited < MaxTreeNodes; visited++)
        {
            var (node, path) = pending.Pop();
            var dataKey = node.m_dataKey.m_key;

            if (dataKey != 0)
            {
                paths.TryAdd(dataKey, path);
            }

            for (var child = node.m_firstChild; child is not null; child = child.m_nextSibling)
            {
                pending.Push((child, $"{path}.{child.m_name}"));
            }
        }

        return paths;
    }

    private static void Patch(Type type, string method, Type[]? parameters, string prefix)
    {
        var name = $"{type.Name}.{method}";

        try
        {
            var original = AccessTools.Method(type, method, parameters)
                ?? throw new MissingMethodException(type.FullName, method);

            _harmony!.Patch(original, prefix: new HarmonyMethod(AccessTools.Method(typeof(InteropRecorder), prefix)));
            Hooked.Add(name);
        }
        catch (Exception exception)
        {
            Failed.Add($"{name}: {exception.Message}");
        }
    }

    // The prefixes run inside the game's calls: nothing may escape them, and they never stop the original from running.
    // Their parameter names are the originals', which Harmony matches them by.
    private static void OnSubsystemEvent(uint eventID, InteropReference objectTo, TypedValue data)
    {
        if (!EventRecorder.IsRecording || !_events)
        {
            return;
        }

        try
        {
            _lastSubsystemEvent = (Time.frameCount, eventID);
            RecordNative(eventID, objectTo, data, "GameInteropSubsystem.SendEvent");
        }
        catch (Exception)
        {
        }
    }

    private static void OnPluginEvent(uint eventID, InteropReference objectTo, TypedValue data)
    {
        if (!EventRecorder.IsRecording || !_events)
        {
            return;
        }

        try
        {
            if (_lastSubsystemEvent != (Time.frameCount, eventID))
            {
                RecordNative(eventID, objectTo, data, "GamePlugin.SendEvent");
            }
        }
        catch (Exception)
        {
        }
    }

    private static void OnSicoreEvent(uint eventID)
    {
        if (!EventRecorder.IsRecording || !_events)
        {
            return;
        }

        EventRecorder.Record(SicoreType, $"#{eventID}", recorded => recorded.AddNote("eventId", eventID));
    }

    private static void OnSubsystemWrite(Bindings.Key key, TypedValue data)
    {
        if (!EventRecorder.IsRecording || !_writes)
        {
            return;
        }

        try
        {
            _lastSubsystemWrite = (Time.frameCount, key.m_key);
            RecordWrite(key, data, "GameInteropSubsystem.UpdateChannelData");
        }
        catch (Exception)
        {
        }
    }

    // Carries every kind of channel action; only UpdateChannelData is a binding write, the rest is data traffic.
    private static void OnPluginWrite(ChannelAction action, TypedValue value)
    {
        if (!EventRecorder.IsRecording || !_writes)
        {
            return;
        }

        try
        {
            if (action.mode != ChannelActionMode.UpdateChannelData)
            {
                if (_data)
                {
                    RecordData(action, value);
                }
            }
            else if (_lastSubsystemWrite != (Time.frameCount, action.key))
            {
                RecordWrite(KeyOf(action.key), value, "GamePlugin.UpdateChannelData");
            }
        }
        catch (Exception)
        {
        }
    }

    // Batched channel actions. The values of batched writes travel in a list of tuples, which aren't read: structs with
    // references are what crashed the game when touched through interop. Which property was written still shows.
    private static void OnBatch(ChannelActions actions)
    {
        if (!EventRecorder.IsRecording || !_writes || actions is null)
        {
            return;
        }

        try
        {
            for (var i = 0; i < actions.Count; i++)
            {
                var action = actions[i];

                if (action.mode == ChannelActionMode.UpdateChannelData)
                {
                    RecordWrite(KeyOf(action.key), null, "GamePlugin.PerformChannelActions");
                }
            }
        }
        catch (Exception)
        {
        }
    }

    private static void OnUiEvent(EventBase evt)
    {
        if (!EventRecorder.IsRecording || !_ui || evt is null)
        {
            return;
        }

        try
        {
            // The subsystem hands its events to the dispatcher, which would record them again.
            if (_lastUiEvent == (Time.frameCount, evt.Pointer))
            {
                return;
            }

            _lastUiEvent = (Time.frameCount, evt.Pointer);
            var name = evt.GetIl2CppType().FullName;

            EventRecorder.Record(UiType, name, recorded =>
            {
                foreach (var (field, value) in ObjectFieldReader.Read(evt, "EventBase"))
                {
                    recorded.AddNote(field, value);
                }
            });
        }
        catch (Exception)
        {
        }
    }

    private static void RecordNative(uint eventId, InteropReference? target, TypedValue? data, string via)
    {
        var name = EventName(eventId);

        if (IgnoredEvents.Contains(name))
        {
            return;
        }

        EventRecorder.Record(NativeType, name, recorded =>
        {
            recorded.AddNote("event", name);
            recorded.AddNote("eventId", eventId);
            recorded.AddNote("via", via);
            recorded.AddReference("Target", target);
            recorded.AddValue("Data", data);
        });
    }

    private static void RecordWrite(Bindings.Key key, TypedValue? value, string via)
    {
        var node = NodeOf(key);
        var property = node?.m_name;

        if (property is not null && IgnoredProperties.Contains(property))
        {
            return;
        }

        EventRecorder.Record(BindingType, property, recorded =>
        {
            recorded.AddNote("property", property);
            recorded.AddNote("path", node is null ? null : PathOf(node));
            recorded.AddNote("key", key.m_key);
            recorded.AddNote("via", via);
            recorded.AddNote("record", node is null ? null : RecordAbove(node));

            if (value is null)
            {
                recorded.AddNote("Value", "(not read: batched)");
            }
            else
            {
                recorded.AddValue("Value", value);
            }
        });
    }

    private static void RecordData(ChannelAction action, TypedValue? value)
    {
        EventRecorder.Record(DataType, action.mode.ToString(), recorded =>
        {
            recorded.AddNote("dataKey", action.key);
            recorded.AddNote("value", TypedValueReader.ReadCompact(value));
        }, toDataStream: true);
    }

    // A Key is its 64 bits; reinterpreted here rather than built through the game's constructor.
    private static Bindings.Key KeyOf(ulong raw)
    {
        return Unsafe.As<ulong, Bindings.Key>(ref raw);
    }

    private static Bindings.Node? NodeOf(Bindings.Key key)
    {
        return GameBindings.Get().TryGetNode(ref key, out var node) ? node?.TryCast<Bindings.Node>() : null;
    }

    // The node names from the root down, e.g. a screen's container, then "tactic", then the property written.
    private static string PathOf(Bindings.Node node)
    {
        var names = new List<string>();

        for (var current = node; current is not null && names.Count < MaxPathDepth; current = current.m_parent)
        {
            if (!string.IsNullOrEmpty(current.m_name))
            {
                names.Add(current.m_name);
            }
        }

        names.Reverse();
        return string.Join('.', names);
    }

    // The record the written property belongs to (e.g. the team's tactic), held by the node above it.
    private static Dictionary<string, object?>? RecordAbove(Bindings.Node node)
    {
        if (node.m_parent is not { } parent)
        {
            return null;
        }

        var key = parent.m_key;
        var reference = TypedValueReader.AsReference(GameBindings.Get().Get(ref key));
        return reference is null ? null : TypedValueReader.ReadReference(reference);
    }
}
