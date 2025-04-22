using Microsoft.Extensions.DependencyInjection;

namespace RogueLib.Registry;

public static class RogueLibRegistry
{
    public static IServiceCollection RegisterRogueLib(this IServiceCollection services) =>
        services
            .AddScoped<IResourceReader, ResourceReader>()
            .AddScoped<IRayLoader, RayLoader>()
            .AddScoped<IScreenPresenter, ScreenPresenter>()
            .AddScoped<IHealthBarPresenter, HealthBarPresenter>()
            .AddScoped<IGame, Game>()
            .AddScoped<IScreenDrawer, ScreenDrawer>()
            .AddScoped<IRayConnectionFactory, RayConnectionFactory>()
            .AddScoped<IRayLoader, RayLoader>();
}