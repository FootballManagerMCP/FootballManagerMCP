using FootballManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FootballManager.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISquadService, SquadService>();
        services.AddScoped<ITacticsService, TacticsService>();
        services.AddScoped<ITacticEditingService, TacticEditingService>();
        services.AddScoped<IMatchesService, MatchesService>();
        services.AddScoped<ITeamsService, TeamsService>();
        services.AddScoped<IScoutingService, ScoutingService>();
        services.AddScoped<IScoutingEditingService, ScoutingEditingService>();
        services.AddScoped<IPlayerSearchService, PlayerSearchService>();
        services.AddScoped<IInboxService, InboxService>();
        services.AddScoped<IDatabaseSearchService, DatabaseSearchService>();
        services.AddScoped<ITrialsService, TrialsService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<ICompetitionsService, CompetitionsService>();
        return services;
    }
}
