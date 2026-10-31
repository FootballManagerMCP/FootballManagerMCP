using FootballManager.Application.Interfaces;
using FootballManager.Infrastructure.Clients;
using FootballManager.Infrastructure.Mappings;
using FootballManager.Infrastructure.Settings;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace FootballManager.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        InfrastructureSettings settings)
    {
        services.AddSingleton(settings);

        services.AddHttpClient(GameBridge.HttpClientName, client =>
        {
            client.BaseAddress = new Uri(settings.GameBridgeBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);

            if (!string.IsNullOrWhiteSpace(settings.GameBridgeHostHeader))
            {
                client.DefaultRequestHeaders.Host = settings.GameBridgeHostHeader;
            }
        });

        services.AddScoped<GameBridge>();
        services.AddScoped<ISquadGameClient, SquadBridgeClient>();
        services.AddScoped<ITacticsGameClient, TacticsBridgeClient>();

        var mappingConfig = new MappingConfig();
        mappingConfig.Register(TypeAdapterConfig.GlobalSettings);

        return services;
    }
}
