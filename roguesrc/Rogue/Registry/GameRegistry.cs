using Microsoft.Extensions.DependencyInjection;
using RogueLib.Registry;

namespace Rogue.Registry;

public static class GameRegistry
{
    public static IServiceCollection RegisterGame(this IServiceCollection services) =>
        services
            .RegisterRogueLib();
}