using Il2CppInterop.Runtime;
using SI.Translation;
using UnityEngine;

namespace FootballManager.Plugin.Inspection;

// Which options each player instruction group offers, with the game's names for both, from the tactics screen's UI
// asset (the bindings carry only option numbers). A group's id is the PlayerInstructionGroupID context the tactic's
// SelectedPlayerInstruction is read and written with; an option's id is the value it holds. Names come as the game
// shows them, in the user's language.
internal static class PlayerInstructionCatalog
{
    public static List<Dictionary<string, object?>> Describe()
    {
        var assets = Resources.FindObjectsOfTypeAll(Il2CppType.Of<PlayerInstructionEntries>());

        if (assets.Length == 0)
        {
            throw new InvalidOperationException("The game hasn't loaded its player instruction data yet: open a player's instructions once.");
        }

        var groups = assets[0].Cast<PlayerInstructionEntries>().m_playerInstructionGroups
            ?? throw new InvalidOperationException("The player instruction data has no groups.");
        var described = new List<Dictionary<string, object?>>();

        for (var i = 0; i < groups.Length; i++)
        {
            var group = groups[i];

            if (group is null)
            {
                continue;
            }

            // Managed enums' names, not the game's ToString (see TypedValueReader on boxed structs).
            var extra = group.m_extraGroupID;

            described.Add(new Dictionary<string, object?>
            {
                ["group"] = group.m_groupID.ToString(),
                ["groupId"] = (int)group.m_groupID,
                ["name"] = Translate(group.m_groupName),
                ["extraGroup"] = extra == FM.UI.PlayerInstructionGrouping.Invalid ? null : extra.ToString(),
                ["options"] = OptionsOf(group)
            });
        }

        return described;
    }

    private static List<Dictionary<string, object?>> OptionsOf(FM.UI.PlayerInstructionGroup group)
    {
        var options = new List<Dictionary<string, object?>>();
        var entries = group.m_playerInstructionOptionEntries;

        for (var i = 0; entries is not null && i < entries.Length; i++)
        {
            var option = entries[i];

            if (option is null)
            {
                continue;
            }

            options.Add(new Dictionary<string, object?>
            {
                ["option"] = option.m_instructionID.ToString(),
                ["id"] = (int)option.m_instructionID,
                ["name"] = Translate(option.m_instructionTranslation)
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
