using BepInEx;
using BepInEx.Unity.IL2CPP;

namespace FootballManager.Plugin;

[BepInPlugin(Id, Name, Version)]
public sealed class Plugin : BasePlugin
{
    public const string Id = "mcpfm.bridge";
    public const string Name = "MCPFM Bridge";
    public const string Version = "0.4.1";

    private BridgeServer? _server;

    public override void Load()
    {
        var port = Config.Bind("Bridge", "Port", 5121, "Localhost port the MCPFM API talks to the game on.").Value;

        AddComponent<MainThreadDispatcher>();

        _server = new BridgeServer(port, Log);
        _server.Start();
    }

    public override bool Unload()
    {
        _server?.Dispose();
        return true;
    }
}
