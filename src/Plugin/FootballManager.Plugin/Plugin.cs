using BepInEx;
using BepInEx.Unity.IL2CPP;
using FootballManager.Plugin.Session;

namespace FootballManager.Plugin;

[BepInPlugin(Id, Name, Version)]
public sealed class Plugin : BasePlugin
{
    public const string Id = "mcpfm.bridge";
    public const string Name = "MCPFM Bridge";
    public const string Version = "0.13.1";

    private BridgeServer? _server;

    public override void Load()
    {
        var port = Config.Bind("Bridge", "Port", 5121, "Localhost port the MCPFM API talks to the game on.").Value;
        var investigationSave = Config.Bind("Session", "InvestigationSave", "MCPFM investigation",
            "The only save /load opens and the only loaded save /quit closes: a copy of a career in the games folder, named "
            + "without .fm, for an AI to investigate on its own. The user's own saves are never opened or closed.").Value;

        RestoreLastSavedPreferences();
        AddComponent<MainThreadDispatcher>();

        _server = new BridgeServer(port, investigationSave, Log);
        _server.Start();
    }

    public override bool Unload()
    {
        _server?.Dispose();
        return true;
    }

    // A session that loaded the investigation save but didn't quit through the bridge (a crash, the user closing the
    // game) left the game's most recent save pointing at the copy.
    private void RestoreLastSavedPreferences()
    {
        try
        {
            var restored = LastSavedPreferences.Restore();

            if (restored.Count > 0)
            {
                Log.LogInfo($"Put back the most recent save the investigation save replaced: {string.Join(", ", restored)}.");
            }
        }
        catch (Exception exception)
        {
            Log.LogError($"Couldn't put back the game's most recent save: {exception.Message}");
        }
    }
}
