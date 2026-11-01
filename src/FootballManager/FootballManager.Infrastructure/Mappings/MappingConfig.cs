using System.Globalization;
using FootballManager.Domain.Matches;
using FootballManager.Domain.Players;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Effectiveness;
using FootballManager.Infrastructure.Constants;
using FootballManager.Infrastructure.Dtos.Matches;
using FootballManager.Infrastructure.Dtos.Players;
using FootballManager.Infrastructure.Dtos.Tactics;
using FootballManager.Infrastructure.Dtos.Tactics.Effectiveness;
using Mapster;

namespace FootballManager.Infrastructure.Mappings;

public class MappingConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<BridgePlayerDto, Player>()
            .Map(dest => dest.Id, src => src.Id ?? 0)
            .Map(dest => dest.Name, src => src.Name ?? string.Empty)
            .Map(dest => dest.Condition, src => src.Condition >= 0 ? src.Condition : null)
            .Map(dest => dest.IsInjured, src => src.IsInjured ?? false)
            // The game sends 0 for a player without appearances.
            .Map(dest => dest.AverageRating, src => src.AverageRating > 0 ? (double?)Math.Round(src.AverageRating.Value, 2) : null)
            .Map(dest => dest.Attributes, src => AttributeBoundsMapper.ToAttributes(src.AttributeBounds))
            // Appearances always comes back when the season's numbers were asked for.
            .Map(dest => dest.SeasonStats, src => src.Appearances == null ? null : src);

        config.NewConfig<BridgePlayerDto, PlayerSeasonStats>();

        RegisterTactics(config);
        RegisterMatches(config);
    }

    private static void RegisterMatches(TypeAdapterConfig config)
    {
        // A match's score and numbers only mean something once it's played: before that the game sends -1 and zeros.
        // Which side is the user's, and the shape they played, are set by the client.
        config.NewConfig<BridgeFixtureDto, MatchSummary>()
            .Map(dest => dest.Date, src => DateOnly.ParseExact(src.Date!, MatchConstants.DateFormat, CultureInfo.InvariantCulture))
            .Map(dest => dest.IsFriendly, src => src.IsFriendly == true)
            .Map(dest => dest.IsPlayed, src => src.HasResult == true)
            .Map(dest => dest.Home, src => new MatchSide
            {
                TeamId = src.HomeTeamId ?? 0,
                TeamName = src.HomeTeamName ?? string.Empty,
                Goals = src.HasResult == true ? src.HomeScore : null,
                ExpectedGoals = src.HasResult == true ? src.HomeExpectedGoals : null,
                Possession = src.HasResult == true ? src.HomePossession : null,
                Shots = src.HasResult == true ? src.HomeShots : null,
                ShotsOnTarget = src.HasResult == true ? src.HomeShotsOnTarget : null,
                LeaguePosition = string.IsNullOrEmpty(src.HomeLeaguePosition) ? null : src.HomeLeaguePosition
            })
            .Map(dest => dest.Away, src => new MatchSide
            {
                TeamId = src.AwayTeamId ?? 0,
                TeamName = src.AwayTeamName ?? string.Empty,
                Goals = src.HasResult == true ? src.AwayScore : null,
                ExpectedGoals = src.HasResult == true ? src.AwayExpectedGoals : null,
                Possession = src.HasResult == true ? src.AwayPossession : null,
                Shots = src.HasResult == true ? src.AwayShots : null,
                ShotsOnTarget = src.HasResult == true ? src.AwayShotsOnTarget : null,
                LeaguePosition = string.IsNullOrEmpty(src.AwayLeaguePosition) ? null : src.AwayLeaguePosition
            })
            .Ignore(dest => dest.TeamId, dest => dest.Formation!);

        config.NewConfig<BridgeTeamMatchStatsDto, TeamMatchStats>()
            .Map(dest => dest.Possession, src => src.TotalPossession)
            .Map(dest => dest.Shots, src => src.TotalShots)
            .Map(dest => dest.ShotsHitWoodwork, src => src.ShotHitWoodwork)
            .Map(dest => dest.Crosses, src => src.CrossAttempts)
            .Map(dest => dest.Passes, src => src.PassAttempts)
            .Map(dest => dest.PassesPerDefensiveAction, src => src.OppositionPassesPerDefensiveAction)
            .Map(dest => dest.Tackles, src => src.TackleAttempts)
            .Map(dest => dest.Headers, src => src.HeaderAttempts)
            .Map(dest => dest.Fouls, src => src.FoulsMade)
            .Map(dest => dest.AverageRating, src => src.AverageRatingFloat);

        config.NewConfig<BridgePlayerMatchStatsDto, PlayerMatchStats>()
            .Map(dest => dest.Shots, src => src.TotalShots)
            .Map(dest => dest.Passes, src => src.PassAttempts)
            .Map(dest => dest.Dribbles, src => src.DribblesMade)
            .Map(dest => dest.Crosses, src => src.CrossAttempts)
            .Map(dest => dest.Tackles, src => src.TackleAttempts)
            .Map(dest => dest.TacklesWon, src => src.TackleCompleted)
            .Map(dest => dest.AerialDuels, src => src.AerialDuelsAttempted)
            .Map(dest => dest.AerialDuelsWon, src => src.AerialDuelsCompleted)
            .Map(dest => dest.Saves, src => src.TotalSaves)
            .Map(dest => dest.DistanceRun, src => src.DistanceRan);
    }

    private static void RegisterTactics(TypeAdapterConfig config)
    {
        // The game sends -1 for a score it doesn't know (e.g. a role rating for a player it can't judge).
        config.NewConfig<BridgeRatingDto, Rating?>()
            .MapWith(src => src.MaxValue >= 0
                ? new Rating(src.MinValue >= 0 ? src.MinValue!.Value : src.MaxValue!.Value, src.MaxValue!.Value)
                : null);

        config.NewConfig<BridgeAdviceDto, EffectivenessAdvice>()
            .Map(dest => dest.Text, src => src.Text ?? string.Empty)
            .Map(dest => dest.IsPositive, src => src.IsPro ?? false);

        config.NewConfig<BridgeSlotEffectivenessDto, TacticalEffectiveness>()
            .Map(dest => dest.Warnings, src => src.StatusFlags.Where(flag => flag.Value).Select(flag => flag.Key).ToList());

        config.NewConfig<BridgePlayerAbilityDto, PlayerAbilityScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.RoleSuitability, src => src.RoleSuitabilityScore)
            .Map(dest => dest.Capability, src => src.CapabilityScore)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        config.NewConfig<BridgePlayerBuyInDto, PlayerBuyInScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.ManagerSupport, src => src.ManagerSupportScore)
            .Map(dest => dest.Relationships, src => src.TeammateAndStaffRelationshipsScore)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        config.NewConfig<BridgeTacticalUnderstandingDto, TacticalUnderstandingScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.Comprehension, src => src.TacticalComprehension)
            .Map(dest => dest.ComprehensionAdvice, src => src.AdviceForTacticalComprehension)
            .Map(dest => dest.AreasAdvice, src => src.AdviceForTacticalAreas)
            .Map(dest => dest.Complexity, src => src.TacticalComplexity);

        config.NewConfig<BridgeTacticalAreaDto, TacticalArea>()
            .Map(dest => dest.Understanding, src => src.OverallEffectiveness);

        config.NewConfig<BridgeTacticalComplexityDto, TacticalComplexity>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.TeamInstructions, src => src.TeamInstructionComplexity)
            .Map(dest => dest.PlayerInstructions, src => src.PlayerInstructionComplexity)
            .Map(dest => dest.OppositionInstructions, src => src.OppositionInstructionComplexity);

        config.NewConfig<BridgeNextMatchDto, NextMatchScores>()
            .Map(dest => dest.Overall, src => src.OverallEffectiveness)
            .Map(dest => dest.Advice, src => src.AdviceForComponent);

        config.NewConfig<BridgeRoleRowDto, RoleRating>()
            .Map(dest => dest.Role, src => src.TacticsRoleName ?? string.Empty)
            .Map(dest => dest.Rating, src => src.CurrentAbilityStars);

        // Index and position label come from the slot's position list, not the player.
        config.NewConfig<BridgeSlotPlayerDto, SlotPosition>()
            .Map(dest => dest.PlayerId, src => src.Id)
            .Map(dest => dest.PlayerName, src => src.Name)
            .Ignore(dest => dest.Index, dest => dest.InPossession!);
    }
}
