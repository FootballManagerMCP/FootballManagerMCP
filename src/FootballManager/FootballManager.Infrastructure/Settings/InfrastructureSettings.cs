namespace FootballManager.Infrastructure.Settings;

public class InfrastructureSettings
{
    /// <summary>Where the MCPFM plugin inside the game listens (its BepInEx config sets the port).</summary>
    public required string GameBridgeBaseUrl { get; set; }

    /// <summary>
    /// Host header to send instead of the URL's. The plugin only answers requests addressed to localhost, so a
    /// container reaching it as host.docker.internal sets "localhost:5121" here.
    /// </summary>
    public string? GameBridgeHostHeader { get; set; }

    public int TimeoutSeconds { get; set; } = 60;
}
