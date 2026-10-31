namespace FootballManager.Domain.Players;

/// <summary>
/// The player attributes FM27 tracks. Hidden ones are part of the game's challenge: the game itself masks them unless
/// the In-Game Editor is active, and they should only be revealed when the user asks for it.
/// </summary>
public static class PlayerAttributes
{
    public static IReadOnlyList<AttributeDefinition> All { get; } =
    [
        new("Corners", AttributeCategory.Technical),
        new("Crossing", AttributeCategory.Technical),
        new("Dribbling", AttributeCategory.Technical),
        new("Finishing", AttributeCategory.Technical),
        new("FirstTouch", AttributeCategory.Technical),
        new("FreeKicks", AttributeCategory.Technical),
        new("Heading", AttributeCategory.Technical),
        new("LongShots", AttributeCategory.Technical),
        new("LongThrows", AttributeCategory.Technical),
        new("Marking", AttributeCategory.Technical),
        new("Passing", AttributeCategory.Technical),
        new("PenaltyTaking", AttributeCategory.Technical),
        new("Tackling", AttributeCategory.Technical),
        new("Technique", AttributeCategory.Technical),

        new("Aggression", AttributeCategory.Mental),
        new("Anticipation", AttributeCategory.Mental),
        new("Bravery", AttributeCategory.Mental),
        new("Composure", AttributeCategory.Mental),
        new("Concentration", AttributeCategory.Mental),
        new("Decisions", AttributeCategory.Mental),
        new("Determination", AttributeCategory.Mental),
        new("Flair", AttributeCategory.Mental),
        new("Leadership", AttributeCategory.Mental),
        new("Movement", AttributeCategory.Mental),
        new("Positioning", AttributeCategory.Mental),
        new("Teamwork", AttributeCategory.Mental),
        new("Vision", AttributeCategory.Mental),
        new("WorkRate", AttributeCategory.Mental),

        new("Acceleration", AttributeCategory.Physical),
        new("Agility", AttributeCategory.Physical),
        new("Balance", AttributeCategory.Physical),
        new("JumpingReach", AttributeCategory.Physical),
        new("NaturalFitness", AttributeCategory.Physical),
        new("Pace", AttributeCategory.Physical),
        new("Stamina", AttributeCategory.Physical),
        new("Strength", AttributeCategory.Physical),

        new("AerialReach", AttributeCategory.Goalkeeping),
        new("CommandOfArea", AttributeCategory.Goalkeeping),
        new("Communication", AttributeCategory.Goalkeeping),
        new("Eccentricity", AttributeCategory.Goalkeeping),
        new("Handling", AttributeCategory.Goalkeeping),
        new("Kicking", AttributeCategory.Goalkeeping),
        new("OneOnOnes", AttributeCategory.Goalkeeping),
        new("Reflexes", AttributeCategory.Goalkeeping),
        new("RushingOut", AttributeCategory.Goalkeeping),
        new("TendencyToPunch", AttributeCategory.Goalkeeping),
        new("Throwing", AttributeCategory.Goalkeeping),

        new("Adaptability", AttributeCategory.Hidden),
        new("Ambition", AttributeCategory.Hidden),
        new("Controversy", AttributeCategory.Hidden),
        new("Loyalty", AttributeCategory.Hidden),
        new("Pressure", AttributeCategory.Hidden),
        new("Professionalism", AttributeCategory.Hidden),
        new("Sportsmanship", AttributeCategory.Hidden),
        new("Temperament", AttributeCategory.Hidden)
    ];

    public static IReadOnlyList<AttributeDefinition> Visible { get; } = All.Where(a => !a.IsHidden).ToList();

    public static IReadOnlyList<AttributeDefinition> Hidden { get; } = All.Where(a => a.IsHidden).ToList();
}
