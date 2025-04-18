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
    private readonly Font _menuFont;
    private readonly int _width;
    private readonly int _height;
    private GameView _currentView = GameView.Menu;
    private Queue<KeyboardKey> _keyEvents = new Queue<KeyboardKey>();
    
    // Character dimensions
    private const int CharWidth = 8;
    private const int CharHeight = 14;
    private const int DisplayScale = 4;
    private const int CharHGap = 1;
    private const int CharVGap = 2;
    private const int SidePadding = 8;
    private const int TopPadding = 10;

    private const int MenuFontSize = 32;

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
        
        Raylib.InitWindow(_width * CharWidth * DisplayScale, _height * CharHeight * DisplayScale, "Rogue-like");
        Raylib.SetTargetFPS(60);
        
        _charset = Raylib.LoadTexture("images/Codepage-437-transparent.png");
        _menuFont = Raylib.LoadFont("fonts/Roboto/static/Roboto-Regular.ttf");
    }

    public void Update()
    {
        // Collect all key events that occurred
        int key;
        while ((key = Raylib.GetKeyPressed()) != 0)
        {
            _keyEvents.Enqueue((KeyboardKey)key);
        }
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

        // Clear processed events
        _keyEvents.Clear();
    }

    private void DrawMenu()
    {
        // Draw menu text
        DrawText("Main Menu", 20, 20, Color.White);
        DrawColoredHotkeyText("View (C)haracter Set", 20, 60);
        DrawColoredHotkeyText("e(X)it", 20, 100);
    }

    private record ColoredHotKeyOptions
    {
        public Color BaseColor { get; init; } = DefaultBaseColor;
        public Color HotkeyColor { get; init; } = DefaultHotkeyColor;

        public static Color DefaultBaseColor = Color.Green;
        public static Color DefaultHotkeyColor = Color.Yellow;
    }

    private void DrawColoredHotkeyText(string text, int x, int y, ColoredHotKeyOptions? options = null)
    {
        var currentX = x;
        var startParenIndex = text.IndexOf('(');
        var endParenIndex = text.IndexOf(')');

        var baseColor = options?.BaseColor ?? ColoredHotKeyOptions.DefaultBaseColor;
        var hotkeyColor = options?.HotkeyColor ?? ColoredHotKeyOptions.DefaultHotkeyColor;
        
        if (startParenIndex != -1 && endParenIndex != -1 && endParenIndex > startParenIndex + 1)
        {
            // Draw text before parenthesis
            var beforeText = text.Substring(0, startParenIndex);
            DrawText(beforeText, currentX, y, baseColor);
            currentX += Raylib.MeasureText(beforeText, MenuFontSize) - 1;  // Slight kerning adjustment
            
            // Draw opening parenthesis
            DrawText("(", currentX, y, baseColor);
            currentX += Raylib.MeasureText("(", MenuFontSize) - 2;  // Tighter kerning for parentheses
            
            // Draw hotkey in different color
            var hotkey = text.Substring(startParenIndex + 1, endParenIndex - startParenIndex - 1);
            DrawText(hotkey, currentX, y, hotkeyColor);
            currentX += Raylib.MeasureText(hotkey, MenuFontSize) - 2;  // Tighter kerning for hotkey
            
            // Draw closing parenthesis
            DrawText(")", currentX, y, baseColor);
            currentX += Raylib.MeasureText(")", MenuFontSize) - 1;  // Slight kerning adjustment
            
            // Draw remaining text
            var afterText = text.Substring(endParenIndex + 1);
            DrawText(afterText, currentX, y, baseColor);
        }
        else
        {
            // If no parentheses found, draw the whole text in base color
            DrawText(text, x, y, baseColor);
        }
    }

    private void HandleMenuInput()
    {
        while (_keyEvents.Count > 0)
        {
            var key = _keyEvents.Dequeue();
            if (key == KeyboardKey.C)
            {
                _currentView = GameView.CharacterSet;
                break;
            }
            else if (key == KeyboardKey.X)
            {
                Raylib.CloseWindow();
                break;
            }
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

        DrawText("Press any key to return", 20, _height * CharHeight * DisplayScale - 40, Color.White);
    }

    private void HandleCharacterSetInput()
    {
        if (_keyEvents.Count > 0)
        {
            _currentView = GameView.Menu;
        }
    }

    private void DrawText(string text, int x, int y, Color color)
    {
        Raylib.DrawTextEx(_menuFont, text, new Vector2(x, y), MenuFontSize, 1, color);
    }

    private void DrawCharacter(int charNum, int x, int y, Color color)
    {
        int sourceX = charNum % 32;
        int sourceY = charNum / 32;

        Rectangle sourceRect = new Rectangle(
            SidePadding + (sourceX * (CharWidth + CharHGap)),
            TopPadding + (sourceY * (CharHeight + CharVGap)),
            CharWidth,
            CharHeight
        );

        Rectangle destRect = new Rectangle(
            x,
            y,
            CharWidth * DisplayScale,
            CharHeight * DisplayScale
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
        Raylib.UnloadFont(_menuFont);
        Raylib.CloseWindow();
    }
}