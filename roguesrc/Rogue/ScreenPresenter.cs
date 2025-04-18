using Raylib_cs;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System;
using System.IO;

public interface IScreenPresenter
{
    void Update();
    void Draw(GameState state);
    bool WindowShouldClose();
    void Cleanup();
}

public class ScreenPresenter : IScreenPresenter
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

    private int _animPlayerX = 10;
    private int _animPlayerY = 5;
    private float _timeSinceLastMove = 0;

    // Add these fields to track direction and sword state
    private Direction _lastDirection = Direction.Right;
    private bool _isSwordSwinging = false;
    private float _swordSwingTime = 0;
    private const float SwordSwingDuration = 0.25f;  // Reduced from 0.3f to make animation even faster

    // Add a silvery-blue color for the sword
    private readonly Color _swordColor = new Color(180, 210, 230, 255);  // Silvery-blue color

    // Add enemy fields
    private int _enemyX = 15;
    private int _enemyY = 5;
    private float _enemyMoveTimer = 0;
    private const float EnemyMoveDelay = 0.8f;  // Move every 0.8 seconds
    private readonly Random _random = new Random();
    private bool _enemyAlive = true;  // Add this field
    
    // Multiple enemies support
    private List<Enemy> _enemies = new List<Enemy>();
    private float _enemySpawnTimer = 0;
    private const float EnemySpawnDelay = 1.0f;  // Spawn a new enemy every 1 second
    private const int MaxEnemies = 3;  // Maximum number of enemies

    // Add health fields
    private int _maxHealth = 10;
    private int _currentHealth = 7;  // Start with 7 out of 10 health
    private readonly Color _healthColor = Color.Red;
    private readonly Color _emptyHealthColor = new Color(100, 100, 100, 255);  // Gray color for empty hearts
    
    // Add invincibility fields
    private bool _isInvincible = false;
    private float _invincibilityTimer = 0f;
    private const float InvincibilityDuration = 1.0f;  // 1 second of invincibility after taking damage

    public ScreenPresenter()
    {
        _width = 40;
        _height = 16;
        
        Raylib.InitWindow(_width * CharWidth * DisplayScale, _height * CharHeight * DisplayScale, "Rogue-like");
        Raylib.SetTargetFPS(60);
        
        _charset = Raylib.LoadTexture("images/Codepage-437-transparent.png");
        
        // Load font from embedded resource
        _menuFont = LoadFontFromEmbeddedResource("Rogue.fonts.Roboto-Regular.ttf");
        
        // Initialize first enemy
        _enemies.Add(new Enemy { X = _enemyX, Y = _enemyY });
    }

    private Font LoadFontFromEmbeddedResource(string resourceName)
    {
        // Get the current assembly
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        
        // Open a stream to the embedded resource
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
            }
            
            // Read the font data into a byte array
            byte[] fontData = new byte[stream.Length];
            stream.Read(fontData, 0, fontData.Length);
            
            // Create a temporary file to load the font
            // string tempFile = Path.GetTempFileName();
            var tempFile = "Roboto-Regular.ttf";
            File.WriteAllBytes(tempFile, fontData);
            
            // Load the font using Raylib
            Font font = Raylib.LoadFont(tempFile);
            
            // Delete the temporary file
            try { File.Delete(tempFile); } catch { /* Ignore errors */ }
            
            return font;
        }
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
        // Draw health bar at the top
        DrawHealthBar();
        
        // Draw a field of dots
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 20; x++)
            {
                // Draw player
                if (x == _animPlayerX && y == _animPlayerY)
                {
                    // Draw smiley face at player position
                    // If invincible, make the player flash by alternating visibility based on timer
                    Color playerColor = Color.Yellow;
                    if (_isInvincible && (int)(_invincibilityTimer * 10) % 2 == 0)
                    {
                        playerColor = new Color((byte)playerColor.R, (byte)playerColor.G, (byte)playerColor.B, (byte)128);
                    }
                    DrawCharacter(1, 100 + x * 40, 100 + y * 40, playerColor);
                }
                // Draw enemies
                else if (_enemies.Any(e => e.Alive && e.X == x && e.Y == y))
                {
                    DrawCharacter(128, 100 + x * 40, 100 + y * 40, Color.RayWhite);
                }
                else
                {
                    // Draw ground dots
                    const char groundChar = (char)250;
                    DrawCharacter(groundChar, 100 + x * 40, 100 + y * 40, Color.Green);
                }
            }
        }
        
        // Draw sword if swinging (drawn after ground to appear on top)
        if (_isSwordSwinging)
        {
            // Calculate animation progress (0.0 to 1.0)
            float progress = _swordSwingTime / SwordSwingDuration;
            if (progress > 1.0f) progress = 1.0f;
            
            // Calculate frame (0, 1, or 2)
            int frame = (int)(progress * 3);
            if (frame > 2) frame = 2;
            
            // Calculate fractional position
            float xOffset = 0;
            float yOffset = 0;
            
            // Determine position based on direction and animation progress
            switch (_lastDirection)
            {
                case Direction.Left:
                    // Fixed position to the left, sweeping from top to bottom
                    xOffset = -0.9f;  // Reduced from -1.2f to bring closer to character
                    yOffset = (progress - 0.5f) * 1.2f;  // Kept the vertical range the same
                    break;
                case Direction.Right:
                    // Fixed position to the right, sweeping from top to bottom
                    xOffset = 0.9f;  // Reduced from 1.2f to bring closer to character
                    yOffset = (progress - 0.5f) * 1.2f;  // Kept the vertical range the same
                    break;
                case Direction.Up:
                    // Fixed position above, sweeping from left to right
                    yOffset = -1.2f;  // Kept the same
                    xOffset = (progress - 0.5f) * 1.2f;  // Kept the same
                    break;
                case Direction.Down:
                    // Fixed position below, sweeping from left to right
                    yOffset = 1.2f;  // Kept the same
                    xOffset = (progress - 0.5f) * 1.2f;  // Kept the same
                    break;
            }
            
            // Get sword character based on direction and animation frame
            char swordChar = (_lastDirection, frame) switch
            {
                // Left side: \ → - → /
                (Direction.Left, 0) => '\\',
                (Direction.Left, 1) => '-',
                (Direction.Left, 2) => '/',
                
                // Right side: / → - → \
                (Direction.Right, 0) => '/',
                (Direction.Right, 1) => '-',
                (Direction.Right, 2) => '\\',
                
                // Up: \ → | → /
                (Direction.Up, 0) => '\\',
                (Direction.Up, 1) => '|',
                (Direction.Up, 2) => '/',
                
                // Down: / → | → \
                (Direction.Down, 0) => '/',
                (Direction.Down, 1) => '|',
                (Direction.Down, 2) => '\\',
                
                // Fallback
                _ => '+'
            };
            
            // Calculate exact pixel position
            float swordX = 100 + (_animPlayerX + xOffset) * 40;
            float swordY = 100 + (_animPlayerY + yOffset) * 40;
            
            // Draw the sword character with silvery-blue color
            DrawCharacter(swordChar, (int)swordX, (int)swordY, _swordColor);
        }

        DrawText("Use WASD to move, SPACE to swing sword, ESC to return to menu", 20, _height * CharHeight * DisplayScale - 40, Color.White);
    }
    
    private void HandleAnimationInput()
    {
        // Handle ESC key via event queue for menu navigation
        while (_keyEvents.Count > 0)
        {
            var key = _keyEvents.Dequeue();
            if (key == KeyboardKey.Escape)
            {
                _currentView = GameView.Menu;
                return;
            }
            if (key == KeyboardKey.Space && !_isSwordSwinging)
            {
                _isSwordSwinging = true;
                _swordSwingTime = 0;
            }
        }
        
        // Handle movement with direct key state checks
        // This allows for continuous movement when keys are held down
        
        // Add a small delay to control movement speed
        const float moveDelay = 0.1f; // seconds between moves
        
        // Update time since last move
        _timeSinceLastMove += Raylib.GetFrameTime();
        
        // Only move if enough time has passed
        if (_timeSinceLastMove >= moveDelay)
        {
            bool moved = false;
            
            // Check WASD keys
            if (Raylib.IsKeyDown(KeyboardKey.W))
            {
                _animPlayerY = Math.Max(0, _animPlayerY - 1);
                _lastDirection = Direction.Up;
                moved = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.S))
            {
                _animPlayerY = Math.Min(9, _animPlayerY + 1);
                _lastDirection = Direction.Down;
                moved = true;
            }
            
            if (Raylib.IsKeyDown(KeyboardKey.A))
            {
                _animPlayerX = Math.Max(0, _animPlayerX - 1);
                _lastDirection = Direction.Left;
                moved = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.D))
            {
                _animPlayerX = Math.Min(19, _animPlayerX + 1);
                _lastDirection = Direction.Right;
                moved = true;
            }
            
            // Reset timer if moved
            if (moved)
            {
                _timeSinceLastMove = 0;
            }
        }
        
        // Update sword swing animation
        if (_isSwordSwinging)
        {
            _swordSwingTime += Raylib.GetFrameTime();
            if (_swordSwingTime >= SwordSwingDuration)
            {
                _isSwordSwinging = false;
            }
        }
        
        // Update enemy movement
        UpdateEnemy();
        
        // Update invincibility timer
        if (_isInvincible)
        {
            _invincibilityTimer += Raylib.GetFrameTime();
            if (_invincibilityTimer >= InvincibilityDuration)
            {
                _isInvincible = false;
                _invincibilityTimer = 0f;
            }
        }
    }

    private void UpdateEnemy()
    {
        // Update enemy spawn timer
        _enemySpawnTimer += Raylib.GetFrameTime();
        
        // Count living enemies
        int livingEnemies = _enemies.Count(e => e.Alive);
        
        // Spawn new enemy if timer expired and we're under the max living enemies
        if (_enemySpawnTimer >= EnemySpawnDelay && livingEnemies < MaxEnemies)
        {
            // Find a position that's not occupied by the player or another enemy
            int newX, newY;
            bool validPosition;
            
            do {
                newX = _random.Next(20);  // 0-19
                newY = _random.Next(10);  // 0-9
                
                validPosition = (newX != _animPlayerX || newY != _animPlayerY) && 
                               !_enemies.Any(e => e.Alive && e.X == newX && e.Y == newY);
            } while (!validPosition);
            
            _enemies.Add(new Enemy { X = newX, Y = newY });
            _enemySpawnTimer = 0;
        }
        
        // Update each enemy with its own timer
        float frameTime = Raylib.GetFrameTime();
        foreach (var enemy in _enemies)
        {
            if (!enemy.Alive) continue;  // Skip dead enemies
            
            // Update this enemy's individual timer
            enemy.MoveTimer += frameTime;
            
            if (enemy.MoveTimer >= EnemyMoveDelay)
            {
                // Choose a random direction (0-3)
                int direction = _random.Next(4);
                
                int newX = enemy.X;
                int newY = enemy.Y;
                
                switch (direction)
                {
                    case 0: // Up
                        newY = Math.Max(0, enemy.Y - 1);
                        break;
                    case 1: // Right
                        newX = Math.Min(19, enemy.X + 1);
                        break;
                    case 2: // Down
                        newY = Math.Min(9, enemy.Y + 1);
                        break;
                    case 3: // Left
                        newX = Math.Max(0, enemy.X - 1);
                        break;
                }
                
                // Only move if the new position is not occupied by another enemy or the player
                bool positionOccupied = (newX == _animPlayerX && newY == _animPlayerY) ||
                                       _enemies.Any(e => e != enemy && e.Alive && e.X == newX && e.Y == newY);
                
                if (!positionOccupied)
                {
                    enemy.X = newX;
                    enemy.Y = newY;
                }
                
                // Reset this enemy's timer
                enemy.MoveTimer = 0;
            }
        }

        // Check for sword collision with any enemy
        if (_isSwordSwinging)
        {
            // Define the collision area based on the direction
            List<(int x, int y)> collisionPoints = new List<(int x, int y)>();
            
            switch (_lastDirection)
            {
                case Direction.Left:
                    // Check left, top-left, and bottom-left
                    collisionPoints.Add((_animPlayerX - 1, _animPlayerY));     // Left
                    collisionPoints.Add((_animPlayerX - 1, _animPlayerY - 1)); // Top-left
                    collisionPoints.Add((_animPlayerX - 1, _animPlayerY + 1)); // Bottom-left
                    break;
                    
                case Direction.Right:
                    // Check right, top-right, and bottom-right
                    collisionPoints.Add((_animPlayerX + 1, _animPlayerY));     // Right
                    collisionPoints.Add((_animPlayerX + 1, _animPlayerY - 1)); // Top-right
                    collisionPoints.Add((_animPlayerX + 1, _animPlayerY + 1)); // Bottom-right
                    break;
                    
                case Direction.Up:
                    // Check up, top-left, and top-right
                    collisionPoints.Add((_animPlayerX, _animPlayerY - 1));     // Up
                    collisionPoints.Add((_animPlayerX - 1, _animPlayerY - 1)); // Top-left
                    collisionPoints.Add((_animPlayerX + 1, _animPlayerY - 1)); // Top-right
                    break;
                    
                case Direction.Down:
                    // Check down, bottom-left, and bottom-right
                    collisionPoints.Add((_animPlayerX, _animPlayerY + 1));     // Down
                    collisionPoints.Add((_animPlayerX - 1, _animPlayerY + 1)); // Bottom-left
                    collisionPoints.Add((_animPlayerX + 1, _animPlayerY + 1)); // Bottom-right
                    break;
            }

            // Check if any collision point matches any enemy position
            foreach (var enemy in _enemies)
            {
                if (enemy.Alive && collisionPoints.Any(p => p.x == enemy.X && p.y == enemy.Y))
                {
                    enemy.Alive = false;
                }
            }
        }

        // Check for player-enemy collisions
        foreach (var enemy in _enemies)
        {
            if (enemy.Alive && _animPlayerX == enemy.X && _animPlayerY == enemy.Y)
            {
                // Player and enemy are in the same position
                if (!_isInvincible)
                {
                    // Take damage
                    _currentHealth = Math.Max(0, _currentHealth - 1);
                    
                    // If health reaches zero, reset to full
                    if (_currentHealth == 0)
                    {
                        _currentHealth = _maxHealth;
                    }
                    
                    // Become invincible
                    _isInvincible = true;
                    _invincibilityTimer = 0f;
                    
                    // Determine knockback direction based on enemy position relative to player's movement
                    // Calculate which direction the enemy hit the player from
                    Direction knockbackDirection;
                    
                    // If player was moving, assume they were hit from the direction they were moving
                    knockbackDirection = _lastDirection;
                    
                    // Apply knockback in the opposite direction of the hit
                    switch (knockbackDirection)
                    {
                        case Direction.Left:
                            // If hit from left, knock right
                            _animPlayerX = Math.Min(19, _animPlayerX + 4);  // Doubled from 2 to 4
                            break;
                        case Direction.Right:
                            // If hit from right, knock left
                            _animPlayerX = Math.Max(0, _animPlayerX - 4);  // Doubled from 2 to 4
                            break;
                        case Direction.Up:
                            // If hit from above, knock down
                            _animPlayerY = Math.Min(9, _animPlayerY + 4);  // Doubled from 2 to 4
                            break;
                        case Direction.Down:
                            // If hit from below, knock up
                            _animPlayerY = Math.Max(0, _animPlayerY - 4);  // Doubled from 2 to 4
                            break;
                    }
                }
            }
        }
    }

    private void DrawText(string text, int x, int y, Color color)
    {
        Raylib.DrawTextEx(_menuFont, text, new Vector2(x, y), MenuFontSize, 1, color);
    }

    private void DrawCharacter(int charNum, int x, int y, Color color, bool showBorder = false)
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
        
        if(showBorder)
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

    private void DrawHealthBar()
    {
        const int heartChar = 3;  // ASCII/CP437 code for heart symbol (♥)
        const int heartSpacing = 30;  // Pixels between hearts
        const int startX = 20;
        const int startY = 20;
        
        for (int i = 0; i < _maxHealth; i++)
        {
            // Determine if this heart should be filled or empty
            Color heartColor = (i < _currentHealth) ? _healthColor : _emptyHealthColor;
            
            // Draw the heart
            DrawCharacter(heartChar, startX + (i * heartSpacing), startY, heartColor);
        }
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