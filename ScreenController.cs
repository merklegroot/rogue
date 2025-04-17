using Raylib_cs;
using System.Numerics;

public enum GameView
{
    Menu,
    CharacterSet
}

public class ScreenController
{
    private readonly Texture2D _charset;
    private readonly int _width;
    private readonly int _height;
    private GameView _currentView = GameView.Menu;
    
    // Character dimensions
    private const int CHAR_WIDTH = 8;
    private const int CHAR_HEIGHT = 14;
    private const int DISPLAY_SCALE = 4;
    private const int CHAR_H_GAP = 1;
    private const int CHAR_V_GAP = 2;
    private const int SIDE_PADDING = 8;
    private const int TOP_PADDING = 10;

    // React default background color
    private readonly Color _backgroundColor = new Color(40, 44, 52, 255);  // #282c34

    private readonly Color[] _colors = new Color[] 
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

    public ScreenController()
    {
        _width = 40;
        _height = 16;
        
        Raylib.InitWindow(_width * CHAR_WIDTH * DISPLAY_SCALE, _height * CHAR_HEIGHT * DISPLAY_SCALE, "Rogue-like");
        Raylib.SetTargetFPS(60);
        
        _charset = Raylib.LoadTexture("images/Codepage-437-transparent.png");
    }

    public void Draw(GameState state)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(_backgroundColor);

        switch (_currentView)
        {
            case GameView.Menu:
                DrawMenu();
                HandleMenuInput();
                break;
            
            case GameView.CharacterSet:
                DrawCharacterSet();
                HandleCharacterSetInput();
                break;
        }

        Raylib.EndDrawing();
    }

    private void DrawMenu()
    {
        // Draw menu text
        DrawText("Main Menu", 20, 20, Color.White);
        DrawText("View (C)haracter Set", 20, 60, Color.White);
    }

    private void HandleMenuInput()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.C))
        {
            _currentView = GameView.CharacterSet;
        }
    }

    private void DrawCharacterSet()
    {
        // Draw all characters in a grid
        for (int charNum = 0; charNum < 256; charNum++)
        {
            int row = charNum / 32;
            int col = charNum % 32;
            
            DrawCharacter(
                charNum,
                20 + (col * 40),
                20 + (row * 60),
                _colors[charNum % _colors.Length]
            );
        }

        DrawText("Press any key to return", 20, _height * CHAR_HEIGHT * DISPLAY_SCALE - 40, Color.White);
    }

    private void HandleCharacterSetInput()
    {
        if (Raylib.GetKeyPressed() != 0)
        {
            _currentView = GameView.Menu;
        }
    }

    private void DrawText(string text, int x, int y, Color color)
    {
        Raylib.DrawText(text, x, y, 20, color);
    }

    private void DrawCharacter(int charNum, int x, int y, Color color)
    {
        int sourceX = charNum % 32;
        int sourceY = charNum / 32;

        Rectangle sourceRect = new Rectangle(
            SIDE_PADDING + (sourceX * (CHAR_WIDTH + CHAR_H_GAP)),
            TOP_PADDING + (sourceY * (CHAR_HEIGHT + CHAR_V_GAP)),
            CHAR_WIDTH,
            CHAR_HEIGHT
        );

        Rectangle destRect = new Rectangle(
            x,
            y,
            CHAR_WIDTH * DISPLAY_SCALE,
            CHAR_HEIGHT * DISPLAY_SCALE
        );

        // Draw the character
        Raylib.DrawTexturePro(_charset, sourceRect, destRect, Vector2.Zero, 0, color);
        
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