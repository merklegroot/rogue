using Raylib_cs;
using RogueLib.Constants;
using System.Numerics;
using RogueLib.Utils;
using RogueLib.Models;

namespace RogueLib.Presenters;

public interface IBestiaryPresenter
{
    void Draw(IRayConnection rayConnection);
}

public class BestiaryPresenter : IBestiaryPresenter
{
    private readonly IDrawUtil _drawUtil;
    private readonly IDrawEnemyUtil _drawEnemyUtil;
    private const int TitleSize = 48;
    private const int EnemyNameSize = 32;
    private const int DescriptionSize = 20;
    private const int EntrySpacing = 20;  // Vertical spacing between rows
    private const int BorderPadding = 20;
    private const int ScreenMargin = 60;  // Margin from screen edges
    private const int ProfileSize = 120;   // Size of the profile square
    private const int ProfileMargin = 20;  // Margin between profile and text
    private const int HorizontalSpacing = 40; // Spacing between entries in the same row

    private record struct EnemyEntry(string Name, string[] Description, Color Color, EnemyEnum Type);

    public BestiaryPresenter(IDrawUtil drawUtil, IDrawEnemyUtil drawEnemyUtil)
    {
        _drawUtil = drawUtil;
        _drawEnemyUtil = drawEnemyUtil;
    }

    public void Draw(IRayConnection rayConnection)
    {
        // Define all enemy entries
        var entries = new[]
        {
            new EnemyEntry(
                "The Kestrel",
                new[]
                {
                    "An agile flying enemy that hunts the player",
                    "- Can only move left or right",
                    "- Darts toward player when between it and wall",
                    "- Keeps moving until hitting a wall",
                    "- Takes 1 hit to defeat",
                    "- Deals 1 damage on contact"
                },
                Color.SkyBlue,
                EnemyEnum.Kestrel
            ),
            new EnemyEntry(
                "The Cedilla",
                new[]
                {
                    "A basic enemy that wanders aimlessly",
                    "- Moves randomly in 8 directions",
                    "- Takes 1 hit to defeat",
                    "- Deals 1 damage on contact"
                },
                Color.White,
                EnemyEnum.Cedilla
            ),
            new EnemyEntry(
                "The Spinner",
                new[]
                {
                    "A whirling menace that bounces around",
                    "- Larger collision radius than other enemies",
                    "- Can be knocked back with sword",
                    "- Deals 1 damage on contact",
                    "- Moves in straight lines until hit"
                },
                Color.Yellow,
                EnemyEnum.Spinner
            ),
            new EnemyEntry(
                "The Charger",
                new[]
                {
                    "A powerful enemy that hunts you down",
                    "- Charges directly at the player",
                    "- Takes 5 hits to defeat",
                    "- Deals 2 damage on contact",
                    "- Drops valuable gold when defeated",
                    "- Appears after killing 20 regular enemies"
                },
                Color.Red,
                EnemyEnum.Charger
            ),
            new EnemyEntry(
                "The Minotaur",
                new[]
                {
                    "A legendary beast that lurks in the maze",
                    "- Large and imposing, but does not move (yet)",
                    "- Takes many hits to defeat (future)",
                    "- Deals heavy damage on contact (future)",
                    "- Rarely spawns among regular enemies"
                },
                Color.Brown,
                EnemyEnum.Minotaur
            )
        };

        // Calculate screen dimensions
        var screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var screenHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
        
        // Calculate card width (screen width minus margins and spacing between cards)
        var totalWidth = screenWidth - (ScreenMargin * 2);
        var cardWidth = (totalWidth - HorizontalSpacing) / 2; // Split width between two cards
        
        // Calculate maximum height needed for any entry
        float maxHeight = 0;
        foreach (var entry in entries)
        {
            var nameSize = Raylib.MeasureTextEx(rayConnection.MenuFont, entry.Name, EnemyNameSize, 1);
            var height = Math.Max(
                ProfileSize + (BorderPadding * 2),
                nameSize.Y + (entry.Description.Length * (DescriptionSize + 5)) + (BorderPadding * 2)
            );
            maxHeight = Math.Max(maxHeight, height);
        }
        
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
        Raylib.DrawRectangle((int)(centerX - lineWidth/2), lineY, (int)lineWidth, 2, Color.Gold);

        // Start Y position for enemy entries
        var startY = lineY + 50;

        // Calculate X positions for left and right columns
        var leftX = ScreenMargin;
        var rightX = ScreenMargin + cardWidth + HorizontalSpacing;

        // Draw entries in rows of two
        for (int i = 0; i < entries.Length; i += 2)
        {
            var rowY = startY + (i / 2 * (maxHeight + EntrySpacing));
            
            // Draw left entry
            DrawEnemyEntry(rayConnection, entries[i].Name, entries[i].Description, rowY, leftX, 
                entries[i].Color, cardWidth, entries[i].Type, maxHeight);
            
            // Draw right entry if it exists
            if (i + 1 < entries.Length)
            {
                DrawEnemyEntry(rayConnection, entries[i + 1].Name, entries[i + 1].Description, rowY, rightX, 
                    entries[i + 1].Color, cardWidth, entries[i + 1].Type, maxHeight);
            }
        }

        // Draw return instruction
        _drawUtil.DrawText(rayConnection, "Press any key to return", 20, 
            screenHeight - 40, Color.White);
    }

    private void DrawEnemyEntry(IRayConnection rayConnection, string name, string[] description, float y, float x, Color color, float cardWidth, EnemyEnum enemyType, float borderHeight)
    {
        var nameSize = Raylib.MeasureTextEx(rayConnection.MenuFont, name, EnemyNameSize, 1);
        float borderX = x;
        float borderY = y - BorderPadding;

        // Use consistent dark background colors based on white
        var backgroundColor = new Color(
            (byte)(255 * 0.2f),
            (byte)(255 * 0.2f),
            (byte)(255 * 0.2f),
            (byte)255
        );

        // Even darker background for profile box
        var profileBackgroundColor = new Color(
            (byte)(255 * 0.1f),
            (byte)(255 * 0.1f),
            (byte)(255 * 0.1f),
            (byte)255
        );

        // Draw main entry background
        Raylib.DrawRectangle(
            (int)borderX, (int)borderY,
            (int)cardWidth, (int)borderHeight,
            backgroundColor);

        // Draw outer border
        Raylib.DrawRectangleLines(
            (int)borderX, (int)borderY,
            (int)cardWidth, (int)borderHeight,
            Color.Gold);

        // Draw profile box background and border
        float profileX = borderX + BorderPadding;
        float profileY = borderY + (borderHeight - ProfileSize) / 2;

        // Draw profile background
        Raylib.DrawRectangle(
            (int)profileX, (int)profileY,
            ProfileSize, ProfileSize,
            profileBackgroundColor);

        // Draw profile border
        Raylib.DrawRectangleLines(
            (int)profileX, (int)profileY,
            ProfileSize, ProfileSize,
            Color.Gold);

        // Draw the enemy in the profile box - adjusted to be more centered
        var screenPos = new Coord2dFloat(
            profileX + ProfileSize/2 - (ScreenConstants.CharWidth * ScreenConstants.DisplayScale)/2,
            profileY + ProfileSize/2 - (ScreenConstants.CharHeight * ScreenConstants.DisplayScale)/2
        );

        if (enemyType == EnemyEnum.Spinner)
        {
            _drawEnemyUtil.DrawSpinner(rayConnection, screenPos, 0);
        }
        else
        {
            _drawEnemyUtil.Draw(rayConnection, enemyType, screenPos, null);
        }

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