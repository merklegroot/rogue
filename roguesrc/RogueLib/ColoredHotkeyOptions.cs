using Raylib_cs;

namespace RogueLib;

public record ColoredHotkeyOptions
{
    public Color BaseColor { get; init; } = DefaultBaseColor;
    public Color HotkeyColor { get; init; } = DefaultHotkeyColor;
    public bool IsHovered { get; init; } = false;
    public Rectangle? HoverBounds { get; init; } = null;

    public static Color DefaultBaseColor = Color.Green;
    public static Color DefaultHotkeyColor = Color.Yellow;
}