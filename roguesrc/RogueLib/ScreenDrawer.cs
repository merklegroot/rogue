using Raylib_cs;
using System.Numerics;
using RogueLib.Constants;
namespace RogueLib;

public interface IScreenDrawer
{
    void DrawText(IRayConnection rayConnection, string text, int x, int y, Color color);
    void DrawCharacter(IRayConnection rayConnection, int charNum, int x, int y, Color color, bool showBorder = false, float scale = 1.0f);
}


public class ScreenDrawer : IScreenDrawer
{
    public void DrawText(IRayConnection rayConnection, string text, int x, int y, Color color)
    {
        Raylib.DrawTextEx(rayConnection.MenuFont, text, new Vector2(x, y), ScreenConstants.MenuFontSize, 1, color);
    }

    public void DrawCharacter(IRayConnection rayConnection, int charNum, int x, int y, Color color, bool showBorder = false, float scale = 1.0f)
    {
        var sourceX = charNum % 32;
        var sourceY = charNum / 32;

        Rectangle sourceRect = new(
            ScreenConstants.SidePadding + (sourceX * (ScreenConstants.CharWidth + ScreenConstants.CharHGap)),
            ScreenConstants.TopPadding + (sourceY * (ScreenConstants.CharHeight + ScreenConstants.CharVGap)),
            ScreenConstants.CharWidth,
            ScreenConstants.CharHeight
        );

        Rectangle destRect = new(
            x,
            y,
            ScreenConstants.CharWidth * ScreenConstants.DisplayScale * scale,
            ScreenConstants.CharHeight * ScreenConstants.DisplayScale * scale
        );

        // Draw the character
        Raylib.DrawTexturePro(
            rayConnection.CharsetTexture,
            sourceRect, destRect, Vector2.Zero, 0, color);

        if (showBorder)
        {
            // Draw border around destination rectangle
            Raylib.DrawRectangleLines(
                (int)destRect.X,
                (int)destRect.Y,
                (int)destRect.Width,
                (int)destRect.Height,
                Color.Gray
            );

            // Draw border around source rectangle (scaled to match destination)
            Raylib.DrawRectangleLines(
                (int)destRect.X - 1,
                (int)destRect.Y - 1,
                (int)destRect.Width + 2,
                (int)destRect.Height + 2,
                Color.DarkGray
            );
        }
    }
}