using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rogue;

class Program
{
    static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        // Resolve and run the game
        var game = host.Services.GetRequiredService<IGame>();
        game.Run();
    }
    
    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                // Register services
                services.AddScoped<IScreenPresenter, ScreenPresenter>();
                services.AddScoped<IGame, Game>();
                services.AddScoped<IRayLoader, RayLoader>();
                services.AddScoped<IResourceReader, ResourceReader>();
            });
}