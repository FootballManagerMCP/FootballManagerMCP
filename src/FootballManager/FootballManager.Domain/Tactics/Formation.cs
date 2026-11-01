namespace FootballManager.Domain.Tactics;

/// <summary>Names a shape the way FM does, counting the outfield lines from the back: "4-2-2-2", "5-3-2".</summary>
public static class Formation
{
    // Wing-backs count in the back line, as in FM's "5-3-2 WB".
    private static readonly PitchPosition[][] Lines =
    [
        [PitchPosition.SW, PitchPosition.DR, PitchPosition.DL, PitchPosition.DC, PitchPosition.WBR, PitchPosition.WBL],
        [PitchPosition.DM],
        [PitchPosition.MR, PitchPosition.ML, PitchPosition.MC],
        [PitchPosition.AMR, PitchPosition.AML, PitchPosition.AMC],
        [PitchPosition.ST]
    ];

    /// <summary>The shape of the given positions; null without any outfield player.</summary>
    public static string? Describe(IEnumerable<TacticalPosition> positions)
    {
        var outfield = positions.Select(p => p.Position).Where(p => p != PitchPosition.GK).ToList();

        var counts = Lines
            .Select(line => outfield.Count(line.Contains))
            .Where(count => count > 0)
            .ToList();

        return counts.Count == 0 ? null : string.Join('-', counts);
    }
}
