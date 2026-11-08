using FM.UI;
using FootballManager.Plugin.Data;
using FootballManager.Plugin.Inspection;
using FootballManager.Plugin.Writes;
using SI.Bindable;
using SI.Core;
using UnityEngine.SceneManagement;
using DateTime = System.DateTime;

namespace FootballManager.Plugin.Session;

// Opens and closes the one save the bridge may handle on its own: a copy of the user's career kept for investigation
// (the BepInEx setting Session.InvestigationSave), so an AI can start the game, look around and quit without the user.
// The user's own saves are never opened or closed: /load works only from the start screen and opens only that copy,
// and /quit works only on the start screen or with that copy loaded.
//
// Loading raises the event the load screen raises when a save is picked (LoadSelectedGameEvent), with the record it
// carries: the file name, an empty folder (the default games folder), ".fm", and the rest copied from the game's own
// last-save record, whose field ids it reuses. Quitting is where the quit dialog's "Yes" leads, ForceQuitGame, which
// asks nothing. Both keep the game's most recent save pointing at the user's own (LastSavedPreferences).
internal static class SaveSession
{
    private const string AppRoot = "app";
    private const string LastSavePath = "LastSave";
    private const string FileNameField = "SelectedFileName";
    private const string FolderField = "SelectedFolderName";
    private const string FileTypeField = "SelectedFileType";
    private const string SaveMethodField = "SelectedSaveMethod";
    private const string LoadScopeField = "SelectedLoadScope";
    private const string SaveFileType = ".fm";

    // A load takes about ten seconds; until the game is loaded, a second /load would start another.
    private static readonly TimeSpan LoadInProgress = TimeSpan.FromSeconds(90);

    private static readonly string GamesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Sports Interactive", "Football Manager 27", "games");

    private static DateTime? _loadSentUtc;

    public static async Task<string> DescribeAsync(string investigationSave, TimeSpan timeout)
    {
        var state = await StateAsync(timeout);
        return Json.Serialize(Describe(state, investigationSave));
    }

    public static async Task<string> LoadAsync(string investigationSave, bool send, TimeSpan timeout)
    {
        var file = SaveFile(investigationSave);

        if (!File.Exists(file))
        {
            throw new ArgumentException($"There's no investigation save at {file}. Copy the user's save there first.");
        }

        var state = await StateAsync(timeout);

        if (state.GameLoaded)
        {
            throw new ArgumentException(
                $"A save is loaded ({state.LastSaveName ?? "unknown"}); /load works only from the start screen. Quit first: /quit if it's the "
                + "investigation save, otherwise that's the user's to do.");
        }

        if (DateTime.UtcNow - _loadSentUtc < LoadInProgress)
        {
            throw new ArgumentException("A load is already under way; watch /session until the save is loaded.");
        }

        var lastSave = state.LastSave
            ?? throw new ArgumentException("The game has no last-save record to build the load on (has a save ever been loaded?).");

        var kept = send ? LastSavedPreferences.Keep(investigationSave) : [];

        var sent = await MainThreadDispatcher.RunAsync(() =>
        {
            var record = LoadRecord(lastSave, investigationSave);

            // Read before sending: the game's handler owns the record from then on.
            var description = TypedValueReader.Read(TypedValue.Create<DynamicReference>(record));

            if (send)
            {
                Subsystems.Get<EventSubsystem>().SendEvent(new LoadSelectedGameEvent { LoadGameDataRecord = record });
                _loadSentUtc = DateTime.UtcNow;
            }

            return description;
        });

        return Json.Serialize(new Dictionary<string, object?>
        {
            ["save"] = investigationSave,
            ["file"] = file,
            ["dryRun"] = !send,
            ["sent"] = sent,
            ["preferencesKept"] = kept,
            ["next"] = send ? "The game takes about ten seconds; watch /session until loaded is true." : null
        });
    }

    public static async Task<string> QuitAsync(string investigationSave, bool send, TimeSpan timeout)
    {
        var state = await StateAsync(timeout);

        if (state.GameLoaded && !IsInvestigationSave(state.LastSaveName, investigationSave))
        {
            throw new ArgumentException(
                $"The loaded save is {state.LastSaveName ?? "unknown"}, not the investigation save ({investigationSave}). Quitting the user's "
                + "own game is theirs to do.");
        }

        var restored = send ? LastSavedPreferences.Restore() : [];

        if (send)
        {
            await MainThreadDispatcher.RunAsync(() =>
            {
                GlobalGameModule.ForceQuitGame((int)QuitGameEventAction.ExitGame);
                return true;
            });
        }

        return Json.Serialize(new Dictionary<string, object?>
        {
            ["quitting"] = state.GameLoaded ? investigationSave : "the start screen",
            ["dryRun"] = !send,
            ["preferencesRestored"] = restored
        });
    }

