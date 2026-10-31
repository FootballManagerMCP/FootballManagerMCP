using FootballManager.Api.Responses;
using FootballManager.Domain.Entities;
using FootballManager.Domain.Tactics;
using Mapster;

namespace FootballManager.Api.Mappings;

public class MappingConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Squad, SquadResponse>();

        config.NewConfig<Player, PlayerResponse>()
            .Map(dest => dest.Attributes, src => GroupAttributes(src.Attributes));

        // Scores and positions as FM writes them: "17" or "15-17", "DCL".
        config.NewConfig<Rating, string>()
            .MapWith(src => src.Display);

        config.NewConfig<TacticalPosition, string>()
            .MapWith(src => src.Code);

        config.NewConfig<TacticalEffectiveness, TacticalEffectivenessResponse>()
            .Map(dest => dest.Warnings, src => src.Warnings.Count > 0 ? src.Warnings : null);
    }

    // Grouped by category with FM's own notation, which is far more compact than one object per attribute when a
    // whole squad comes back.
    public static Dictionary<string, Dictionary<string, string?>>? GroupAttributes(List<PlayerAttribute> attributes)
    {
        if (attributes.Count == 0)
        {
            return null;
        }

        return attributes
            .GroupBy(a => a.Category)
            .ToDictionary(
                group => group.Key.ToString().ToLowerInvariant(),
                group => group.ToDictionary(a => a.Name, a => a.Display));
    }
}
