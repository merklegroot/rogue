using Raylib_cs;
using RogueLib.Constants;
namespace RogueLib;

public interface IRayConnection
{
    // Interface can be empty for now, we'll add methods as needed
}

public class RayConnection : IRayConnection
{
    // Empty implementation for now
}

public interface IRayConnectionFactory
{
    IRayConnection Connect();
}

public class RayConnectionFactory : IRayConnectionFactory
{
    public IRayConnection Connect()
    {
        // Disable the default ESC key behavior that closes the window
        Raylib.SetExitKey(KeyboardKey.Null);
        
        var windowWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var windowHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;

        Raylib.InitWindow(windowWidth, windowHeight, ScreenConstants.Title);
        
        Raylib.SetTargetFPS(ScreenConstants.TargetFps);
        
        return new RayConnection();
    }
} 