using Raylib_cs;
using System.Numerics;
using RogueLib.Constants;
namespace RogueLib;

public interface IScreenDrawer
{
    void DrawCharacter(IRayConnection rayConnection, int charNum, int x, int y, Color color, bool showBorder = false);
}


public class ScreenDrawer : IScreenDrawer
{
    public void DrawCharacter(IRayConnection rayConnection, int charNum, int x, int y, Color color, bool showBorder = false)
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
            ScreenConstants.CharWidth * ScreenConstants.DisplayScale,
            ScreenConstants.CharHeight * ScreenConstants.DisplayScale
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