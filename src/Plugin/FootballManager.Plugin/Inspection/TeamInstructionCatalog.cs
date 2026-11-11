using FM.UI;
using Il2CppInterop.Runtime;
using SI.Translation;
using UnityEngine;

namespace FootballManager.Plugin.Inspection;

// Which options each team instruction offers. Only the tactics screen's UI asset knows: the bindings carry just the
// option chosen. Option names are property names, so their ids are what the tactic's SelectedTeamInstructionOption
// holds and takes, and an instruction's id is the TeamInstructionID context it's read with. Names come as the game
// shows them too, in the user's language.
internal static class TeamInstructionCatalog
{
    // The asset's option names that differ from the property registry's.
    private static readonly Dictionary<string, string> RegistryNames = new()
    {
        ["DefensiveLineMuchDeeper"] = "MuchDeeperDefensiveLine",
        ["DefensiveLineDeeper"] = "DeeperDefensiveLine",
        ["DefensiveLineHigherDefensiveLine"] = "HigherDefensiveLine",
        ["DefensiveLineMuchHigher"] = "MuchHigherDefensiveLine",
        ["LineOfEngagementHigh"] = "LineOfEngagementHigher",

        // A tactic holding WasteTime shows "Normal" on screen; the other two follow.
        ["WasteTimeLessOften"] = "TimeWastingNone",
        ["WasteTimeStandard"] = "WasteTime",
        ["WasteTimeMoreOften"] = "WasteTimeExtreme"
    };

    public static List<Dictionary<string, object?>> Describe()
    {
        var assets = Resources.FindObjectsOfTypeAll(Il2CppType.Of<TacticTeamInstructionData>());

        if (assets.Length == 0)
        {
            throw new InvalidOperationException("The game hasn't loaded its team instruction data yet: open the tactics screen once.");
        }

        var entries = assets[0].Cast<TacticTeamInstructionData>().m_storedDataEntryList
            ?? throw new InvalidOperationException("The team instruction data has no entries.");
        var instructions = new List<Dictionary<string, object?>>();

        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];

            if (entry is null)
            {
                continue;
            }

            var extra = entry.m_extraInstructionID;

            instructions.Add(new Dictionary<string, object?>
            {
                ["instruction"] = entry.m_instructionID.ToString(),
                ["teamInstructionId"] = (int)entry.m_instructionID,
                ["name"] = Translate(entry.m_nameTranslationID),
                ["phase"] = entry.m_phaseID.ToString(),
                ["extraInstruction"] = extra == TacticTeamInstructions.Invalid ? null : extra.ToString(),
                ["options"] = OptionsOf(entry)
            });
        }

        return instructions;
    }

    private static List<Dictionary<string, object?>> OptionsOf(TacticTeamInstructionEntry entry)
    {
        var options = new List<Dictionary<string, object?>>();
        var optionEntries = entry.m_instructionOptionEntries;

        for (var i = 0; optionEntries is not null && i < optionEntries.Length; i++)
        {
            var option = optionEntries[i];

            if (option is null)
            {
                continue;
            }

            // A managed enum's name, not the game's ToString (see TypedValueReader on boxed structs).
            var name = option.m_optionID.ToString();

            options.Add(new Dictionary<string, object?>
            {
                ["option"] = name,
                ["id"] = IdentifierCatalog.TryIdOf(RegistryNames.GetValueOrDefault(name, name)),
                ["name"] = Translate(option.m_nameTranslationID)
            });
        }

        return options;
    }

    // Through the game's static translator, with the id read from the struct's field (no call on the struct itself).
    private static string? Translate(TranslationID id)
    {
        try
        {
            var translation = TranslationManager.Translate(id.GameID);
            return translation is null ? null : RichText.StripRecordLinks(translation);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
