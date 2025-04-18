using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            });
}