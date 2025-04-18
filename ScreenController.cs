using Raylib_cs;
using System.Numerics;

public enum GameView
{
    Menu,
    CharacterSet,
    Animation
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
                
            case GameView.Animation:
                DrawAnimation();
                HandleAnimationInput();
                break;
        }

        Raylib.EndDrawing();

        // Clear processed events
        _keyEvents.Clear();
    }

    private record ColoredHotkeyOptions
    {
        public Color BaseColor { get; init; } = DefaultBaseColor;
        public Color HotkeyColor { get; init; } = DefaultHotkeyColor;

        public static Color DefaultBaseColor = Color.Green;
        public static Color DefaultHotkeyColor = Color.Yellow;
    }

    private void DrawMenu()
    {
        // Draw menu text
        DrawText("Main Menu", 20, 20, Color.White);
        DrawColoredHotkeyText("View (C)haracter Set", 20, 60);
        DrawColoredHotkeyText("(A)nimation", 20, 100);
        DrawColoredHotkeyText("e(X)it", 20, 140);
    }

    private void DrawColoredHotkeyText(string text, int x, int y, ColoredHotkeyOptions? options = null)
    {
        options ??= new ColoredHotkeyOptions();
        
        int startParenIndex = text.IndexOf('(');
        int endParenIndex = text.IndexOf(')');
        
        // Guard clause: if no valid parentheses found, draw plain text
        if (startParenIndex == -1 || endParenIndex == -1 || endParenIndex <= startParenIndex + 1)
        {
            DrawText(text, x, y, options.BaseColor);
            return;
        }

        var currentX = x;
        
        // Draw text before parenthesis
        var beforeText = text[..startParenIndex];
        DrawText(beforeText, currentX, y, options.BaseColor);
        currentX += Raylib.MeasureText(beforeText, MenuFontSize) - 1;  // Slight kerning adjustment
        
        // Draw opening parenthesis
        DrawText("(", currentX, y, options.BaseColor);
        currentX += Raylib.MeasureText("(", MenuFontSize) - 2;  // Tighter kerning for parentheses
        
        // Draw hotkey in different color
        var hotkey = text[(startParenIndex + 1)..endParenIndex];
        DrawText(hotkey, currentX, y, options.HotkeyColor);
        currentX += Raylib.MeasureText(hotkey, MenuFontSize) - 2;  // Tighter kerning for hotkey
        
        // Draw closing parenthesis
        DrawText(")", currentX, y, options.BaseColor);
        currentX += Raylib.MeasureText(")", MenuFontSize) - 1;  // Slight kerning adjustment
        
        // Draw remaining text
        var afterText = text[(endParenIndex + 1)..];
        DrawText(afterText, currentX, y, options.BaseColor);
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
            if (key == KeyboardKey.A)
            {
                _currentView = GameView.Animation;
                break;
            }
            if (key == KeyboardKey.X)
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

    private void DrawAnimation()
    {
        // Draw a field of dots
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 20; x++)
            {
                // Draw dots everywhere except at position (10, 5)
                if (x == 10 && y == 5)
                {
                    // Draw smiley face at center
                    DrawCharacter(1, 100 + x * 40, 100 + y * 40, Color.Yellow);
                }
                else
                {
                    // Draw dots
                    DrawCharacter(0x2E, 100 + x * 40, 100 + y * 40, Color.White);
                }
            }
        }

        DrawText("Press ESC to return to menu", 20, _height * CharHeight * DisplayScale - 40, Color.White);
    }

    private void HandleAnimationInput()
    {
        while (_keyEvents.Count > 0)
        {
            var key = _keyEvents.Dequeue();
            if (key == KeyboardKey.Escape)
            {
                _currentView = GameView.Menu;
                break;
            }
        }
    }

    private void DrawText(string text, int x, int y, Color color)
    {
        Raylib.DrawTextEx(_menuFont, text, new Vector2(x, y), MenuFontSize, 1, color);
    }

    private void DrawCharacter(int charNum, int x, int y, Color color)
    {
        var sourceX = charNum % 32;
        var sourceY = charNum / 32;

        Rectangle sourceRect = new(
            SidePadding + (sourceX * (CharWidth + CharHGap)),
            TopPadding + (sourceY * (CharHeight + CharVGap)),
            CharWidth,
            CharHeight
        );

        Rectangle destRect = new(
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