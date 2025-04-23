using Raylib_cs;
using System.Numerics;
using RogueLib.Constants;
namespace RogueLib;

public interface IScreenDrawerUtil
{
    void DrawText(IRayConnection rayConnection, string text, int x, int y, Color color);
    void DrawCharacter(IRayConnection rayConnection, int charNum, int x, int y, Color color, bool showBorder = false, float scale = 1.0f);
    void DrawColoredHotkeyText(IRayConnection rayConnection, string text, int x, int y, ColoredHotkeyOptions? options = null);
}


public class ScreenDrawerUtilUtil : IScreenDrawerUtil
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

        // Calculate scaled dimensions - only scale height
        float scaledWidth = ScreenConstants.CharWidth * ScreenConstants.DisplayScale;  // Keep width constant
        float scaledHeight = ScreenConstants.CharHeight * ScreenConstants.DisplayScale * scale;
        
        // Calculate the difference in height to keep bottom anchored
        float heightDiff = scaledHeight - (ScreenConstants.CharHeight * ScreenConstants.DisplayScale);
        
        // Adjust Y position to keep bottom anchored
        float adjustedY = y - heightDiff;

        Rectangle destRect = new(
            x,
            adjustedY,
            scaledWidth,
            scaledHeight
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

    public void DrawColoredHotkeyText(IRayConnection rayConnection, string text, int x, int y, ColoredHotkeyOptions? options = null)
    {
        options ??= new ColoredHotkeyOptions();

        int startParenIndex = text.IndexOf('(');
        int endParenIndex = text.IndexOf(')');

        // Guard clause: if no valid parentheses found, draw plain text
        if (startParenIndex == -1 || endParenIndex == -1 || endParenIndex <= startParenIndex + 1)
        {
            DrawText(rayConnection, text, x, y, options.BaseColor);
            return;
        }

        // Calculate positions using MeasureTextEx for more accurate spacing
        var currentX = x;

        // Draw text before parenthesis
        var beforeText = text[..startParenIndex];
        DrawText(rayConnection, beforeText, currentX, y, options.BaseColor);
        
        // Use MeasureTextEx for more accurate width measurement
        Vector2 beforeSize = Raylib.MeasureTextEx(rayConnection.MenuFont, beforeText, ScreenConstants.MenuFontSize, 1);
        currentX += (int)beforeSize.X - 4;  // Reduce spacing before parenthesis

        // Draw opening parenthesis
        DrawText(rayConnection, "(", currentX, y, options.BaseColor);
        Vector2 parenSize = Raylib.MeasureTextEx(rayConnection.MenuFont, "(", ScreenConstants.MenuFontSize, 1);
        currentX += (int)parenSize.X - 2;  // Tighter spacing after opening parenthesis

        // Draw hotkey in different color
        var hotkey = text[(startParenIndex + 1)..endParenIndex];
        DrawText(rayConnection, hotkey, currentX, y, options.HotkeyColor);
        Vector2 hotkeySize = Raylib.MeasureTextEx(rayConnection.MenuFont, hotkey, ScreenConstants.MenuFontSize, 1);
        currentX += (int)hotkeySize.X - 2;  // Tighter spacing after hotkey

        // Draw closing parenthesis
        DrawText(rayConnection, ")", currentX, y, options.BaseColor);
        Vector2 closeParenSize = Raylib.MeasureTextEx(rayConnection.MenuFont, ")", ScreenConstants.MenuFontSize, 1);
        currentX += (int)closeParenSize.X - 2;  // Tighter spacing after closing parenthesis

        // Draw remaining text
        var afterText = text[(endParenIndex + 1)..];
        DrawText(rayConnection, afterText, currentX, y, options.BaseColor);
    }
}