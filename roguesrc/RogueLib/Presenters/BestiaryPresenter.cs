using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IBestiaryPresenter
{
    void Draw(IRayConnection rayConnection);
}

public class BestiaryPresenter : IBestiaryPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const int TitleSize = 48;

    public BestiaryPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection)
    {
        // Calculate screen dimensions
        var screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var screenHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
        
        // Draw title
        var title = "BESTIARY";
        var titleSize = Raylib.MeasureTextEx(rayConnection.MenuFont, title, TitleSize, 1);
        var centerX = screenWidth / 2;
        
        // Draw title with shadow effect
        Raylib.DrawTextEx(rayConnection.MenuFont, title, 
            new System.Numerics.Vector2(centerX - titleSize.X/2 + 3, 40 + 3), TitleSize, 1, 
            new Color(30, 30, 30, 200));
        Raylib.DrawTextEx(rayConnection.MenuFont, title, 
            new System.Numerics.Vector2(centerX - titleSize.X/2, 40), TitleSize, 1, Color.Gold);

        // Draw return instruction
        _drawUtil.DrawText(rayConnection, "Press any key to return", 20, 
            screenHeight - 40, Color.White);
    }
} 