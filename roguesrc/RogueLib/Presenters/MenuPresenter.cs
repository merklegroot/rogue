using Raylib_cs;
using System.Numerics;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IMenuPresenter
{
    void Draw(IRayConnection rayConnection);
    void HandleMouseClick(Vector2 mousePosition, GameState state);
}

public class MenuPresenter : IMenuPresenter
{
    private const int TitleSize = 48;
    private const int MenuSpacing = 60;
    private const int LineHeight = 2;
    private const int VersionYOffset = 40;
    private const int DecorativeElementYOffset = 10;
    private readonly IDrawUtil _drawUtil;
    private List<(Rectangle bounds, Action<GameState> action)> _menuItems = new();

    public MenuPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection)
    {
        // Clear previous menu items
        _menuItems.Clear();

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
        
        // Draw menu options with colored hotkeys and store their clickable areas
        var adventureText = "Start (A)dventure";
        var adventureSize = Raylib.MeasureTextEx(rayConnection.MenuFont, adventureText, ScreenConstants.MenuFontSize, 1);
        _drawUtil.DrawColoredHotkeyText(rayConnection, adventureText, 
            centerX - 120, menuStartY);
        _menuItems.Add((new Rectangle(centerX - 120, menuStartY, adventureSize.X, adventureSize.Y), 
            state => state.CurrentScreen = GameScreenEnum.Adventure));

        var characterText = "View (C)haracter Set";
        var characterSize = Raylib.MeasureTextEx(rayConnection.MenuFont, characterText, ScreenConstants.MenuFontSize, 1);
        _drawUtil.DrawColoredHotkeyText(rayConnection, characterText, 
            centerX - 120, menuStartY + MenuSpacing);
        _menuItems.Add((new Rectangle(centerX - 120, menuStartY + MenuSpacing, characterSize.X, characterSize.Y), 
            state => state.CurrentScreen = GameScreenEnum.CharacterSet));

        var crtText = "(T)oggle CRT Effect";
        var crtSize = Raylib.MeasureTextEx(rayConnection.MenuFont, crtText, ScreenConstants.MenuFontSize, 1);
        _drawUtil.DrawColoredHotkeyText(rayConnection, crtText, 
            centerX - 120, menuStartY + MenuSpacing * 2);
        _menuItems.Add((new Rectangle(centerX - 120, menuStartY + MenuSpacing * 2, crtSize.X, crtSize.Y), 
            state => state.ShouldEnableCrtEffect = !state.ShouldEnableCrtEffect));

        var exitText = "e(X)it Game";
        var exitSize = Raylib.MeasureTextEx(rayConnection.MenuFont, exitText, ScreenConstants.MenuFontSize, 1);
        _drawUtil.DrawColoredHotkeyText(rayConnection, exitText, 
            centerX - 120, menuStartY + MenuSpacing * 3);
        _menuItems.Add((new Rectangle(centerX - 120, menuStartY + MenuSpacing * 3, exitSize.X, exitSize.Y), 
            state => Raylib.CloseWindow()));
        
        // Draw version number
        var version = "v0.1 Alpha";
        _drawUtil.DrawText(rayConnection, version, 20, 
            screenHeight - VersionYOffset, new Color(150, 150, 150, 200));
        
        // Draw decorative element
        _drawUtil.DrawCharacter(rayConnection, 2, 
            centerX - 10, menuStartY + MenuSpacing * 4 + DecorativeElementYOffset, Color.White);
    }

    public void HandleMouseClick(Vector2 mousePosition, GameState state)
    {
        foreach (var (bounds, action) in _menuItems)
        {
            if (Raylib.CheckCollisionPointRec(mousePosition, bounds))
            {
                action(state);
                break;
            }
        }
    }
} 