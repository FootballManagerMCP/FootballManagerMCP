using FM.UI;
using FootballManager.Plugin.Data;
using SI.Core;
using SI.Interop;

namespace FootballManager.Plugin.Writes;

// Sends one of the tactics events the game's screens raise (a role picked, a formation chosen, a player put in a
// slot, a preset applied), for the game's own handler to act on exactly as after a click. Each field comes from the request, or else
// from the last recorded event of that type, so a real click's contents can serve as the template.
internal static class EventSender
{
    public const string RoleType = "role";
    public const string FormationType = "formation";
    public const string SelectionType = "selection";

    // Raised by the tactic menu, e.g. applying a preset: Data is a row of the tactic's ModifyTacticOptions tree.
    public const string ModifyType = "modify";

    // The value fields each event carries, and the game records it targets.
    private static readonly Dictionary<string, (string[] Values, string[] References)> Fields = new()
    {
        [RoleType] = (["InPossession", "SelectionIndex", "PositionMask", "Person", "Data"], ["Tactic"]),
        [FormationType] = (["InPossession", "Data"], ["Target"]),
        [SelectionType] = (["Data", "Position", "IsInPossession", "TeamSelectionIndex"], ["Target", "Team"]),
        [ModifyType] = (["Data", "SecondaryData", "Team", "IsAddingSlot"], ["Target"])
    };

    public static IReadOnlyCollection<string> Types => Fields.Keys;

    public static async Task<string> SendAsync(
        DataQuery query, string type, IReadOnlyDictionary<string, ValueSource> given, bool useTemplate, bool send, TimeSpan timeout)
    {
        if (!Fields.TryGetValue(type, out var fields))
        {
            throw new ArgumentException($"Unknown event type '{type}'. Use one of {string.Join(", ", Fields.Keys)}.");
        }

        var unknown = given.Keys.Except(fields.Values.Concat(fields.References), StringComparer.Ordinal).ToList();

        if (unknown.Count > 0)
        {
            throw new ArgumentException(
                $"A {type} event has no {string.Join(", ", unknown)}; its fields are {string.Join(", ", fields.Values.Concat(fields.References))}.");
        }

        var (source, contexts) = await DataQueries.ResolveSourceAsync(query, timeout);
        var values = await ValueSources.ResolveAsync(source, contexts, given, timeout);

        return await MainThreadDispatcher.RunAsync(() =>
        {
            var template = useTemplate ? EventRecorder.Last(type) : null;

            // The record the request reads from is the tactic the event targets, when it has a base (base=Team.Tactic).
            var baseRecord = query.BasePath is null ? null : TypedValueReader.AsReference(source);
            var sent = new RecordedEvent(type);

            TypedValue Value(string name)
            {
                var value = values.GetValueOrDefault(name) ?? template?.Values.GetValueOrDefault(name)
                    ?? throw new ArgumentException($"No {name} for the {type} event: give it, or record one by hand to use as the template.");

                sent.AddValue(name, value);
                return TypedValue.CreateCopyFrom(value);
            }

            InteropReference Reference(string name, bool baseIsCandidate = true)
            {
                var reference = (values.TryGetValue(name, out var value) ? TypedValueReader.AsReference(value) : null)
                    ?? (baseIsCandidate ? baseRecord : null) ?? template?.References.GetValueOrDefault(name)
                    ?? throw new ArgumentException($"No {name} record for the {type} event: read from base=Team.Tactic, or give it.");

                sent.AddReference(name, reference);
                return reference;
            }

            bool Flag(string name)
            {
                return TypedValueReader.Read(Value(name)) as bool?
                    ?? throw new ArgumentException($"{name} for the {type} event must be true or false.");
            }

            EventBase evt = type switch
            {
                RoleType => new RoleOptionEvent
                {
                    _Tactic_k__BackingField = Reference("Tactic"),
                    _InPossession_k__BackingField = Value("InPossession"),
                    _SelectionIndex_k__BackingField = Value("SelectionIndex"),
                    _PositionMask_k__BackingField = Value("PositionMask"),
                    _Person_k__BackingField = Value("Person"),
                    _Data_k__BackingField = Value("Data")
                },
                FormationType => new FormationOptionEvent
                {
                    _Target_k__BackingField = Reference("Target"),
                    _InPossession_k__BackingField = Value("InPossession"),
                    _Data_k__BackingField = Value("Data")
                },
                SelectionType => new TacticTeamSelectionDropdownEvent
                {
                    _Target_k__BackingField = Reference("Target"),
                    _Team_k__BackingField = TeamOf(Reference("Team", baseIsCandidate: false)),
                    _Data_k__BackingField = Value("Data"),
                    _Position_k__BackingField = Value("Position"),
                    _IsInPossession_k__BackingField = Value("IsInPossession"),
                    _TeamSelectionIndex_k__BackingField = Value("TeamSelectionIndex")
                },
                _ => new ModifyTacticEvent
                {
                    _Target_k__BackingField = Reference("Target"),
                    _Data_k__BackingField = Value("Data"),
                    _SecondaryData_k__BackingField = Value("SecondaryData"),
                    _Team_k__BackingField = Value("Team"),
                    _IsAddingSlot_k__BackingField = Flag("IsAddingSlot")
                }
            };

            if (send)
            {
                Subsystems.Get<EventSubsystem>().SendEvent(evt);
            }

            return Json.Serialize(new Dictionary<string, object?>
            {
                ["type"] = type,
                ["dryRun"] = !send,
                ["template"] = template?.Utc.ToString("HH:mm:ss.fff"),
                ["sent"] = sent.Description
            });
        });
    }

    private static TeamReference TeamOf(InteropReference reference)
    {
        return reference.TryCast<TeamReference>() ?? throw new ArgumentException("The Team given isn't a team.");
    }
}
