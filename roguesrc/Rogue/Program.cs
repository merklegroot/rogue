using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rogue;
using Rogue.Registry;

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
                services.RegisterGame();
            });
}