using Raylib_cs;

namespace RogueLib;

public record ColoredHotkeyOptions
{
    public Color BaseColor { get; init; } = DefaultBaseColor;
    public Color HotkeyColor { get; init; } = DefaultHotkeyColor;

    public static Color DefaultBaseColor = Color.Green;
    public static Color DefaultHotkeyColor = Color.Yellow;
}