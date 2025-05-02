using RogueLib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RogueLib.Registry;
using Xunit.Abstractions;
namespace RogueLibTests;

public class RayLoaderTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void Should_load_the_minotaur()
    {
        var host = CreateHost();
        var rayLoader = host.Services.GetRequiredService<IRayLoader>();

        var minotaur = rayLoader.LoadMinotaur();
        Assert.NotNull(minotaur);

        // Display each frame of the minotaur animation nicely
        outputHelper.WriteLine($"Found {minotaur.Count} minotaur frames:");
        
        for (int i = 0; i < minotaur.Count; i++)
        {
            outputHelper.WriteLine($"{Environment.NewLine}Frame {i + 1}:");
            outputHelper.WriteLine("[BEGIN]");
            foreach (var line in minotaur[i])
            {
                outputHelper.WriteLine(line);
            }
            outputHelper.WriteLine("[END]");
        }
    }

    private IHost CreateHost()
    {
        var host = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.RegisterRogueLib();
            })
            .Build();
        return host;
    }
}