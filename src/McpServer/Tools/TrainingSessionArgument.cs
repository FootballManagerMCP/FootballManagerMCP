using System.ComponentModel;

namespace McpServer.Tools;

[Description("A session of the training schedule to replace.")]
public sealed class TrainingSessionArgument
{
    [Description("The day, yyyy-MM-dd, in this week or the next as get_training lists them.")]
    public string Date { get; set; } = string.Empty;

    [Description("The session's place in the day, 0 to 2, as get_training lists it.")]
    public int Slot { get; set; }

    [Description("The new session's id from get_training's sessionTypes, e.g. 304.")]
    public int Session { get; set; }
}
