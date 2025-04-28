using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using System.Numerics;

namespace RogueLib.Presenters;

public interface IBestiaryPresenter
{
    void Draw(IRayConnection rayConnection);
}

public class BestiaryPresenter : IBestiaryPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const int TitleSize = 48;
    private const int EnemyNameSize = 32;
    private const int DescriptionSize = 20;
    private const int EntrySpacing = 180;
    private const int BorderPadding = 20;
    private const int BorderThickness = 2;

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
            new Vector2(centerX - titleSize.X/2 + 3, 40 + 3), TitleSize, 1, 
            new Color(30, 30, 30, 200));
        Raylib.DrawTextEx(rayConnection.MenuFont, title, 
            new Vector2(centerX - titleSize.X/2, 40), TitleSize, 1, Color.Gold);

        // Draw decorative line under title
        var lineY = 40 + TitleSize + 10;
        var lineWidth = titleSize.X - 20;
        Raylib.DrawRectangle((int)(centerX - lineWidth/2), (int)lineY, (int)lineWidth, 2, Color.Gold);

        // Start Y position for enemy entries
        var startY = lineY + 50;

        // Draw The Cedilla entry
        DrawEnemyEntry(rayConnection, "The Cedilla", new[]
        {
            "A basic enemy that wanders aimlessly",
            "- Moves randomly in 8 directions",
            "- Takes 1 hit to defeat",
            "- Deals 1 damage on contact",
            "- Common enemy, spawns frequently"
        }, startY, Color.White);

        // Draw The Spinner entry
        DrawEnemyEntry(rayConnection, "The Spinner", new[]
        {
            "A whirling menace that bounces around",
            "- Larger collision radius than other enemies",
            "- Can be knocked back with sword",
            "- Deals 1 damage on contact",
            "- Moves in straight lines until hit"
        }, startY + EntrySpacing, Color.Yellow);

        // Draw The Charger entry
        DrawEnemyEntry(rayConnection, "The Charger", new[]
        {
            "A powerful boss that hunts you down",
            "- Charges directly at the player",
            "- Takes 5 hits to defeat",
            "- Deals 2 damage on contact",
            "- Drops valuable gold when defeated",
            "- Appears after killing 20 regular enemies"
        }, startY + EntrySpacing * 2, Color.Red);

        // Draw return instruction
        _drawUtil.DrawText(rayConnection, "Press any key to return", 20, 
            screenHeight - 40, Color.White);
    }

    private void DrawEnemyEntry(IRayConnection rayConnection, string name, string[] description, float y, Color color)
    {
        var screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var centerX = screenWidth / 2;
        var nameSize = Raylib.MeasureTextEx(rayConnection.MenuFont, name, EnemyNameSize, 1);
        
        // Calculate the widest description line for border width
        float maxWidth = description.Max(line => Raylib.MeasureTextEx(rayConnection.MenuFont, line, DescriptionSize, 1).X);
        float borderWidth = Math.Max(nameSize.X, maxWidth) + (BorderPadding * 2);
        float borderHeight = nameSize.Y + (description.Length * (DescriptionSize + 5)) + (BorderPadding * 2);
        float borderX = centerX - borderWidth / 2;
        float borderY = y - BorderPadding;

        // Draw border
        Raylib.DrawRectangleLines(
            (int)borderX, (int)borderY,
            (int)borderWidth, (int)borderHeight,
            Color.Gold);

        // Draw name
        Raylib.DrawTextEx(rayConnection.MenuFont, name,
            new Vector2(centerX - nameSize.X/2, y),
            EnemyNameSize, 1, color);

        // Draw description lines
        float descY = y + nameSize.Y + 10;
        foreach (var line in description)
        {
            var descSize = Raylib.MeasureTextEx(rayConnection.MenuFont, line, DescriptionSize, 1);
            Raylib.DrawTextEx(rayConnection.MenuFont, line,
                new Vector2(centerX - descSize.X/2, descY),
                DescriptionSize, 1, Color.LightGray);
            descY += DescriptionSize + 5;
        }
    }
} 