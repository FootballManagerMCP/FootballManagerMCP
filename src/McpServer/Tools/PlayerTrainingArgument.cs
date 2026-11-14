using System.ComponentModel;

namespace McpServer.Tools;

[Description("A player's own training to change. Anything left out stays as it is.")]
public sealed class PlayerTrainingArgument
{
    [Description("The player's FM id, as get_training lists him.")]
    public int PlayerId { get; set; }

    [Description("An additional focus's id from get_training's focusOptions, or -1 for none. Goalkeepers and outfield " +
                 "players have their own; injured players also have rehabilitation.")]
    public long? Focus { get; set; }

    [Description("An id from get_training's intensityOptions: -1 automatic (the game picks), 0 half, 1 normal, 2 double, " +
                 "or a rest period (no pitch or gym work for some days).")]
    public long? Intensity { get; set; }
}
