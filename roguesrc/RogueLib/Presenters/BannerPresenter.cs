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
    private const int SkullSpacing = 20;
    private const int FontSize = 24;
    private const float BannerDuration = 3.0f;

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        if (state.IsBannerVisible)
        {
            // Update banner timer
            state.BannerTimer += Raylib.GetFrameTime();
            if (state.BannerTimer >= BannerDuration)
            {
                state.IsBannerVisible = false;
                state.BannerTimer = 0;
                return;
            }

            // Calculate banner position and size
            int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
            int screenHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
            int bannerY = screenHeight / 3;

            // Draw semi-transparent background
            Raylib.DrawRectangle(0, bannerY, screenWidth, BannerHeight, new Color(0, 0, 0, 200));

            // Draw text with a slight glow effect
            string bannerText = "Everybody's gangsta until the charger appears";
            Vector2 textSize = Raylib.MeasureTextEx(rayConnection.MenuFont, bannerText, FontSize, 1);
            float textX = (screenWidth - textSize.X) / 2;
            float textY = bannerY + (BannerHeight - textSize.Y) / 2;

            // Draw skull textures on both sides
            // Draw left skull
            Raylib.DrawTexture(rayConnection.SkullTexture, 
                (int)(textX - SkullSize - SkullSpacing), 
                (int)(textY + (textSize.Y - SkullSize) / 2), 
                Color.White);
            
            // Draw right skull
            Raylib.DrawTexture(rayConnection.SkullTexture, 
                (int)(textX + textSize.X + SkullSpacing), 
                (int)(textY + (textSize.Y - SkullSize) / 2), 
                Color.White);

            // Draw text with a slight glow effect
            Color glowColor = new Color(200, 0, 0, 100);
            Raylib.DrawTextEx(rayConnection.MenuFont, bannerText, 
                new Vector2(textX + 2, textY + 2), FontSize, 1, glowColor);
            Raylib.DrawTextEx(rayConnection.MenuFont, bannerText, 
                new Vector2(textX, textY), FontSize, 1, Color.Red);
        }
    }
} 