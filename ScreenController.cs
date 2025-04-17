using Raylib_cs;
using System.Numerics;

public class ScreenController
{
    private readonly Texture2D _charset;
    private readonly int _width;
    private readonly int _height;
    
    // Character dimensions
    private const int CHAR_WIDTH = 8;
    private const int CHAR_HEIGHT = 10;
    private const int CHAR_H_GAP = 1;
    private const int CHAR_V_GAP = 6;
    private const int SIDE_PADDING = 8;
    private const int TOP_PADDING = 10;

    private const string FONT_PATH = "Ac437_IBM_VGA_8x16.ttf";

    public ScreenController()
    {
        _width = 80;
        _height = 16;
        
        Raylib.InitWindow(_width * CHAR_WIDTH, _height * CHAR_HEIGHT, "Rogue-like");
        Raylib.SetTargetFPS(60);
        
        // Load the character set image
        _charset = Raylib.LoadTexture("images/Codepage-437.png");
    }

    public void Draw(GameState state)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        // Draw characters 1-4 in different colors (first row)
        DrawCharacter(1, 10, 10, Color.White);    // Smiley
        DrawCharacter(2, 20, 10, Color.Red);      // Inverse smiley
        DrawCharacter(3, 30, 10, Color.Green);    // Heart
        DrawCharacter(4, 40, 10, Color.Blue);     // Diamond

        // Draw characters 5-8 in different colors (second row)
        DrawCharacter(5, 10, 30, Color.Yellow);   // Club
        DrawCharacter(6, 20, 30, Color.Purple);   // Spade
        DrawCharacter(7, 30, 30, Color.Orange);   // •
        DrawCharacter(8, 40, 30, Color.Pink);     // ◘

        Raylib.EndDrawing();
    }

    private void DrawCharacter(int charNum, int x, int y, Color color)
    {
        Rectangle sourceRect = new Rectangle(
            SIDE_PADDING + (charNum * (CHAR_WIDTH + CHAR_H_GAP)),
            TOP_PADDING,
            CHAR_WIDTH,
            CHAR_HEIGHT
        );

        Rectangle destRect = new Rectangle(
            x,
            y,
            CHAR_WIDTH,
            CHAR_HEIGHT
        );

        Raylib.DrawTexturePro(_charset, sourceRect, destRect, Vector2.Zero, 0, color);
    }

    public bool WindowShouldClose()
    {
        return Raylib.WindowShouldClose();
    }

    public void Cleanup()
    {
        Raylib.UnloadTexture(_charset);
        Raylib.CloseWindow();
    }
}