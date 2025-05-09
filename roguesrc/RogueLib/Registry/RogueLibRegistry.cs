using Microsoft.Extensions.DependencyInjection;
using RogueLib.Presenters;
using RogueLib.Handlers;
using RogueLib.Utils;

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
            .AddScoped<IGameApp, GameApp>()
            .AddScoped<IDrawUtil, DrawUtil>()
            .AddScoped<IDrawEnemyUtil, DrawEnemyUtil>()
            .AddScoped<IRayConnectionFactory, RayConnectionFactory>()
            .AddScoped<IRayLoader, RayLoader>()
            .AddScoped<IChunkPresenter, ChunkPresenter>()
            .AddScoped<IDebugPanelPresenter, DebugPanelPresenter>()
            .AddScoped<IPlayerPresenter, PlayerPresenter>()
            .AddScoped<ISpawnEnemyHandler, SpawnEnemyHandler>()
            .AddScoped<IBannerPresenter, BannerPresenter>()
            .AddScoped<IMenuPresenter, MenuPresenter>()
            .AddScoped<ICharacterSetPresenter, CharacterSetPresenter>()
            .AddScoped<ISwordPresenter, SwordPresenter>()
            .AddScoped<IInstructionsPresenter, InstructionsPresenter>()
            .AddScoped<IGoldCounterPresenter, GoldCounterPresenter>()
            .AddScoped<IFlyingGoldPresenter, FlyingGoldPresenter>()
            .AddScoped<IUpdateEnemiesHandler, UpdateEnemiesHandler>()
            .AddScoped<ICooldownIndicatorPresenter, CooldownIndicatorPresenter>()
            .AddScoped<IMenuInputHandler, MenuInputHandler>()
            .AddScoped<IEnemyPresenter, EnemyPresenter>()
            .AddScoped<IExplosionPresenter, ExplosionPresenter>()
            .AddScoped<IMapPresenter, MapPresenter>()
            .AddScoped<IMapUtil, MapUtil>()
            .AddScoped<IScreenUtil, ScreenUtil>()
            .AddScoped<IChargerPresenter, ChargerPresenter>()
            .AddScoped<IGoldPickupPresenter, GoldPickupPresenter>()
            .AddScoped<IBestiaryPresenter, BestiaryPresenter>()
            .AddScoped<IInitializeShopHandler, InitializeShopHandler>()
            .AddScoped<IShopInputHandler, ShopInputHandler>()
            .AddScoped<IBestiaryInputHandler, BestiaryInputHandler>()
            .AddScoped<IDevTestingInputHandler, DevTestingInputHandler>();
}