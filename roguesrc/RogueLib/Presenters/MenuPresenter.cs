using Raylib_cs;
using System.Numerics;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IMenuPresenter
{
    void Draw(IRayConnection rayConnection);
}

public class MenuPresenter : IMenuPresenter
{
    private const int TitleSize = 48;
    private const int MenuSpacing = 60;
    private const int LineHeight = 2;
    private const int VersionYOffset = 40;
    private const int DecorativeElementYOffset = 10;
    private readonly IScreenDrawerUtil _screenDrawerUtil;

    public MenuPresenter(IScreenDrawerUtil screenDrawerUtil)
    {
        _screenDrawerUtil = screenDrawerUtil;
    }

    public void Draw(IRayConnection rayConnection)
    {
        // Calculate screen dimensions
        var screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var screenHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
        
        // Calculate title size and position
        var titleSize = Raylib.MeasureTextEx(rayConnection.MenuFont, ScreenConstants.Title, TitleSize, 1);
        var centerX = screenWidth / 2;
        
        // Draw title with shadow effect
        Raylib.DrawTextEx(rayConnection.MenuFont, ScreenConstants.Title, 
            new Vector2(centerX - titleSize.X/2 + 3, 40 + 3), TitleSize, 1, 
            new Color(30, 30, 30, 200));
        Raylib.DrawTextEx(rayConnection.MenuFont, ScreenConstants.Title, 
            new Vector2(centerX - titleSize.X/2, 40), TitleSize, 1, Color.Gold);
        
        // Draw decorative line under title
        var lineY = 40 + TitleSize + 10;
        var lineWidth = titleSize.X - 20;
        Raylib.DrawRectangle((int)(centerX - lineWidth/2), (int)lineY, (int)lineWidth, LineHeight, Color.Gold);
        
        // Draw menu options
        var menuStartY = lineY + 30;
        
        // Draw menu options with colored hotkeys
        _screenDrawerUtil.DrawColoredHotkeyText(rayConnection, "Start (A)dventure", 
            centerX - 120, menuStartY);
        _screenDrawerUtil.DrawColoredHotkeyText(rayConnection, "View (C)haracter Set", 
            centerX - 120, menuStartY + MenuSpacing);
        _screenDrawerUtil.DrawColoredHotkeyText(rayConnection, "(T)oggle CRT Effect", 
            centerX - 120, menuStartY + MenuSpacing * 2);
        _screenDrawerUtil.DrawColoredHotkeyText(rayConnection, "e(X)it Game", 
            centerX - 120, menuStartY + MenuSpacing * 3);
        
        // Draw version number
        var version = "v0.1 Alpha";
        _screenDrawerUtil.DrawText(rayConnection, version, 20, 
            screenHeight - VersionYOffset, new Color(150, 150, 150, 200));
        
        // Draw decorative element
        _screenDrawerUtil.DrawCharacter(rayConnection, 2, 
            centerX - 10, menuStartY + MenuSpacing * 4 + DecorativeElementYOffset, Color.White);
    }
} 