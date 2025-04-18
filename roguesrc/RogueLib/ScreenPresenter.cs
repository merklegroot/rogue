using System.Numerics;
using Raylib_cs;

namespace RogueLib;

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

    private GameScreenEnum _currentScreenEnum = GameScreenEnum.Menu;
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
    private int _scanlineCountLoc;

    // Add health pickup fields
    private readonly List<HealthPickup> _healthPickups = [];
    private float _timeSinceLastHealthSpawn = 0f;
    private const float HealthSpawnInterval = 30f;  // Spawn a health pickup every 30 seconds
    private const char HealthChar = (char)3;  // ASCII/CP437 heart symbol (♥)

    // Add sword cooldown fields
    private float _swordCooldownTimer = 0f;
    private float _swordCooldown = 1.0f;  // Changed from const to field
    private bool _swordOnCooldown = false;

    // Add the _swordReach field
    private int _swordReach = 1;  // Default sword reach

    // Add these fields for crossbow functionality
    private bool _hasCrossbow = false;
    private bool _isCrossbowFiring = false;
    private float _crossbowCooldown = 2.0f;
    private float _crossbowCooldownTimer = 0f;
    private bool _crossbowOnCooldown = false;
    private readonly List<CrossbowBolt> _crossbowBolts = [];
    private const float BoltSpeed = 8.0f; // Bolts move 8 tiles per second
    private const char BoltChar = '-';    // Character to represent horizontal bolt
    private readonly Color _boltColor = new(210, 180, 140, 255); // Light brown color for bolts

    // Add these fields for tracking enemy kills and the charger
    private int _enemiesKilled = 0;
    private const int KillsForCharger = 10;
    private bool _chargerActive = false;
    private ChargerEnemy? _charger = null;
    private const float ChargerSpeed = 0.3f; // Charger moves faster than regular enemies
    private const char ChargerChar = (char)2; // ASCII/CP437 smiley face (☻)
    private readonly Color _chargerColor = new(255, 50, 50, 255); // Bright red color
    private const int ChargerHealth = 5; // Define charger health as a constant

    // Add fields for player idle animation
    private float _playerIdleAnimTimer = 0f;
    private int _playerIdleAnimFrame = 0;
    private const int PlayerIdleFrameCount = 4;
    private const float PlayerIdleFrameDuration = 0.25f; // 4 frames per second

    // Add a single field for player animation
    private int _playerChar = 2; // Default player character

    // Add this method to update the player animation
    private void UpdatePlayerAnimation()
    {
        // Simple animation: change character every 30 frames (about 0.5 seconds)
        if ((int)(Raylib.GetTime() * 60) % 30 == 0)
        {
            // Toggle between character 2 and 1
            _playerChar = (_playerChar == 2) ? 1 : 2;
        }
    }

    // Add this method to get the current player character
    private int GetPlayerChar()
    {
        return _playerChar;
    }

    // Define ShopItem class before it's used
    private class ShopItem
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int Price { get; set; }
        public required Action OnPurchase { get; set; }
        public ShopCategory Category { get; set; } = ShopCategory.Consumable;  // Default category
    }

    // Add shop-related fields
    private bool _shopOpen = false;
    private int _selectedShopItem = 0;
    private readonly List<ShopItem> _shopInventory = [];

    private readonly IRayLoader _rayLoader;

    public ScreenPresenter(IRayLoader rayLoader)
    {
        _rayLoader = rayLoader;

        // Disable the default ESC key behavior that closes the window
        Raylib.SetExitKey(KeyboardKey.Null);  // This disables the ESC key from closing the window

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

        // Initialize shop inventory
        InitializeShop();
    }

    private void InitCrtShader()
    {
        // Create a render texture the size of the window
        _gameTexture = Raylib.LoadRenderTexture(Width * CharWidth * DisplayScale, Height * CharHeight * DisplayScale);
        
        // Load the CRT shader
        _crtShader = _rayLoader.LoadCrtShader();
        
        // Get shader uniform locations
        _resolutionLoc = Raylib.GetShaderLocation(_crtShader, "resolution");
        _curvatureLoc = Raylib.GetShaderLocation(_crtShader, "curvature");
        _scanlineLoc = Raylib.GetShaderLocation(_crtShader, "scanlineIntensity");
        _scanlineCountLoc = Raylib.GetShaderLocation(_crtShader, "scanlineCount");
        _vignetteLoc = Raylib.GetShaderLocation(_crtShader, "vignetteIntensity");
        _brightnessLoc = Raylib.GetShaderLocation(_crtShader, "brightness");
        _distortionLoc = Raylib.GetShaderLocation(_crtShader, "distortion");
        _flickerLoc = Raylib.GetShaderLocation(_crtShader, "flickerIntensity");
        _timeLoc = Raylib.GetShaderLocation(_crtShader, "time");
        
        // Set initial uniform values
        float[] resolution = { Width * CharWidth * DisplayScale, Height * CharHeight * DisplayScale };
        Raylib.SetShaderValue(_crtShader, _resolutionLoc, resolution, ShaderUniformDataType.Vec2);
        
        // Fix the extreme warping by drastically reducing curvature
        Raylib.SetShaderValue(_crtShader, _curvatureLoc, 0.05f, ShaderUniformDataType.Float);  // Decreased from 0.5 to 0.3
        Raylib.SetShaderValue(_crtShader, _scanlineLoc, 0.1f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _scanlineCountLoc, 240.0f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _vignetteLoc, 0.1f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _brightnessLoc, 1.05f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _distortionLoc, 0.002f, ShaderUniformDataType.Float);
        Raylib.SetShaderValue(_crtShader, _flickerLoc, 0.01f, ShaderUniformDataType.Float);
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

        switch (_currentScreenEnum)
        {
            case GameScreenEnum.Menu:
                DrawMenu();
                HandleMenuInput();
                break;

            case GameScreenEnum.CharacterSet:
                DrawCharacterSet();
                HandleCharacterSetInput();
                break;

            case GameScreenEnum.Animation:
                DrawAnimation();
                HandleAnimationInput();
                break;

            case GameScreenEnum.Shop:
                DrawShop();
                HandleShopInput();
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
        // Draw debug grid to help with alignment
        DrawDebugGrid();
        
        // Draw a fancy title
        string title = "ROGUE ADVENTURE";
        int titleSize = 48;
        
        // Use MeasureTextEx instead of MeasureText to account for spacing
        Vector2 titleSize2D = Raylib.MeasureTextEx(_menuFont, title, titleSize, 1);
        int titleWidth = (int)titleSize2D.X;
        
        int centerX = (Width * CharWidth * DisplayScale) / 2;
        
        // Draw title with shadow effect
        Raylib.DrawTextEx(_menuFont, title, new Vector2(centerX - titleWidth/2 + 3, 40 + 3), titleSize, 1, new Color(30, 30, 30, 200));
        Raylib.DrawTextEx(_menuFont, title, new Vector2(centerX - titleWidth/2, 40), titleSize, 1, Color.Gold);
        
        // Draw a decorative line under the title - with correct width
        int lineY = 40 + titleSize + 10; // Position it directly under the title with a small gap
        int lineWidth = titleWidth - 20; // Make the line slightly shorter than the title text
        Raylib.DrawRectangle(centerX - lineWidth/2, lineY, lineWidth, 2, Color.Gold);
        
        // Draw center marker
        Raylib.DrawRectangle(centerX - 1, 0, 2, Height * CharHeight * DisplayScale, new Color(255, 0, 0, 100));
        
        // Draw menu options centered with more spacing
        int menuStartY = lineY + 30; // Start menu options below the line
        int menuSpacing = 60;
        
        DrawColoredHotkeyText("Start (A)dventure", centerX - 120, menuStartY);
        DrawColoredHotkeyText("View (C)haracter Set", centerX - 120, menuStartY + menuSpacing);
        DrawColoredHotkeyText("(T)oggle CRT Effect", centerX - 120, menuStartY + menuSpacing * 2);
        DrawColoredHotkeyText("e(X)it Game", centerX - 120, menuStartY + menuSpacing * 3);
        
        // Draw a version number and copyright
        string version = "v0.1 Alpha";
        DrawText(version, 20, Height * CharHeight * DisplayScale - 40, new Color(150, 150, 150, 200));
        
        // Draw a small decorative element
        DrawCharacter(2, centerX - 10, menuStartY + menuSpacing * 4, Color.White);
    }

    private void DrawDebugGrid()
    {
        int screenWidth = Width * CharWidth * DisplayScale;
        int screenHeight = Height * CharHeight * DisplayScale;
        
        // Draw vertical grid lines every 50 pixels
        for (int x = 0; x < screenWidth; x += 50)
        {
            Raylib.DrawLine(x, 0, x, screenHeight, new Color(100, 100, 100, 50));
            
            // Draw coordinate labels
            if (x % 100 == 0)
            {
                Raylib.DrawText(x.ToString(), x + 2, 2, 16, new Color(100, 100, 100, 150));
            }
        }
        
        // Draw horizontal grid lines every 50 pixels
        for (int y = 0; y < screenHeight; y += 50)
        {
            Raylib.DrawLine(0, y, screenWidth, y, new Color(100, 100, 100, 50));
            
            // Draw coordinate labels
            if (y % 100 == 0)
            {
                Raylib.DrawText(y.ToString(), 2, y + 2, 16, new Color(100, 100, 100, 150));
            }
        }
        
        // Draw a vertical line at the center of the screen
        int centerX = screenWidth / 2;
        Raylib.DrawLine(centerX, 0, centerX, screenHeight, new Color(255, 0, 0, 100));
        
        // Draw a horizontal line at the center of the screen
        int centerY = screenHeight / 2;
        Raylib.DrawLine(0, centerY, screenWidth, centerY, new Color(255, 0, 0, 100));
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

        // Calculate positions using MeasureTextEx for more accurate spacing
        var currentX = x;

        // Draw text before parenthesis
        var beforeText = text[..startParenIndex];
        DrawText(beforeText, currentX, y, options.BaseColor);
        
        // Use MeasureTextEx for more accurate width measurement
        Vector2 beforeSize = Raylib.MeasureTextEx(_menuFont, beforeText, MenuFontSize, 1);
        currentX += (int)beforeSize.X - 4;  // Reduce spacing before parenthesis

        // Draw opening parenthesis
        DrawText("(", currentX, y, options.BaseColor);
        Vector2 parenSize = Raylib.MeasureTextEx(_menuFont, "(", MenuFontSize, 1);
        currentX += (int)parenSize.X - 2;  // Tighter spacing after opening parenthesis

        // Draw hotkey in different color
        var hotkey = text[(startParenIndex + 1)..endParenIndex];
        DrawText(hotkey, currentX, y, options.HotkeyColor);
        Vector2 hotkeySize = Raylib.MeasureTextEx(_menuFont, hotkey, MenuFontSize, 1);
        currentX += (int)hotkeySize.X - 2;  // Tighter spacing after hotkey

        // Draw closing parenthesis
        DrawText(")", currentX, y, options.BaseColor);
        Vector2 closeParenSize = Raylib.MeasureTextEx(_menuFont, ")", MenuFontSize, 1);
        currentX += (int)closeParenSize.X - 2;  // Tighter spacing after closing parenthesis

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
                _currentScreenEnum = GameScreenEnum.CharacterSet;
                break;
            }
            if (key == KeyboardKey.A)
            {
                _currentScreenEnum = GameScreenEnum.Animation;
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
            _currentScreenEnum = GameScreenEnum.Menu;
        }
    }

    private void DrawAnimation()
    {
        UpdatePlayerIdleAnimation();
        DrawHealthBar();
        DrawGoldCounter();
        DrawWorld();
        DrawExplosions();
        DrawSwordAnimation();
        DrawFlyingGold();
        DrawCooldownIndicators();
        DrawCrossbowBolts();
        DrawChargerHealth();
        DrawInstructions();
    }

    private void UpdatePlayerIdleAnimation()
    {
        _playerIdleAnimTimer += Raylib.GetFrameTime();
        if (_playerIdleAnimTimer >= PlayerIdleFrameDuration)
        {
            _playerIdleAnimTimer -= PlayerIdleFrameDuration;
            _playerIdleAnimFrame = (_playerIdleAnimFrame + 1) % PlayerIdleFrameCount;
        }
    }

    private void DrawExplosions()
    {
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
    }

    private void DrawSwordAnimation()
    {
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
    }

    private void DrawFlyingGold()
    {
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
    }

    private void DrawCooldownIndicators()
    {
        // Draw sword cooldown indicator
        if (_swordOnCooldown)
        {
            // Calculate cooldown progress (0.0 to 1.0)
            float progress = _swordCooldownTimer / _swordCooldown;
            
            // Draw a small cooldown bar above the player
            int barWidth = 30;
            int barHeight = 5;
            int barX = 100 + _animPlayerX * 40 - barWidth / 2 + 20;  // Center above player
            int barY = 100 + _animPlayerY * 40 - 15;  // Above player
            
            // Background (empty) bar
            Raylib.DrawRectangle(barX, barY, barWidth, barHeight, new Color(50, 50, 50, 200));
            
            // Foreground (filled) bar - grows as cooldown progresses
            Raylib.DrawRectangle(barX, barY, (int)(barWidth * progress), barHeight, new Color(200, 200, 200, 200));
        }
        
        // Draw crossbow cooldown indicator if player has crossbow
        if (_hasCrossbow && _crossbowOnCooldown)
        {
            // Calculate cooldown progress (0.0 to 1.0)
            float progress = _crossbowCooldownTimer / _crossbowCooldown;
            
            // Draw a small cooldown bar below the player
            int barWidth = 30;
            int barHeight = 5;
            int barX = 100 + _animPlayerX * 40 - barWidth / 2 + 20;  // Center below player
            int barY = 100 + _animPlayerY * 40 + 45;  // Below player
            
            // Background (empty) bar
            Raylib.DrawRectangle(barX, barY, barWidth, barHeight, new Color(50, 50, 50, 200));
            
            // Foreground (filled) bar - grows as cooldown progresses
            Raylib.DrawRectangle(barX, barY, (int)(barWidth * progress), barHeight, new Color(150, 150, 200, 200));
        }
    }

    private void DrawCrossbowBolts()
    {
        foreach (var bolt in _crossbowBolts)
        {
            // Choose character based on direction
            char boltChar = bolt.Direction switch
            {
                Direction.Left or Direction.Right => '-',
                Direction.Up or Direction.Down => '|',
                _ => '+'
            };
            
            // Draw the bolt at its current position
            DrawCharacter(boltChar, 100 + (int)(bolt.X * 40), 100 + (int)(bolt.Y * 40), _boltColor);
        }
    }

    private void DrawChargerHealth()
    {
        // Draw charger health if active
        if (_chargerActive && _charger != null && _charger.Alive)
        {
            string healthText = $"Charger HP: {_charger.Health}/{ChargerHealth} (Hit {_charger.HitCount} times)";
            DrawText(healthText, 20, 60, Color.Red);
        }
    }

    private void DrawInstructions()
    {
        // Move the instructions text up by 20 pixels
        string instructionsText = "Use WASD to move, SPACE to swing sword";
        if (_hasCrossbow)
        {
            instructionsText += ", F to fire crossbow";
        }
        instructionsText += ", ESC to return to menu, (G) for debug gold";
        
        // Changed from Height * CharHeight * DisplayScale - 40 to Height * CharHeight * DisplayScale - 60
        DrawText(instructionsText, 20, Height * CharHeight * DisplayScale - 60, Color.White);
    }

    private void DrawWorld()
    {
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 20; x++)
            {
                // Draw player with idle animation
                if (Math.Abs(x - _animPlayerX) < 0.5f && Math.Abs(y - _animPlayerY) < 0.5f)
                {
                    // Simple animation: toggle between character 2 and 1 every 0.5 seconds
                    int playerChar = (Raylib.GetTime() % 1 < 0.5) ? 2 : 1;
                    DrawCharacter(playerChar, 100 + x * 40, 100 + y * 40, Color.White);
                    continue;
                }
                
                // Draw charger
                if (_chargerActive && _charger != null && _charger.Alive && 
                         Math.Abs(x - _charger.X) < 0.5f && Math.Abs(y - _charger.Y) < 0.5f)
                {
                    Color chargerColor = _chargerColor;
                    if (_charger.IsInvincible && (int)(_charger.InvincibilityTimer * 10) % 2 == 0)
                    {
                        chargerColor = new Color((byte)_chargerColor.R, (byte)_chargerColor.G, (byte)_chargerColor.B, (byte)128);
                    }
                    DrawCharacter(ChargerChar, 100 + x * 40, 100 + y * 40, chargerColor);
                }
                // Draw health pickups
                else if (_healthPickups.Any(h => h.X == x && h.Y == y))
                {
                    DrawCharacter(HealthChar, 100 + x * 40, 100 + y * 40, _healthColor);
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
    }
    
    private void HandleAnimationInput()
    {
        // Handle ESC key via event queue for menu navigation
        while (_keyEvents.Count > 0)
        {
            var key = _keyEvents.Dequeue();
            if (key == KeyboardKey.Escape)
            {
                _currentScreenEnum = GameScreenEnum.Menu;
                return;
            }
            if (key == KeyboardKey.Space && !_isSwordSwinging && !_swordOnCooldown)
            {
                _isSwordSwinging = true;
                _swordSwingTime = 0;
            }
            // Add debug option to get free gold with G key
            if (key == KeyboardKey.G)
            {
                _playerGold += 100;
                
                // Optional: Add a visual indicator that gold was added
                int screenWidth = Width * CharWidth * DisplayScale;
                _flyingGold.Add(new FlyingGold { 
                    StartX = screenWidth / 2,
                    StartY = Height * CharHeight * DisplayScale / 2,
                    Value = 100,
                    Timer = 0f
                });
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

        // Update sword cooldown
        if (_swordOnCooldown)
        {
            _swordCooldownTimer += Raylib.GetFrameTime();
            if (_swordCooldownTimer >= _swordCooldown)
            {
                _swordOnCooldown = false;
                _swordCooldownTimer = 0f;
            }
        }

        // Handle sword swinging
        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !_isSwordSwinging && !_swordOnCooldown)
        {
            _isSwordSwinging = true;
            _swordSwingTime = 0;
        }

        // Update sword swing animation
        if (_isSwordSwinging)
        {
            _swordSwingTime += Raylib.GetFrameTime();
            if (_swordSwingTime >= SwordSwingDuration)
            {
                _isSwordSwinging = false;
                _swordOnCooldown = true;  // Start cooldown when swing finishes
                _swordCooldownTimer = 0f;
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

        // Update health pickup spawn timer
        _timeSinceLastHealthSpawn += Raylib.GetFrameTime();
        if (_timeSinceLastHealthSpawn >= HealthSpawnInterval)
        {
            SpawnHealthPickup();
            _timeSinceLastHealthSpawn = 0f;
        }
        
        // Check for health pickup collection
        CollectHealth();

        // Open shop with 'B' key
        if (Raylib.IsKeyPressed(KeyboardKey.B))
        {
            _shopOpen = true;
            _currentScreenEnum = GameScreenEnum.Shop;
            _selectedShopItem = 0;
        }

        // Handle crossbow firing with F key
        if (Raylib.IsKeyPressed(KeyboardKey.F) && _hasCrossbow && !_crossbowOnCooldown)
        {
            _isCrossbowFiring = true;
            _crossbowOnCooldown = true;
            _crossbowCooldownTimer = 0f;
            
            // Create a new bolt based on player direction
            float boltX = _animPlayerX;
            float boltY = _animPlayerY;
            Direction boltDirection = _lastDirection;
            
            _crossbowBolts.Add(new CrossbowBolt
            {
                X = boltX,
                Y = boltY,
                Direction = boltDirection,
                DistanceTraveled = 0f
            });
        }
        
        // Update crossbow cooldown
        if (_crossbowOnCooldown)
        {
            _crossbowCooldownTimer += Raylib.GetFrameTime();
            if (_crossbowCooldownTimer >= _crossbowCooldown)
            {
                _crossbowOnCooldown = false;
                _crossbowCooldownTimer = 0f;
            }
        }
        
        // Update crossbow bolts
        UpdateCrossbowBolts();

        // Check for collisions with enemies
        foreach (var enemy in _enemies)
        {
            if (enemy.Alive && !_isInvincible)
            {
                // Check if player is colliding with this enemy
                if (Math.Abs(_animPlayerX - enemy.X) < 0.5f && Math.Abs(_animPlayerY - enemy.Y) < 0.5f)
                {
                    // Player takes damage
                    _currentHealth--;
                    Console.WriteLine($"Player hit by enemy! Health: {_currentHealth}");
                    
                    // Make player invincible briefly
                    _isInvincible = true;
                    _invincibilityTimer = 0f;
                    
                    break; // Only take damage from one enemy per frame
                }
            }
        }
        
        // Check for collisions with charger (deals more damage)
        if (_chargerActive && _charger != null && _charger.Alive && !_isInvincible)
        {
            // Check if player is colliding with the charger
            if (Math.Abs(_animPlayerX - _charger.X) < 0.5f && Math.Abs(_animPlayerY - _charger.Y) < 0.5f)
            {
                // Player takes more damage from charger (2 instead of 1)
                _currentHealth -= 2;
                Console.WriteLine($"Player hit by charger! Health: {_currentHealth}");
                
                // Make player invincible briefly
                _isInvincible = true;
                _invincibilityTimer = 0f;
            }
        }
    }

    private void CollectGold()
    {
        // Find any gold within one square of the player's position
        for (int i = _goldItems.Count - 1; i >= 0; i--)
        {
            // Check if gold is at the player's position or one square away
            if (Math.Abs(_goldItems[i].X - _animPlayerX) <= 1 && 
                Math.Abs(_goldItems[i].Y - _animPlayerY) <= 1)
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

    private void CollectHealth()
    {
        // Find any health pickup within one square of the player's position
        for (int i = _healthPickups.Count - 1; i >= 0; i--)
        {
            // Check if health pickup is at the player's position or one square away
            if (Math.Abs(_healthPickups[i].X - _animPlayerX) <= 1 && 
                Math.Abs(_healthPickups[i].Y - _animPlayerY) <= 1)
            {
                // Add health to the player
                _currentHealth = Math.Min(_maxHealth, _currentHealth + _healthPickups[i].HealAmount);
                
                // Remove the collected health pickup
                _healthPickups.RemoveAt(i);
                
                // Only collect one health pickup per move
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
                                        _enemies.Any(e => e != enemy && e.Alive && e.X == newX && e.Y == newY) ||
                                        (_chargerActive && _charger != null && _charger.Alive && 
                                         Math.Abs(newX - _charger.X) < 0.5f && Math.Abs(newY - _charger.Y) < 0.5f);

                if (!positionOccupied)
                {
                    enemy.X = newX;
                    enemy.Y = newY;
                }

                // Reset this enemy's timer
                enemy.MoveTimer = 0;
            }
        }

        // Update charger if active
        if (_chargerActive && _charger != null && _charger.Alive)
        {
            _charger.MoveTimer += frameTime;
            
            if (_charger.MoveTimer >= ChargerSpeed) // Charger moves faster than regular enemies
            {
                // Charger always moves toward the player (unlike random movement of regular enemies)
                float dx = _animPlayerX - _charger.X;
                float dy = _animPlayerY - _charger.Y;
                
                // Determine primary direction to move (horizontal or vertical)
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    // Move horizontally
                    if (dx > 0)
                        _charger.X = Math.Min(19, _charger.X + 1);
                    else
                        _charger.X = Math.Max(0, _charger.X - 1);
                }
                else
                {
                    // Move vertically
                    if (dy > 0)
                        _charger.Y = Math.Min(9, _charger.Y + 1);
                    else
                        _charger.Y = Math.Max(0, _charger.Y - 1);
                }
                
                // Reset charger's timer
                _charger.MoveTimer = 0;
            }
            
            // Check for collision with player
            if (Math.Abs(_charger.X - _animPlayerX) < 0.5f && Math.Abs(_charger.Y - _animPlayerY) < 0.5f)
            {
                HandleChargerDamage(true); // true for sword
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
                    
                    // Increment kill counter
                    _enemiesKilled++;
                    
                    // Check if we should spawn a charger
                    if (_enemiesKilled >= KillsForCharger && !_chargerActive)
                    {
                        SpawnCharger();
                        _enemiesKilled = 0; // Reset kill counter
                    }
                    
                    // Create explosion at enemy position
                    _explosions.Add(new Explosion { 
                        X = enemy.X, 
                        Y = enemy.Y, 
                        Timer = 0f 
                    });
                    
                    // Gold will be spawned when the explosion finishes
                }
            }
            
            // Check for sword collision with charger
            if (_chargerActive && _charger != null && _charger.Alive)
            {
                if (collisionPoints.Any(p => Math.Abs(p.x - _charger.X) < 0.5f && Math.Abs(p.y - _charger.Y) < 0.5f))
                {
                    HandleChargerDamage(true); // true for sword
                }
            }
        }
        
        // Also check for crossbow bolt collision with charger
        foreach (var bolt in _crossbowBolts)
        {
            if (_chargerActive && _charger != null && _charger.Alive)
            {
                if (Math.Abs(bolt.X - _charger.X) < 0.5f && Math.Abs(bolt.Y - _charger.Y) < 0.5f)
                {
                    HandleChargerDamage(false); // false for bolt
                }
            }
        }

        // Update charger invincibility
        if (_chargerActive && _charger != null && _charger.Alive && _charger.IsInvincible)
        {
            _charger.InvincibilityTimer += Raylib.GetFrameTime();
            if (_charger.InvincibilityTimer >= 0.5f) // 0.5 seconds of invincibility
            {
                _charger.IsInvincible = false;
                Console.WriteLine("COLLISION FIX: Charger invincibility ended");
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

    private void SpawnHealthPickup()
    {
        // Find a position that's not occupied by the player, enemies, gold, or other health pickups
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
                            !_goldItems.Any(g => g.X == newX && g.Y == newY) &&
                            !_healthPickups.Any(h => h.X == newX && h.Y == newY);

            if (isPositionValid)
                break;
        }

        if (!isPositionValid)
            return;

        _healthPickups.Add(new HealthPickup { X = newX, Y = newY, HealAmount = 20 });  // Restore 20 health
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
        // Check if window close was requested (like clicking the X button)
        // but ignore if it was triggered by the Escape key
        bool closeRequested = Raylib.WindowShouldClose();
        
        // If close is requested and it's because of Escape key, ignore it
        if (closeRequested && Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            return false;
        }
        
        return closeRequested;
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

    private class HealthPickup
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int HealAmount { get; set; }  // How much health this pickup restores
    }

    private void InitializeShop()
    {
        // Regular items section
        _shopInventory.Add(new ShopItem
        {
            Name = "Health Potion",
            Description = "Restores 25 health",
            Price = 10,
            OnPurchase = () => { _currentHealth = Math.Min(10, _currentHealth + 25); },
            Category = ShopCategory.Consumable
        });
        
        _shopInventory.Add(new ShopItem
        {
            Name = "Faster Sword",
            Description = "Reduces sword cooldown by 0.2s",
            Price = 25,
            OnPurchase = () => { 
                if (_swordCooldown > 0.3f) // Don't let it go below 0.3s
                    _swordCooldown -= 0.2f; 
            },
            Category = ShopCategory.Upgrade
        });
        
        _shopInventory.Add(new ShopItem
        {
            Name = "Longer Sword",
            Description = "Increases sword reach",
            Price = 30,
            OnPurchase = () => { _swordReach++; },
            Category = ShopCategory.Upgrade
        });
        
        _shopInventory.Add(new ShopItem
        {
            Name = "Invincibility",
            Description = "5 seconds of invincibility",
            Price = 50,
            OnPurchase = () => { 
                _isInvincible = true;
                _invincibilityTimer = 0f;
                _invincibilityTimer = 5f;
            },
            Category = ShopCategory.Consumable
        });
        
        // Weapons section (empty for now)
        // You can add weapon items here later
        _shopInventory.Add(new ShopItem
        {
            Name = "--- Weapons ---",
            Description = "Coming soon...",
            Price = 0,
            OnPurchase = () => { /* Do nothing, this is just a header */ },
            Category = ShopCategory.Header
        });
        
        // Add crossbow to weapons section
        _shopInventory.Add(new ShopItem
        {
            Name = "Crossbow",
            Description = "Fires bolts at enemies from a distance",
            Price = 75,
            OnPurchase = () => { _hasCrossbow = true; },
            Category = ShopCategory.Weapon
        });
    }

    private void DrawShop()
    {
        // Draw shop background
        Raylib.DrawRectangle(50, 50, Width * CharWidth * DisplayScale - 100, Height * CharHeight * DisplayScale - 100, new Color(0, 0, 0, 200));
        
        // Draw shop title
        DrawText("SHOP", Width * CharWidth * DisplayScale / 2 - 50, 70, Color.Gold);
        
        // Draw gold amount
        DrawText($"Your Gold: {_playerGold}", Width * CharWidth * DisplayScale / 2 - 80, 110, _goldColor);
        
        // Draw shop items
        int startY = 150;
        int itemHeight = 80;
        
        for (int i = 0; i < _shopInventory.Count; i++)
        {
            var item = _shopInventory[i];
            
            // Special handling for header items
            if (item.Category == ShopCategory.Header)
            {
                // Draw section header with special styling
                DrawText(item.Name, 80, startY + i * itemHeight + 10, Color.Gold);
                DrawText(item.Description, 80, startY + i * itemHeight + 35, new Color(150, 150, 150, 255));
                continue;  // Skip the rest of the drawing for headers
            }
            
            Color itemColor = (_selectedShopItem == i) ? Color.Yellow : Color.White;
            Color priceColor = (_playerGold >= item.Price) ? Color.Green : Color.Red;
            
            // Draw selection background if this is the selected item
            if (_selectedShopItem == i)
            {
                Raylib.DrawRectangle(60, startY + i * itemHeight, Width * CharWidth * DisplayScale - 120, itemHeight - 5, new Color(50, 50, 50, 100));
            }
            
            // Draw item name
            DrawText(item.Name, 80, startY + i * itemHeight + 10, itemColor);
            
            // Draw item description
            DrawText(item.Description, 80, startY + i * itemHeight + 35, new Color(200, 200, 200, 255));
            
            // Draw item price
            DrawText($"{item.Price} gold", Width * CharWidth * DisplayScale - 200, startY + i * itemHeight + 20, priceColor);
        }
        
        // Draw instructions
        DrawText("↑/↓: Select Item   Enter: Buy   Esc: Close Shop", 80, Height * CharHeight * DisplayScale - 80, Color.White);
    }

    private void HandleShopInput()
    {
        while (_keyEvents.Count > 0)
        {
            var key = _keyEvents.Dequeue();
            
            if (key == KeyboardKey.Escape)
            {
                _shopOpen = false;
                _currentScreenEnum = GameScreenEnum.Animation;
            }
            else if (key == KeyboardKey.Up)
            {
                // Move selection up, skipping headers
                int newSelection = _selectedShopItem - 1;
                while (newSelection >= 0 && _shopInventory[newSelection].Category == ShopCategory.Header)
                {
                    newSelection--;
                }
                _selectedShopItem = Math.Max(0, newSelection);
            }
            else if (key == KeyboardKey.Down)
            {
                // Move selection down, skipping headers
                int newSelection = _selectedShopItem + 1;
                while (newSelection < _shopInventory.Count && _shopInventory[newSelection].Category == ShopCategory.Header)
                {
                    newSelection++;
                }
                _selectedShopItem = Math.Min(_shopInventory.Count - 1, newSelection);
            }
            else if (key == KeyboardKey.Enter)
            {
                // Try to purchase the selected item
                if (_selectedShopItem >= 0 && _selectedShopItem < _shopInventory.Count)
                {
                    var item = _shopInventory[_selectedShopItem];
                    // Don't allow purchasing headers
                    if (item.Category != ShopCategory.Header && _playerGold >= item.Price)
                    {
                        // Deduct gold
                        _playerGold -= item.Price;
                        
                        // Apply the purchase effect
                        item.OnPurchase();
                    }
                }
            }
        }
    }

    // Add a category enum for shop items
    private enum ShopCategory
    {
        Consumable,
        Upgrade,
        Weapon,
        Header  // Used for section headers
    }

    private void UpdateCrossbowBolts()
    {
        float frameTime = Raylib.GetFrameTime();
        
        for (int i = _crossbowBolts.Count - 1; i >= 0; i--)
        {
            var bolt = _crossbowBolts[i];
            
            // Move the bolt based on its direction
            float moveDistance = BoltSpeed * frameTime;
            bolt.DistanceTraveled += moveDistance;
            
            switch (bolt.Direction)
            {
                case Direction.Left:
                    bolt.X -= moveDistance;
                    break;
                case Direction.Right:
                    bolt.X += moveDistance;
                    break;
                case Direction.Up:
                    bolt.Y -= moveDistance;
                    break;
                case Direction.Down:
                    bolt.Y += moveDistance;
                    break;
            }
            
            // Check if bolt hit any enemy
            bool hitEnemy = false;
            foreach (var enemy in _enemies)
            {
                if (!enemy.Alive) continue;
                
                // Check if bolt is within 0.5 tiles of enemy (for hit detection)
                if (Math.Abs(bolt.X - enemy.X) < 0.5f && Math.Abs(bolt.Y - enemy.Y) < 0.5f)
                {
                    // Kill the enemy
                    enemy.Alive = false;
                    
                    // Create explosion at enemy position
                    _explosions.Add(new Explosion { 
                        X = enemy.X, 
                        Y = enemy.Y, 
                        Timer = 0f 
                    });
                    
                    // Remove the bolt
                    hitEnemy = true;
                    break;
                }
            }
            
            // Remove bolt if it hit an enemy or traveled too far (10 tiles)
            if (hitEnemy || bolt.DistanceTraveled > 10)
            {
                _crossbowBolts.RemoveAt(i);
            }
            // Also remove if it went off-screen
            else if (bolt.X < 0 || bolt.X > 19 || bolt.Y < 0 || bolt.Y > 9)
            {
                _crossbowBolts.RemoveAt(i);
            }
        }
    }

    // Add CrossbowBolt class inside the ScreenPresenter class
    private class CrossbowBolt
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Direction Direction { get; set; }
        public float DistanceTraveled { get; set; }
    }

    // Add ChargerEnemy class inside ScreenPresenter with a hit counter
    private class ChargerEnemy
    {
        public float X { get; set; }
        public float Y { get; set; }
        public bool Alive { get; set; } = true;
        public float MoveTimer { get; set; } = 0f;
        public int Health { get; set; } = 5; // Health display
        public int HitCount { get; set; } = 0; // Count hits separately
        public bool IsInvincible { get; set; } = false; // Add invincibility flag
        public float InvincibilityTimer { get; set; } = 0f; // Add invincibility timer
    }

    // Create a completely new method for handling charger damage
    private void HandleChargerDamage(bool fromSword)
    {
        if (_chargerActive && _charger != null && _charger.Alive && !_charger.IsInvincible)
        {
            // Increment hit counter
            _charger.HitCount++;
            
            // Update displayed health
            _charger.Health = ChargerHealth - _charger.HitCount;
            
            Console.WriteLine($"COLLISION FIX: Charger hit {_charger.HitCount} times. Health display: {_charger.Health}");
            
            // Make charger briefly invincible to prevent multiple hits
            _charger.IsInvincible = true;
            _charger.InvincibilityTimer = 0f;
            
            // Only kill if hit exactly 5 times
            if (_charger.HitCount >= 5)
            {
                Console.WriteLine("COLLISION FIX: Charger defeated after 5 hits!");
                _charger.Alive = false;
                
                // Create explosion at charger position
                _explosions.Add(new Explosion { 
                    X = (int)_charger.X, 
                    Y = (int)_charger.Y, 
                    Timer = 0f 
                });
                
                // Spawn more valuable gold for killing the charger
                for (int i = 0; i < 3; i++)
                {
                    _goldItems.Add(new GoldItem { 
                        X = (int)_charger.X, 
                        Y = (int)_charger.Y, 
                        Value = _random.Next(10, 21)  // 10-20 gold per drop, 3 drops
                    });
                }
                
                _chargerActive = false;
            }
        }
    }

    // Update the SpawnCharger method
    private void SpawnCharger()
    {
        // Find a position that's not occupied by the player or another enemy
        float newX = 0;
        float newY = 0;
        bool isPositionValid = false;

        const int maxAttempts = 10;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Try to spawn at the edge of the map
            int side = _random.Next(4); // 0: top, 1: right, 2: bottom, 3: left
            
            switch (side)
            {
                case 0: // Top
                    newX = _random.Next(20);
                    newY = 0;
                    break;
                case 1: // Right
                    newX = 19;
                    newY = _random.Next(10);
                    break;
                case 2: // Bottom
                    newX = _random.Next(20);
                    newY = 9;
                    break;
                case 3: // Left
                    newX = 0;
                    newY = _random.Next(10);
                    break;
            }

            isPositionValid = (Math.Abs(newX - _animPlayerX) > 3 || Math.Abs(newY - _animPlayerY) > 3) &&
                              !_enemies.Any(e => e.Alive && Math.Abs(e.X - newX) < 0.5f && Math.Abs(e.Y - newY) < 0.5f);

            if (isPositionValid)
                break;
        }

        if (!isPositionValid)
            return;

        // Create a new charger with hardcoded health value
        _charger = new ChargerEnemy { 
            X = newX, 
            Y = newY, 
            Health = 5, // Hardcoded to 5
            Alive = true 
        };
        _chargerActive = true;
        
        Console.WriteLine($"NEW IMPLEMENTATION: Spawned charger with {_charger.Health} health");
    }
}