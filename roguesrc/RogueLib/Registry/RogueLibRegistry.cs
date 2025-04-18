using Microsoft.Extensions.DependencyInjection;
using Rogue;

namespace RogueLib.Registry;

public static class RogueLibRegistry
{
    public static IServiceCollection RegisterRogueLib(this IServiceCollection services) =>
        services
            .AddScoped<IResourceReader, ResourceReader>()
            .AddScoped<IRayLoader, RayLoader>();
}