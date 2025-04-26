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
    private const int BorderPadding = 10;
    private const int BorderThickness = 2;
    private readonly IDrawUtil _drawUtil;
    private List<(Rectangle bounds, Action<GameState> action, string text)> _menuItems = new();

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
        
        // Find the widest menu item text
        var menuTexts = new[] { "Start (A)dventure", "View (C)haracter Set", "(T)oggle CRT Effect", "e(X)it Game" };
        var maxWidth = menuTexts.Max(text => Raylib.MeasureTextEx(rayConnection.MenuFont, text, ScreenConstants.MenuFontSize, 1).X);
        var menuItemWidth = (int)maxWidth + (BorderPadding * 2);
        
        // Draw menu options with colored hotkeys and store their clickable areas
        var adventureText = "Start (A)dventure";
        var adventureSize = Raylib.MeasureTextEx(rayConnection.MenuFont, adventureText, ScreenConstants.MenuFontSize, 1);
        var adventureX = centerX - menuItemWidth / 2;
        _drawUtil.DrawColoredHotkeyText(rayConnection, adventureText, 
            adventureX + BorderPadding, menuStartY);
        Raylib.DrawRectangleLines((int)adventureX, (int)(menuStartY - BorderPadding), 
            menuItemWidth, (int)(adventureSize.Y + (BorderPadding * 2)), Color.Gold);
        _menuItems.Add((new Rectangle(adventureX, menuStartY - BorderPadding, menuItemWidth, adventureSize.Y + (BorderPadding * 2)), 
            state => state.CurrentScreen = GameScreenEnum.Adventure, adventureText));

        var characterText = "View (C)haracter Set";
        var characterSize = Raylib.MeasureTextEx(rayConnection.MenuFont, characterText, ScreenConstants.MenuFontSize, 1);
        var characterX = centerX - menuItemWidth / 2;
        _drawUtil.DrawColoredHotkeyText(rayConnection, characterText, 
            characterX + BorderPadding, menuStartY + MenuSpacing);
        Raylib.DrawRectangleLines((int)characterX, (int)(menuStartY + MenuSpacing - BorderPadding), 
            menuItemWidth, (int)(characterSize.Y + (BorderPadding * 2)), Color.Gold);
        _menuItems.Add((new Rectangle(characterX, menuStartY + MenuSpacing - BorderPadding, menuItemWidth, characterSize.Y + (BorderPadding * 2)), 
            state => state.CurrentScreen = GameScreenEnum.CharacterSet, characterText));

        var crtText = "(T)oggle CRT Effect";
        var crtSize = Raylib.MeasureTextEx(rayConnection.MenuFont, crtText, ScreenConstants.MenuFontSize, 1);
        var crtX = centerX - menuItemWidth / 2;
        _drawUtil.DrawColoredHotkeyText(rayConnection, crtText, 
            crtX + BorderPadding, menuStartY + MenuSpacing * 2);
        Raylib.DrawRectangleLines((int)crtX, (int)(menuStartY + MenuSpacing * 2 - BorderPadding), 
            menuItemWidth, (int)(crtSize.Y + (BorderPadding * 2)), Color.Gold);
        _menuItems.Add((new Rectangle(crtX, menuStartY + MenuSpacing * 2 - BorderPadding, menuItemWidth, crtSize.Y + (BorderPadding * 2)), 
            state => state.ShouldEnableCrtEffect = !state.ShouldEnableCrtEffect, crtText));

        var exitText = "e(X)it Game";
        var exitSize = Raylib.MeasureTextEx(rayConnection.MenuFont, exitText, ScreenConstants.MenuFontSize, 1);
        var exitX = centerX - menuItemWidth / 2;
        _drawUtil.DrawColoredHotkeyText(rayConnection, exitText, 
            exitX + BorderPadding, menuStartY + MenuSpacing * 3);
        Raylib.DrawRectangleLines((int)exitX, (int)(menuStartY + MenuSpacing * 3 - BorderPadding), 
            menuItemWidth, (int)(exitSize.Y + (BorderPadding * 2)), Color.Gold);
        _menuItems.Add((new Rectangle(exitX, menuStartY + MenuSpacing * 3 - BorderPadding, menuItemWidth, exitSize.Y + (BorderPadding * 2)), 
            state => Raylib.CloseWindow(), exitText));
        
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
        foreach (var (bounds, action, _) in _menuItems)
        {
            if (Raylib.CheckCollisionPointRec(mousePosition, bounds))
            {
                action(state);
                break;
            }
        }
    }
} 