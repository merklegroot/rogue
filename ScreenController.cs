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
    private const string FONT_PATH = "font/Ac437_IBM_VGA_8x16.ttf";

    public ScreenController()
    {
        _width = 80;  // Standard terminal width
        _height = 16;
        
        // Initialize Raylib
        Raylib.InitWindow(_width * FONT_SIZE, _height * FONT_SIZE, "Rogue-like");
        Raylib.SetTargetFPS(60);
        
        // Load the CP437 font
        _font = Raylib.LoadFont(FONT_PATH);
        
        // Initialize buffer
        _buffer = new StringBuilder(_width * _height);
        _emptyLine = new string((char)_ground, _width);
    }

    public void Draw(GameState state)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        // Build and draw the screen
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

        Raylib.EndDrawing();
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