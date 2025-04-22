using Raylib_cs;

namespace RogueLib.Constants;

public static class ScreenConstants
{
    public const int MenuFontSize = 32;
    public const int Width = 40;
    public const int Height = 16;
    public const int CharWidth = 8;
    public const int CharHeight = 14;
    public const int DisplayScale = 4;
    public const int TargetFps = 60;
    public const string Title = "ROGUE ADVENTURE";


    public const int EnemyChar = 128; // (Captial C with a cedilla under it)

    public static readonly Color SwordColor = new(180, 210, 230, 255);  // Silvery-blue color

    public static readonly Color PlayerColor = Color.Yellow;  // Yellow for player
    public static readonly Color EnemyColor = new(255, 100, 100, 255); // Red for enemies

    public const int SidePadding = 8;
    public const int TopPadding = 10;

    public const int CharHGap = 1;
    public const int CharVGap = 2;

    public const int MaxHealth = 10;

    public static readonly Color HealthColor = Color.Red;
    
    public static readonly Color GoldColor = new(255, 215, 0, 255);

    public static readonly Color BackgroundColor = new(40, 44, 52, 255);  // #282c34

    public static readonly Color[] SampleColors = new[]
    {
        Color.White,
        Color.Red,
        Color.Green,
        Color.Blue,
        Color.Yellow,
        Color.Purple,
        Color.Orange,
        Color.Pink
    };
    
    
    public static readonly Color ExplosionColor = new(255, 165, 0, 255);  // Orange color for explosions
}