using FootballManager.Domain.Scouting;

namespace FootballManager.Infrastructure.Constants;

// The game's player search, on the user's manager with the search's type (ScoutingConstants.PlayerSearch). Its filter
// holds standing rules (age, position, ...) and the conditions added in its editor; quick filters and the interest
// setting sit on the search itself.
internal static class PlayerSearchConstants
{
    public const string Search = "HumanPersonSearch";
    public const string Filter = Search + ".PersonFilter";

    // On the search. A quick filter's state is read with its flag as the ID context.
    public const string QuickFilterEnabled = "IsContextQuickFilterEnabled";
    public const string QuickFilterContext = "ID";
    public const string InterestSelection = "PlayerInterestLevelSelection";
    public const string InterestTypes = "PlayerInterestLevelList";

    // "Modifier recherche (3)": how many conditions the filter holds, quick filters included.
    public const string EditFilterLabel = "EditFilterString";

    // On a rule.
    public const string Enabled = "Enabled";
    public const string Name = "Name";
    public const string Comparison = "SelectedCompareFlag";
    public const string Comparisons = "PossibleCompareFlags";
    public const string Value = "FilterValue";
    public const string MinValue = "MinValue";
    public const string MaxValue = "MaxValue";
    public const string ValueText = "FilterValueString";
    public const string Values = "Items";
    public const string MinValues = "MinItems";
    public const string MaxValues = "MaxItems";

    public static readonly string[] RuleState = [Enabled, Name, Comparison, Value, MinValue, MaxValue, ValueText];

    // Events on the search. Store keeps a copy of the filter's rules; restoring with true applies the filter as it now
    // stands and runs the search again, with false it puts the copy's rules back (the quick filters stay as they are).
    // A reset clears the rules and quick filters, not the interest.
    public const string StoreEvent = "EventFmxFilterStoreFilter";
    public const string RestoreEvent = "EventFmxFilterRestoreFilter";
    public const string ResetEvent = "EventFmxResetFilter";
    public const string ToggleQuickFilterEvent = "EventFmxFilterToggleQuickFilter";

    // The interest type's Data is its row of InterestTypes; the level's, the level's number.
    public const string InterestTypeEvent = "EventFmxPlayerInterestTypeSelected";
    public const string InterestLevelEvent = "EventFmxPlayerInterestLevelSelected";

    // Events on a rule, which the game only takes while the rule is bound (as its editor keeps it): sent with a hold.
    // Values and comparisons go as unsigned numbers (a value's ID, a comparison's flag), minimum and maximum as an item
    // of MinValues and MaxValues.
    public const string EnableEvent = "EventFmxSelectFilterEnabled";
    public const string ValueEvent = "EventFmxSelectFilterValue";
    public const string MinValueEvent = "EventFmxSelectFilterMinValue";
    public const string MaxValueEvent = "EventFmxSelectFilterMaxValue";
    public const string ComparisonEvent = "EventFmxSelectFilterCompareFlag";
    public const string HoldMs = "100";

    // Comparison flags.
    public const uint AtLeast = 4;
    public const uint AtMost = 8;
    public const uint Between = 16;
    public const uint NotBetween = 8192;

    // The grade list's "any".
    public const long AnyGrade = 0;

    public static string Rule(SearchRuleKind kind)
    {
        return $"{Filter}.{kind switch
        {
            SearchRuleKind.Age => "FilterRuleAge",
            SearchRuleKind.Position => "FilterRulePlayerPosition",
            SearchRuleKind.ScoutingGrade => "FilterRuleScoutingGrade",
            SearchRuleKind.Contract => "FilterRuleContract",
            SearchRuleKind.TransferStatus => "FilterRuleTransferStatus",
            SearchRuleKind.LoanStatus => "FilterRuleLoanStatus",
            SearchRuleKind.InPossessionRole => "FilterRuleIpRole",
            _ => "FilterRuleOopRole"
        }}";
    }

    // The game's flag for each quick filter.
    public static readonly IReadOnlyDictionary<SearchQuickFilter, int> QuickFilterFlags = new Dictionary<SearchQuickFilter, int>
    {
        [SearchQuickFilter.TransferListed] = 1,
        [SearchQuickFilter.ListedForLoan] = 2,
        [SearchQuickFilter.OutOfContract] = 4,
        [SearchQuickFilter.ContractExpiring] = 8,
        [SearchQuickFilter.HomeGrown] = 16,
        [SearchQuickFilter.Unhappy] = 128,
        [SearchQuickFilter.TwoFooted] = 256,
        [SearchQuickFilter.Goalkeepers] = 512,
        [SearchQuickFilter.Defenders] = 1024,
        [SearchQuickFilter.Midfielders] = 2048,
        [SearchQuickFilter.Attackers] = 4096,
        [SearchQuickFilter.HighKnowledge] = 8192,
        [SearchQuickFilter.NoWorkPermitNeeded] = 16384
    };

    // The interest type's ID in InterestTypes.
    public static readonly IReadOnlyDictionary<SearchInterestType, int> InterestTypeIds = new Dictionary<SearchInterestType, int>
    {
        [SearchInterestType.TransferOrLoan] = 0,
        [SearchInterestType.TransferOnly] = 1,
        [SearchInterestType.LoanOnly] = 2,
        [SearchInterestType.NotConsidered] = 3
    };

    // FM.UI.PlayerInterestLevel. A type that isn't selected, or isn't considered, has -100000: no level.
    public static readonly IReadOnlyDictionary<SearchInterestLevel, int> InterestLevels = new Dictionary<SearchInterestLevel, int>
    {
        [SearchInterestLevel.Doubtful] = 0,
        [SearchInterestLevel.SlightInterest] = 5000,
        [SearchInterestLevel.Interested] = 10000
    };

    // The position rule's comparisons.
    public static readonly IReadOnlyDictionary<PositionFamiliarity, uint> Familiarities = new Dictionary<PositionFamiliarity, uint>
    {
        [PositionFamiliarity.Natural] = 1,
        [PositionFamiliarity.Accomplished] = 16384,
        [PositionFamiliarity.Competent] = 32768,
        [PositionFamiliarity.Unable] = 65536,
        [PositionFamiliarity.Able] = 131072
    };
}
