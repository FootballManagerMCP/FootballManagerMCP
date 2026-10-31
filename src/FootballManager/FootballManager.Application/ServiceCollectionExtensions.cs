using FootballManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FootballManager.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISquadService, SquadService>();
        services.AddScoped<ITacticsService, TacticsService>();
        return services;
    }
}