    private static Dictionary<string, object?> Describe(State state, string investigationSave)
    {
        var file = SaveFile(investigationSave);

        return new Dictionary<string, object?>
        {
            ["scene"] = state.Scene,
            ["loaded"] = state.GameLoaded,
            ["loadedSave"] = state.GameLoaded ? state.LastSaveName : null,
            ["isInvestigationSave"] = state.GameLoaded && IsInvestigationSave(state.LastSaveName, investigationSave),
            ["mostRecentSave"] = state.LastSaveName,
            ["loading"] = !state.GameLoaded && DateTime.UtcNow - _loadSentUtc < LoadInProgress,
            ["investigationSave"] = investigationSave,
            ["investigationSaveFile"] = File.Exists(file) ? file : null,
            ["preferencesKept"] = LastSavedPreferences.AreKept
        };
    }

    private static async Task<State> StateAsync(TimeSpan timeout)
    {
        var (scene, loaded) = await MainThreadDispatcher.RunAsync(() =>
        {
            var human = GameBindings.FindChild(GameBindings.Get().m_root, DataQueries.HumanRoot);

            // The user's manager is a person record once a save is loaded, and a null reference before.
            var person = human is null ? null : TypedValueReader.DatabaseIndexOf(GameBindings.ValueOf(human));
            return (SceneManager.GetActiveScene().name, person is not null);
        });

        TypedValue? lastSave;

        try
        {
            (lastSave, _) = await DataQueries.ResolveSourceAsync(new DataQuery(AppRoot, [], BasePath: LastSavePath), timeout);
        }
        catch (InvalidOperationException)
        {
            // Not answered: the game has no last save yet.
            lastSave = null;
        }

        var name = lastSave is null ? null : await MainThreadDispatcher.RunAsync(() => FieldText(lastSave, FileNameField));
        return new State(scene, loaded, lastSave, name);
    }

    // The load screen's record: the investigation save's name in the default games folder, a local save loaded in full,
    // and whatever else the game's own last-save record holds (its date), keyed as the game keys it.
    private static DynamicReference LoadRecord(TypedValue lastSave, string investigationSave)
    {
        var fields = TypedValueReader.AsRecord(lastSave)
            ?? throw new InvalidOperationException("The game's last-save record isn't a record.");
        var record = new DynamicReference();
        var names = new HashSet<string>();

        foreach (var pair in fields)
        {
            var name = IdentifierCatalog.NameOf(pair.Key);
            names.Add(name);

            record.Add(pair.Key, name switch
            {
                FileNameField => TypedValue.Create(investigationSave),
                FolderField => TypedValue.Create(""),
                FileTypeField => TypedValue.Create(SaveFileType),
                SaveMethodField or LoadScopeField => TypedValue.Create(0),
                _ => TypedValue.CreateCopyFrom(pair.Value)
            });
        }

        var missing = new[] { FileNameField, FolderField, FileTypeField, SaveMethodField, LoadScopeField }.Except(names).ToList();

        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"The game's last-save record has no {string.Join(", ", missing)}: its layout has changed.");
        }

        return record;
    }

    private static string? FieldText(TypedValue record, string field)
    {
        var fields = TypedValueReader.AsRecord(record);

        if (fields is null)
        {
            return null;
        }

        foreach (var pair in fields)
        {
            if (IdentifierCatalog.NameOf(pair.Key) == field)
            {
                return TypedValueReader.Read(pair.Value) as string;
            }
        }

        return null;
    }

    private static bool IsInvestigationSave(string? name, string investigationSave)
    {
        return string.Equals(name, investigationSave, StringComparison.OrdinalIgnoreCase);
    }

    private static string SaveFile(string investigationSave)
    {
        return Path.Combine(GamesFolder, investigationSave + SaveFileType);
    }

    private sealed record State(string Scene, bool GameLoaded, TypedValue? LastSave, string? LastSaveName);
}
