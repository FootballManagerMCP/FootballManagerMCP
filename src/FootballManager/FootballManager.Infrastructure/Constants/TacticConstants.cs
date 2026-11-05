namespace FootballManager.Infrastructure.Constants;

// The game's property paths and contexts for tactics, as its tactics and tactical effectiveness screens bind them.
internal static class TacticConstants
{
    // The tactic the team plays. The game wants a "Boolean" context with it, which its screens pass as true.
    public const string Tactic = "Team.Tactic";
    public const string TacticFlag = "Boolean";

    public const string SquadPath = "Team.Players";
    public const string TeamPath = "Team";

    // Contexts.
    public const string TeamSelectionIndex = "TeamSelectionIndex";
    public const string InPossession = "InPossession";
    public const string TacticIndex = "TacticIndex";
    public const string PositionMask = "PositionMask";
    public const string RoleMask = "RoleMask";
    public const string Person = "Person";
    public const string HumanTeam = "HumanTeam";

    // The tactic as a whole.
    public const string CurrentSlot = "CurrentTacticSlot";
    public const string Slot1Name = "TacticSlot1Name";
    public const string Slot2Name = "TacticSlot2Name";
    public const string Slot3Name = "TacticSlot3Name";
    public const string Slot1Valid = "TacticSlot1Valid";
    public const string Slot2Valid = "TacticSlot2Valid";
    public const string Slot3Valid = "TacticSlot3Valid";
    public const string Mentality = "MentalityString";
    public const string Style = "TacticalStyleDropdown";
    public const string StyleOptions = "TacticalStyleDropdownOptions";
    public const string RecommendedStyles = "TopThreeRecommendedTacticalStyles";
    public const string FormationName = "FormationName";

    // Each holds the id of the chosen option, which the game names (e.g. "MuchHigherTempo").
    public static readonly string[] TeamInstructions =
    [
        "Tempo", "TacticWidth", "PassingDirectness", "CrossingStyle", "DefensiveLine", "DefensiveLineBehaviour",
        "LineOfEngagement", "PressingTrigger", "PressingTrap", "AttackingTransition", "DefensiveTransition"
    ];

    public static readonly string[] Overview =
    [
        CurrentSlot, Slot1Name, Slot2Name, Slot3Name, Slot1Valid, Slot2Valid, Slot3Valid, Mentality, MentalityValue, Style,
        StyleOptions, RecommendedStyles, .. TeamInstructions
    ];

    // A player's roles at a position with their ratings (contexts PositionMask and InPossession, plus human and team).
    public const string RoleTable = "PositionalAbilitiesRoleTable";

    // A lineup slot of the active tactic (contexts TeamSelectionIndex and InPossession).
    public const string LineupPlayer = "PlayerFromTeamSelectionIndex";
    public const string LineupPlayerId = LineupPlayer + "." + GameConstants.UniqueId;
    public const string LineupPlayerName = LineupPlayer + "." + GameConstants.Name;
    public const string LineupRoleRating = LineupPlayer + ".CurrentRoleStarRange";
    public const string LineupRoleTable = LineupPlayer + "." + RoleTable;
    public const string PositionValue = "PositionValue";
    public const string RoleValue = "RoleValue";

    // On the tactic, for a role (contexts RoleMask, InPossession and Person).
    public const string KeyAttributesForRole = "KeyAttributesForRole";

    // A slot's tactical effectiveness (contexts TacticIndex and human), read in parts: as a whole it nests every player's.
    public const string SlotEffectiveness = "TacticalEffectivenessForTacticSlot";
    public const string SlotOverall = SlotEffectiveness + ".OverallEffectiveness";
    public const string SlotPlayerAbility = SlotEffectiveness + ".PlayerAbility";
    public const string SlotPlayerBuyIn = SlotEffectiveness + ".PlayerBuyIn";
    public const string SlotUnderstanding = SlotEffectiveness + ".TacticalUnderstanding";
    public const string SlotNextMatch = SlotEffectiveness + ".NextMatchInformation";
    public const string SlotStatusFlags = SlotEffectiveness + ".StatusFlags";

    public static readonly string[] SlotEffectivenessParts =
    [
        SlotOverall, SlotPlayerAbility, SlotPlayerBuyIn, SlotUnderstanding, SlotNextMatch, SlotStatusFlags
    ];

    // The players the game puts in a slot's lineup, in lineup order.
    public const string SlotPlayers = Tactic + "." + SlotEffectiveness + ".Players";
    public const string SlotPlayerId = "Player." + GameConstants.UniqueId;
    public const string SlotPlayerName = "Player." + GameConstants.Name;
    public const string SlotPlayerEffectiveness = "TacticalEffectiveness.OverallEffectiveness";

    // A slot's positions with role abbreviations (on a player, contexts HumanTeam and TacticIndex).
    public const string SlotPositions = "TacticalEffectivenessIPPositionsList";

    // Editing the active tactic. Values are the ids the tactics screen's own controls read and write; the plugin's
    // /set writes them, read without the human and team contexts as the screen's controls do.
    public const string MentalityValue = "Mentality";
    public const string MentalityDropdown = "MentalityDropdown";
    public const string MentalityOptions = "MentalityDropdownOptions";
    public const string Formation = "Formation";
    public const string FormationOptions = "AddFormationOptions";
    public const string PositionRole = "PositionRole";

    // A team instruction's option, by instruction (context TeamInstructionID, the instruction's id in /instructions).
    public const string TeamInstructionId = "TeamInstructionID";
    public const string SelectedTeamInstruction = "SelectedTeamInstructionOption";
    public const int TeamInstructionIdCount = 33;

    // A lineup slot's player instruction, by group (contexts PlayerInstructionGroupID, TeamSelectionIndex, InPossession).
    public const string PlayerInstructionGroupId = "PlayerInstructionGroupID";
    public const string SelectedPlayerInstruction = "SelectedPlayerInstruction";

    // True when the slot's role or position sets the group (its option then can't change).
    public const string PlayerInstructionLocked = "PlayerInstructionGroupIsLocked";
    public const int PlayerInstructionGroupCount = 17;

    // The groups a player takes out of possession (pressing, marking, tackling); the others are for when the team has
    // the ball. Each locks in the other phase.
    public static readonly string[] OutOfPossessionInstructionGroups = ["TriggerPress", "Marking", "Tackling", "MarkingTarget"];

    // Native event switching the active slot; its data is the slot, 0 to 2.
    public const string SwitchSlotEvent = "EventFmxSetTacticSlot";

    // The game's enums naming what the tactic holds as numbers (the plugin's /enums). Role values are the masks
    // lineup slots hold; the duty flags and "no role" aren't roles.
    public const string StylesEnum = "FM.UI.TacticalStyles";
    public const string MentalitiesEnum = "FM.UI.TacticMentalities";
    public const string FormationsEnum = "FM.UI.Formation";
    public const string InPossessionRolesEnum = "FM.UI.InPossessionRole";
    public const string OutOfPossessionRolesEnum = "FM.UI.OutOfPossessionRole";

    public static readonly string[] Enums =
    [
        StylesEnum, MentalitiesEnum, FormationsEnum, InPossessionRolesEnum, OutOfPossessionRolesEnum
    ];

    // Out-of-possession role names carry this prefix; keys drop it (OPTrackingCentreForward is TrackingCentreForward).
    public const string OutOfPossessionRolePrefix = "OP";
    public const string FormationPrefix = "Formation";
    public const string NoRole = "NoRole";
    public const string DutySuffix = "Duty";
    public const string PlayerInstructionPrefix = "PI_";
}
