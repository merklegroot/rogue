using Raylib_cs;
using System.Numerics;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IBannerPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class BannerPresenter : IBannerPresenter
{
    private const int BannerHeight = 100;
    private const int SkullSize = 32;
    private const int SkullSpacing = 40;
    private int _fontSize = 24;
    private const float BannerDuration = 3.0f;

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        if (!state.IsBannerVisible)
            return;

        // Update banner timer
        state.BannerTimer += Raylib.GetFrameTime();
        if (state.BannerTimer >= BannerDuration)
        {
            state.IsBannerVisible = false;
            state.BannerTimer = 0;
            return;
        }

        // Calculate banner position and size
        var screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        var screenHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
        var bannerY = screenHeight / 3;

        // Draw semi-transparent background
        Raylib.DrawRectangle(0, bannerY, screenWidth, BannerHeight, new Color(0, 0, 0, 200));

        // Draw text with a slight glow effect
        var bannerText = "Everybody's gangsta until the charger appears";
        var textSize = Raylib.MeasureTextEx(rayConnection.MenuFont, bannerText, _fontSize, 1);
        
        // Calculate total width needed for skulls and text
        var totalWidth = textSize.X + (2 * SkullSize) + (2 * SkullSpacing);
        
        // Ensure we have enough space for everything
        if (totalWidth > screenWidth)
        {
            // If not enough space, reduce font size until it fits
            var adjustedFontSize = _fontSize;
            while (totalWidth > screenWidth && adjustedFontSize > 12)
            {
                adjustedFontSize--;
                textSize = Raylib.MeasureTextEx(rayConnection.MenuFont, bannerText, adjustedFontSize, 1);
                totalWidth = textSize.X + (2 * SkullSize) + (2 * SkullSpacing);
            }
            _fontSize = adjustedFontSize;
        }

        // Calculate center position
        var centerX = screenWidth / 2;
        
        // Position elements relative to center
        var leftSkullX = centerX - (totalWidth / 2);
        var textX = leftSkullX + SkullSize + SkullSpacing;
        var rightSkullX = textX + textSize.X + SkullSpacing;

        // Calculate vertical positions
        var textY = bannerY + (BannerHeight - textSize.Y) / 2;
        var skullY = bannerY + (BannerHeight - SkullSize) / 2;

        // Draw skull textures on both sides
        // Draw left skull
        Raylib.DrawTexture(rayConnection.SkullTexture, 
            (int)leftSkullX, 
            (int)skullY, 
            Color.White);
        
        // Draw right skull
        Raylib.DrawTexture(rayConnection.SkullTexture, 
            (int)rightSkullX, 
            (int)skullY, 
            Color.White);

        // Draw text with a slight glow effect
        var glowColor = new Color(200, 0, 0, 100);
        Raylib.DrawTextEx(rayConnection.MenuFont, bannerText, 
            new Vector2(textX + 2, textY + 2), _fontSize, 1, glowColor);
        Raylib.DrawTextEx(rayConnection.MenuFont, bannerText, 
            new Vector2(textX, textY), _fontSize, 1, Color.Red);
    }
} 