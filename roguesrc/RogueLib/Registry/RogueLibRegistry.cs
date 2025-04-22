using Microsoft.Extensions.DependencyInjection;
using RogueLib.Presenters;

namespace RogueLib.Registry;

public static class RogueLibRegistry
{
    public static IServiceCollection RegisterRogueLib(this IServiceCollection services) =>
        services
            .AddScoped<IResourceReader, ResourceReader>()
            .AddScoped<IRayLoader, RayLoader>()
            .AddScoped<IScreenPresenter, ScreenPresenter>()
            .AddScoped<IHealthBarPresenter, HealthBarPresenter>()
            .AddScoped<IShopPresenter, ShopPresenter>()
            .AddScoped<IGameApp, GameAppApp>()
            .AddScoped<IScreenDrawer, ScreenDrawer>()
            .AddScoped<IRayConnectionFactory, RayConnectionFactory>()
            .AddScoped<IRayLoader, RayLoader>()
            .AddScoped<IChunkPresenter, ChunkPresenter>()
            .AddScoped<DebugPanelPresenter>();
}