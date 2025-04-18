using System.Numerics;
using Raylib_cs;

namespace Rogue;

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
    private const int Width = 40;
    private const int Height = 16;

    private GameView _currentView = GameView.Menu;
    private readonly Queue<KeyboardKey> _keyEvents = new();

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
    private readonly Color _backgroundColor = new(40, 44, 52, 255);  // #282c34

    private readonly Color[] _colors = new[]
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
    private float _timeSinceLastMove;

    // Add these fields to track direction and sword state
    private Direction _lastDirection = Direction.Right;
    private bool _isSwordSwinging;
    private float _swordSwingTime;
    private const float SwordSwingDuration = 0.25f;  // Reduced from 0.3f to make animation even faster

    // Add a silvery-blue color for the sword
    private readonly Color _swordColor = new(180, 210, 230, 255);  // Silvery-blue color

    private const float EnemyMoveDelay = 0.8f;  // Move every 0.8 seconds
    private readonly Random _random = new();

    // Multiple enemies support
    private readonly List<Enemy> _enemies = [];
    private float _enemySpawnTimer;
    private const float EnemySpawnDelay = 1.0f;  // Spawn a new enemy every 1 second
    private const int MaxEnemies = 3;  // Maximum number of enemies

    // Add health fields
    private readonly int _maxHealth = 10;
    private int _currentHealth = 7;  // Start with 7 out of 10 health
    private readonly Color _healthColor = Color.Red;
    private readonly Color _emptyHealthColor = new(100, 100, 100, 255);  // Gray color for empty hearts

    // Add invincibility fields
    private bool _isInvincible = false;
    private float _invincibilityTimer = 0f;
    private const float InvincibilityDuration = 1.0f;  // 1 second of invincibility after taking damage

    // Add these fields for explosion animation
    private readonly List<Explosion> _explosions = [];
    private const float ExplosionDuration = 0.5f;  // How long each explosion lasts
    private readonly Color _explosionColor = new(255, 165, 0, 255);  // Orange color for explosions

    // Add gold field
    private int _playerGold = 0;
    private readonly Color _goldColor = new(255, 215, 0, 255);  // Gold color

    // Add gold items field
    private readonly List<GoldItem> _goldItems = [];
    private const int MaxGoldItems = 3;  // Reduced from 5 to 3
    private const char GoldChar = '$';   // Character to represent gold

    // Add flying gold animation fields
    private readonly List<FlyingGold> _flyingGold = [];
    private const float GoldFlyDuration = 0.3f;  // Reduced from 0.5f to 0.3f for faster animation

    // Replace CRT effect fields with shader-related fields
    private bool _enableCrtEffect = true;
    private Shader _crtShader;
    private RenderTexture2D _gameTexture;
    private int _resolutionLoc;
    private int _curvatureLoc;
    private int _scanlineLoc;
    private int _vignetteLoc;
    private int _brightnessLoc;
    private int _distortionLoc;
    private int _flickerLoc;
    private int _timeLoc;
    private float _shaderTime = 0f;

    private readonly IRayLoader _rayLoader;

    public ScreenPresenter(IRayLoader rayLoader)
    {
        _rayLoader = rayLoader;

        Raylib.InitWindow(Width * CharWidth * DisplayScale, Height * CharHeight * DisplayScale, "Rogue-like");
        Raylib.SetTargetFPS(60);

        _charset = _rayLoader.LoadCharsetTexture();
        _menuFont = _rayLoader.LoadRobotoFont();

        // Initialize CRT shader
        InitCrtShader();

        SpawnEnemy();
        
        // Spawn initial gold items
        for (int i = 0; i < MaxGoldItems; i++)
        {
            SpawnGoldItem();
        }
    }

    private void InitCrtShader()
    {
        // Create a render texture the size of the window
        _gameTexture = Raylib.LoadRenderTexture(Width * CharWidth * DisplayScale, Height * CharHeight * DisplayScale);
        
        // Load the CRT shader with absolute path
        // string shaderPath = Path.GetFullPath("resources/crt.fs");
        var shaderPath = "resources/crt.fs";
        
        // Check if the shader file exists
        if (!File.Exists(shaderPath))
        {
            string errorMessage = $"Shader file not found at path: {shaderPath}";
            Console.WriteLine(errorMessage);
            throw new FileNotFoundException(errorMessage, shaderPath);
        }
        
        Console.WriteLine($"Shader file found at: {shaderPath}");
        Console.WriteLine($"Loading shader from: {shaderPath}");
        _crtShader = Raylib.LoadShader(null, shaderPath);
        
        // Get shader uniform locations
        _resolutionLoc = Raylib.GetShaderLocation(_crtShader, "resolution");
        _curvatureLoc = Raylib.GetShaderLocation(_crtShader, "curvature");
        _scanlineLoc = Raylib.GetShaderLocation(_crtShader, "scanlineIntensity");
        _vignetteLoc = Raylib.GetShaderLocation(_crtShader, "vignetteIntensity");
        _brightnessLoc = Raylib.GetShaderLocation(_crtShader, "brightness");
        _distortionLoc = Raylib.GetShaderLocation(_crtShader, "distortion");
        _flickerLoc = Raylib.GetShaderLocation(_crtShader, "flickerIntensity");
        _timeLoc = Raylib.GetShaderLocation(_crtShader, "time");
        
        // Set initial uniform values
        float[] resolution = { Width * CharWidth * DisplayScale, Height * CharHeight * DisplayScale };
        Raylib.SetShaderValue(_crtShader, _resolutionLoc, resolution, ShaderUniformDataType.Vec2);
        Raylib.SetShaderValue(_crtShader, _curvatureLoc, 0.1f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _scanlineLoc, 1.5f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _vignetteLoc, 0.2f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _brightnessLoc, 1.1f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _distortionLoc, 0.05f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _flickerLoc, 0.03f, ShaderUniformDataType.Float);

        // After loading the shader
        if (_crtShader.Id == 0)
        {
            Console.WriteLine("Failed to load CRT shader!");
        }
        else
        {
            Console.WriteLine("CRT shader loaded successfully with ID: " + _crtShader.Id);
            // Check if shader compiled successfully
            int result = Raylib.GetShaderLocation(_crtShader, "nonexistent_uniform");
            Console.WriteLine("Shader test result: " + result);
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
        // Update shader time
        _shaderTime += Raylib.GetFrameTime();
        Raylib.SetShaderValue(_crtShader, _timeLoc, _shaderTime, ShaderUniformDataType.Float);
        
        // Draw game to render texture
        Raylib.BeginTextureMode(_gameTexture);
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
        
        Raylib.EndTextureMode();
        
        // Draw render texture to screen with shader
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        
        if (_gameTexture.Id == 0)
        {
            Console.WriteLine("Render texture not created properly!");
        }
        
        if (_enableCrtEffect)
        {
            Console.WriteLine("Applying shader...");
            Raylib.BeginShaderMode(_crtShader);
            Raylib.DrawTextureRec(
                _gameTexture.Texture,
                new Rectangle(0, 0, _gameTexture.Texture.Width, -_gameTexture.Texture.Height),
                new Vector2(0, 0),
                Color.White
            );
            Raylib.EndShaderMode();
        }
        else
        {
            // Draw without shader if effect is disabled
            Raylib.DrawTextureRec(
                _gameTexture.Texture,
                new Rectangle(0, 0, _gameTexture.Texture.Width, -_gameTexture.Texture.Height),
                new Vector2(0, 0),
                Color.White
            );
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
            // Toggle CRT effect with T key
            if (key == KeyboardKey.T)
            {
                _enableCrtEffect = !_enableCrtEffect;
                break;
            }
        }
    }

    private void DrawCharacterSet()
    {
        // Draw all characters in a grid
        for (var charNum = 0; charNum < 256; charNum++)
        {
            var row = charNum / 32;
            var col = charNum % 32;

            DrawCharacter(
                charNum,
                20 + (col * 40),
                20 + (row * 60),
                _colors[charNum % _colors.Length]
            );
        }

        DrawText("Press any key to return", 20, Height * CharHeight * DisplayScale - 40, Color.White);
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
        
        // Draw gold counter
        DrawGoldCounter();

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
                // Draw gold items
                else if (_goldItems.Any(g => g.X == x && g.Y == y))
                {
                    DrawCharacter(GoldChar, 100 + x * 40, 100 + y * 40, _goldColor);
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
        
        // Draw explosions (after ground and enemies, but before sword)
        foreach (var explosion in _explosions)
        {
            char explosionChar = explosion.Frame switch
            {
                0 => '*',      // Small explosion
                1 => (char)15, // Medium explosion (sun symbol in CP437)
                _ => (char)42  // Large explosion (asterisk)
            };
            
            DrawCharacter(explosionChar, 100 + explosion.X * 40, 100 + explosion.Y * 40, _explosionColor);
        }

        // Draw sword if swinging (drawn after ground to appear on top)
        if (_isSwordSwinging)
        {
            // Calculate animation progress (0.0 to 1.0)
            var progress = _swordSwingTime / SwordSwingDuration;
            if (progress > 1.0f) progress = 1.0f;

            // Calculate frame (0, 1, or 2)
            var frame = (int)(progress * 3);
            if (frame > 2) frame = 2;

            // Calculate fractional position
            var xOffset = 0f;
            var yOffset = 0f;

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

        // Draw flying gold (after everything else so it appears on top)
        foreach (var gold in _flyingGold)
        {
            // Calculate progress (0.0 to 1.0)
            float progress = gold.Timer / GoldFlyDuration;
            if (progress > 1.0f) progress = 1.0f;
            
            // Calculate current position using easing function
            // Using a quadratic ease-out for smooth deceleration
            progress = 1 - (1 - progress) * (1 - progress);
            
            // Calculate end position (gold counter location)
            int screenWidth = Width * CharWidth * DisplayScale;
            string goldText = $"Gold: {_playerGold}";
            int goldTextWidth = Raylib.MeasureText(goldText, MenuFontSize);
            int endX = screenWidth - goldTextWidth - 40;  // Position before the text
            int endY = 20;  // Same Y as gold counter
            
            // Interpolate between start and end positions
            int currentX = (int)(gold.StartX + (endX - gold.StartX) * progress);
            int currentY = (int)(gold.StartY + (endY - gold.StartY) * progress);
            
            // Calculate alpha (opacity) based on progress - fade out as it approaches the counter
            byte alpha = (byte)(255 * (1.0f - progress * 0.8f));  // Fade to 20% opacity
            Color fadingGoldColor = new Color(_goldColor.R, _goldColor.G, _goldColor.B, alpha);
            
            // Draw the flying gold character with fading effect
            DrawCharacter(GoldChar, currentX, currentY, fadingGoldColor);
        }

        DrawText("Use WASD to move, SPACE to swing sword, ESC to return to menu", 20, Height * CharHeight * DisplayScale - 40, Color.White);
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
                
                // Check for gold collection after movement
                CollectGold();
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

        // Update explosions
        for (int i = _explosions.Count - 1; i >= 0; i--)
        {
            _explosions[i].Timer += Raylib.GetFrameTime();
            if (_explosions[i].Timer >= ExplosionDuration)
            {
                // Spawn gold at the explosion location when the explosion finishes
                _goldItems.Add(new GoldItem { 
                    X = _explosions[i].X, 
                    Y = _explosions[i].Y, 
                    Value = _random.Next(3, 8)  // Enemies drop more valuable gold (3-7)
                });
                
                // Remove the explosion
                _explosions.RemoveAt(i);
            }
        }

        // Update flying gold animations
        for (int i = _flyingGold.Count - 1; i >= 0; i--)
        {
            _flyingGold[i].Timer += Raylib.GetFrameTime();
            if (_flyingGold[i].Timer >= GoldFlyDuration)
            {
                // Add the gold value to player's total when animation completes
                _playerGold += _flyingGold[i].Value;
                
                // Remove the flying gold
                _flyingGold.RemoveAt(i);
            }
        }
    }

    private void CollectGold()
    {
        // Find any gold at the player's position
        for (int i = _goldItems.Count - 1; i >= 0; i--)
        {
            if (_goldItems[i].X == _animPlayerX && _goldItems[i].Y == _animPlayerY)
            {
                // Create flying gold animation
                _flyingGold.Add(new FlyingGold { 
                    StartX = 100 + _goldItems[i].X * 40,
                    StartY = 100 + _goldItems[i].Y * 40,
                    Value = _goldItems[i].Value,
                    Timer = 0f
                });
                
                // Remove the collected gold from the map
                _goldItems.RemoveAt(i);
                
                // Only collect one gold piece per move (in case multiple end up in same spot)
                break;
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
            SpawnEnemy();
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
                    
                    // Create explosion at enemy position
                    _explosions.Add(new Explosion { 
                        X = enemy.X, 
                        Y = enemy.Y, 
                        Timer = 0f 
                    });
                    
                    // Gold will be spawned when the explosion finishes
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

    private void SpawnEnemy()
    {
        // Find a position that's not occupied by the player or another enemy
        int newX = 0;
        int newY = 0;
        bool isPositionValid = false;

        const int maxAttempts = 10;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            newX = _random.Next(20);  // 0-19
            newY = _random.Next(10);  // 0-9

            isPositionValid = (newX != _animPlayerX || newY != _animPlayerY) &&
                            !_enemies.Any(e => e.Alive && e.X == newX && e.Y == newY);

            if (isPositionValid)
                break;
        }

        if (!isPositionValid)
            return;

        _enemies.Add(new Enemy { X = newX, Y = newY });
        _enemySpawnTimer = 0;
    }

    private void SpawnGoldItem()
    {
        // Find a position that's not occupied by the player, enemies, or other gold
        int newX = 0;
        int newY = 0;
        bool isPositionValid = false;

        const int maxAttempts = 10;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            newX = _random.Next(20);  // 0-19
            newY = _random.Next(10);  // 0-9

            isPositionValid = (newX != _animPlayerX || newY != _animPlayerY) &&
                            !_enemies.Any(e => e.Alive && e.X == newX && e.Y == newY) &&
                            !_goldItems.Any(g => g.X == newX && g.Y == newY);

            if (isPositionValid)
                break;
        }

        if (!isPositionValid)
            return;

        _goldItems.Add(new GoldItem { X = newX, Y = newY, Value = _random.Next(1, 6) });  // Gold worth 1-5
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

        if (showBorder)
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

    private void DrawGoldCounter()
    {
        // Calculate position for top right placement
        int screenWidth = Width * CharWidth * DisplayScale;
        string goldText = $"Gold: {_playerGold}";
        int goldTextWidth = Raylib.MeasureText(goldText, MenuFontSize);
        
        // Position the gold counter at the top right with some padding
        int startX = screenWidth - goldTextWidth - 20;  // 20px padding from right edge
        const int startY = 20;  // Same vertical position as health
        
        // Draw gold text
        DrawText(goldText, startX, startY, _goldColor);
    }

    public bool WindowShouldClose()
    {
        return Raylib.WindowShouldClose();
    }

    public void Cleanup()
    {
        // Call the existing Dispose method to handle cleanup
        Dispose();
    }

    public void Dispose()
    {
        // Unload shader resources
        Raylib.UnloadShader(_crtShader);
        Raylib.UnloadRenderTexture(_gameTexture);
        
        // Existing cleanup code
        Raylib.UnloadTexture(_charset);
        Raylib.UnloadFont(_menuFont);
        Raylib.CloseWindow();
    }

    private class Explosion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public float Timer { get; set; }
        public int Frame => (int)((Timer / ExplosionDuration) * 3);  // 3 frames of animation
    }

    private class GoldItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Value { get; set; }  // How much this gold is worth
    }

    private class FlyingGold
    {
        public int StartX { get; set; }  // Starting X position in pixels
        public int StartY { get; set; }  // Starting Y position in pixels
        public int Value { get; set; }   // How much this gold is worth
        public float Timer { get; set; } // Animation timer
    }
}