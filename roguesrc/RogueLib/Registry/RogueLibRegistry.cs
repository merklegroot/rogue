using Microsoft.Extensions.DependencyInjection;
using RogueLib.Presenters;
using RogueLib.Handlers;
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
            .AddScoped<IScreenDrawerUtil, ScreenDrawerUtilUtil>()
            .AddScoped<IRayConnectionFactory, RayConnectionFactory>()
            .AddScoped<IRayLoader, RayLoader>()
            .AddScoped<IChunkPresenter, ChunkPresenter>()
            .AddScoped<IDebugPanelPresenter, DebugPanelPresenter>()
            .AddScoped<IPlayerPresenter, PlayerPresenter>()
            .AddScoped<ISpawnEnemyHandler, SpawnEnemyHandler>()
            .AddScoped<IBannerPresenter, BannerPresenter>()
            .AddScoped<IMenuPresenter, MenuPresenter>();
}