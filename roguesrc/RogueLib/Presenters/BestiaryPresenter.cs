using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using System.Numerics;
using RogueLib.Utils;

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
    private const int ScreenMargin = 60;  // Margin from screen edges
    private const int BorderThickness = 2;
    private const int ProfileSize = 120;   // Size of the profile square
    private const int ProfileMargin = 20;  // Margin between profile and text

    public BestiaryPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection)
    {
        // Calculate screen dimensions
        var screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var screenHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
        
        // Calculate card width (screen width minus margins)
        var cardWidth = screenWidth - (ScreenMargin * 2);
        
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
        }, startY, Color.White, cardWidth);

        // Draw The Spinner entry
        DrawEnemyEntry(rayConnection, "The Spinner", new[]
        {
            "A whirling menace that bounces around",
            "- Larger collision radius than other enemies",
            "- Can be knocked back with sword",
            "- Deals 1 damage on contact",
            "- Moves in straight lines until hit"
        }, startY + EntrySpacing, Color.Yellow, cardWidth);

        // Draw The Charger entry
        DrawEnemyEntry(rayConnection, "The Charger", new[]
        {
            "A powerful enemy that hunts you down",
            "- Charges directly at the player",
            "- Takes 5 hits to defeat",
            "- Deals 2 damage on contact",
            "- Drops valuable gold when defeated",
            "- Appears after killing 20 regular enemies"
        }, startY + EntrySpacing * 2, Color.Red, cardWidth);

        // Draw return instruction
        _drawUtil.DrawText(rayConnection, "Press any key to return", 20, 
            screenHeight - 40, Color.White);
    }

    private void DrawEnemyEntry(IRayConnection rayConnection, string name, string[] description, float y, Color color, float cardWidth)
    {
        var screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var centerX = screenWidth / 2;
        var nameSize = Raylib.MeasureTextEx(rayConnection.MenuFont, name, EnemyNameSize, 1);
        
        // Calculate border dimensions
        float borderHeight = Math.Max(ProfileSize + (BorderPadding * 2), 
            nameSize.Y + (description.Length * (DescriptionSize + 5)) + (BorderPadding * 2));
        float borderX = centerX - cardWidth / 2;
        float borderY = y - BorderPadding;

        // Draw outer border
        Raylib.DrawRectangleLines(
            (int)borderX, (int)borderY,
            (int)cardWidth, (int)borderHeight,
            Color.Gold);

        // Draw profile box on the left
        float profileX = borderX + BorderPadding;
        float profileY = borderY + (borderHeight - ProfileSize) / 2;
        Raylib.DrawRectangleLines(
            (int)profileX, (int)profileY,
            ProfileSize, ProfileSize,
            Color.Gold);

        // Calculate text start position (after profile box)
        float textStartX = profileX + ProfileSize + ProfileMargin;
        float textWidth = cardWidth - (textStartX - borderX) - BorderPadding;

        // Draw name (aligned with text area)
        float nameX = textStartX;
        Raylib.DrawTextEx(rayConnection.MenuFont, name,
            new Vector2(nameX, y),
            EnemyNameSize, 1, color);

        // Draw description lines (aligned with text area)
        float descY = y + nameSize.Y + 10;
        foreach (var line in description)
        {
            Raylib.DrawTextEx(rayConnection.MenuFont, line,
                new Vector2(textStartX, descY),
                DescriptionSize, 1, Color.LightGray);
            descY += DescriptionSize + 5;
        }
    }
} 