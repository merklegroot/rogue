using Raylib_cs;
using System.Text;
using System.Numerics;

public class ScreenController
{
    private readonly int _player = 0x01;  // CP437 white smiley face character
    private readonly int _ground = 0x2E;  // CP437 period character (.)
    private readonly int _width;
    private readonly int _height;
    private readonly StringBuilder _buffer;
    private readonly string _emptyLine;
    private readonly Font _font;
    private const int FONT_SIZE = 16;
    // private const string FONT_PATH = "font/Ac437_IBM_VGA_8x16.ttf";

    private const string FONT_PATH = "font/Dernyn's-256(baseline).ttf";
    private bool _showFontTest = true;  // Start with font test mode

    public ScreenController()
    {
        _width = 80;
        _height = 40;
        
        // Initialize Raylib
        Raylib.InitWindow(_width * FONT_SIZE, _height * FONT_SIZE, "Rogue-like");
        Raylib.SetTargetFPS(60);
        
        // Load the CP437 font
        _font = Raylib.LoadFont(FONT_PATH);
        Console.WriteLine($"Font loaded: {_font.BaseSize} glyphs available");
        
        // Initialize buffer
        _buffer = new StringBuilder(_width * _height);
        _emptyLine = new string((char)_ground, _width);
    }

    public void Draw(GameState state)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        if (_showFontTest)
        {
            // Draw font test screen
            DrawFontTest();
            
            // Draw instructions
            string instructions = "Press SPACE to continue to game";
            Raylib.DrawTextEx(_font, instructions, new Vector2(10, _height * FONT_SIZE - 30), FONT_SIZE, 1, Color.White);
        }
        else
        {
            // Build and draw the game screen
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    string charToDraw = x == state.PlayerX && y == state.PlayerY 
                        ? ((char)_player).ToString() 
                        : ((char)_ground).ToString();
                    
                    Vector2 position = new Vector2(x * FONT_SIZE, y * FONT_SIZE);
                    Raylib.DrawTextEx(
                        _font,
                        charToDraw,
                        position,
                        FONT_SIZE,
                        1,  // spacing
                        Color.White
                    );
                }
            }
        }

        Raylib.EndDrawing();
        
        // Check for space key to toggle font test mode
        if (Raylib.IsKeyPressed(KeyboardKey.Space))
        {
            _showFontTest = !_showFontTest;
        }
    }
    
    private void DrawFontTest()
    {
        // Display all 256 CP437 characters in a grid
        int charsPerRow = 16;
        int rows = 16;
        
        for (int i = 0; i < 256; i++)
        {
            int x = (i % charsPerRow) * FONT_SIZE * 2;
            int y = (i / charsPerRow) * FONT_SIZE * 2;
            
            // Draw character
            string charToDraw = ((char)i).ToString();
            Raylib.DrawTextEx(
                _font,
                charToDraw,
                new Vector2(x, y),
                FONT_SIZE,
                1,
                Color.White
            );
            
            // Draw hex value
            string hexValue = $"{i:X2}";
            Raylib.DrawTextEx(
                _font,
                hexValue,
                new Vector2(x, y + FONT_SIZE),
                FONT_SIZE / 2,
                1,
                Color.Gray
            );
        }
    }

    public bool WindowShouldClose()
    {
        return Raylib.WindowShouldClose();
    }

    public void Cleanup()
    {
        Raylib.UnloadFont(_font);
        Raylib.CloseWindow();
    }
} 