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
        CurrentSlot, Slot1Name, Slot2Name, Slot3Name, Slot1Valid, Slot2Valid, Slot3Valid, Mentality, Style, StyleOptions,
        RecommendedStyles, .. TeamInstructions
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
}
