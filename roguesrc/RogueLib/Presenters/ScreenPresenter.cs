using System.Numerics;
using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

// Add CrossbowBolt class inside the ScreenPresenter class

// Add ChargerEnemy class inside ScreenPresenter with a hit counter

public interface IScreenPresenter
{
    void Initialize(IRayConnection rayConnection, GameState state);
    void Update();
    void Draw(IRayConnection rayConnection, GameState state);
    bool WindowShouldClose();
}

public class ScreenPresenter : IScreenPresenter
{
    
    private readonly Queue<KeyboardKey> _keyEvents = new();

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
    
    private readonly Random _random = new();
    
    private const float InvincibilityDuration = 1.0f;  
    
    private readonly Color _explosionColor = new(255, 165, 0, 255);  // Orange color for explosions

    // Add gold field
    private int _playerGold = 0;
    private readonly Color _goldColor = new(255, 215, 0, 255);  // Gold color

    // Add gold items field
    private readonly List<GoldItem> _goldItems = [];
    
    private const char GoldChar = '$';   // Character to represent gold

    // Add flying gold animation fields
    private readonly List<FlyingGold> _flyingGold = [];
    private const float GoldFlyDuration = 0.3f;  // Reduced from 0.5f to 0.3f for faster animation

    // Replace CRT effect fields with shader-related fields
    private bool _enableCrtEffect = true;
    private float _shaderTime = 0f;

    // Add health pickup fields
    private readonly List<HealthPickup> _healthPickups = [];
    private float _timeSinceLastHealthSpawn = 0f;
    private const float HealthSpawnInterval = 30f;  // Spawn a health pickup every 30 seconds

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
    
    
    private readonly Color _boltColor = new(210, 180, 140, 255); // Light brown color for bolts

    // Add these fields for tracking enemy kills and the charger
    private int _enemiesKilled = 0;
    private const int KillsForCharger = 10;
    private bool _chargerActive = false;
    private ChargerEnemyState? _charger = null;
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

    private const float KnockbackDuration = 0.08f;
    private const float KnockbackDistance = 0.5f; // How far to knock the player back

    // Add these color fields near the other color definitions
    private readonly Color _playerColor = new(0, 200, 255, 255);  // Cyan-blue for player
    private readonly Color _enemyColor = new(255, 100, 100, 255); // Red for enemies

    // Add this field with the other private fields
    private bool _gameJustStarted = true;

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
    private readonly IScreenDrawer _screenDrawer;
    // Add a field to store the loaded map
    private List<string> _map;

    // Add these camera variables to the class fields
    private float _cameraX = 0;
    private float _cameraY = 0;
    private const float CameraDeadZone = 5.0f; // Increased from 3.0f to 5.0f

    private readonly IHealthBarPresenter _healthBarPresenter;

    public ScreenPresenter(IRayLoader rayLoader, IScreenDrawer screenDrawer, IHealthBarPresenter healthBarPresenter)
    {
        _rayLoader = rayLoader;
        _screenDrawer = screenDrawer;
        _healthBarPresenter = healthBarPresenter;
        
        // Load the map from the embedded resource
        _map = rayLoader.LoadMap();
    }

    public void Initialize(IRayConnection rayConnection, GameState state)
    {
        // Load the map from the embedded resource
        _map = _rayLoader.LoadMap();
        
        // Initialize player position on a floor tile
        InitializePlayerPosition(state);
        
        // Rest of initialization code...
        SpawnEnemy(state);
        
        // Spawn initial gold items
        for (int i = 0; i < GameConstants.MaxGoldItems; i++)
        {
            SpawnGoldItem(state);
        }

        // Initialize shop inventory
        InitializeShop(state);
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

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Update shader time
        _shaderTime += Raylib.GetFrameTime();
        
        Raylib.SetShaderValue(
            rayConnection.CrtShader,
            rayConnection.TimeLoc,
            _shaderTime,
            ShaderUniformDataType.Float);
        
        // Draw game to render texture
        Raylib.BeginTextureMode(rayConnection.GameTexture);
        Raylib.ClearBackground(_backgroundColor);

        switch (state.CurrentScreen)
        {
            case GameScreenEnum.Menu:
                DrawMenu(rayConnection);
                HandleMenuInput(state);
                break;

            case GameScreenEnum.CharacterSet:
                DrawCharacterSet(rayConnection);
                HandleCharacterSetInput(state);
                break;

            case GameScreenEnum.Animation:
                DrawAnimation(rayConnection, state);
                HandleAnimationInput(state);
                break;

            case GameScreenEnum.Shop:
                DrawShop(rayConnection);
                HandleShopInput(state);
                break;
        }
        
        Raylib.EndTextureMode();
        
        // Draw render texture to screen with shader
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        
        
        if (_enableCrtEffect)
        {
            Raylib.BeginShaderMode(rayConnection.CrtShader);
            Raylib.DrawTextureRec(
                rayConnection.GameTexture.Texture,
                new Rectangle(0, 0, rayConnection.GameTexture.Texture.Width, -rayConnection.GameTexture.Texture.Height),
                new Vector2(0, 0),
                Color.White
            );
            Raylib.EndShaderMode();
        }
        else
        {
            // Draw without shader if effect is disabled
            Raylib.DrawTextureRec(
                rayConnection.GameTexture.Texture,
                new Rectangle(0, 0, rayConnection.GameTexture.Texture.Width, -rayConnection.GameTexture.Texture.Height),
                new Vector2(0, 0),
                Color.White
            );
        }
        
        Raylib.EndDrawing();

        // Clear processed events
        _keyEvents.Clear();
    }

    private void DrawMenu(IRayConnection rayConnection)
    {
        // Draw debug grid to help with alignment
        DrawDebugGrid();
        
        // Draw a fancy title
        const int titleSize = 48;
        
        // Use MeasureTextEx instead of MeasureText to account for spacing
        Vector2 titleSize2D = Raylib.MeasureTextEx(rayConnection.MenuFont, ScreenConstants.Title, titleSize, 1);
        int titleWidth = (int)titleSize2D.X;
        
        int centerX = (ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale) / 2;
        
        // Draw title with shadow effect
        Raylib.DrawTextEx(rayConnection.MenuFont, ScreenConstants.Title, new Vector2(centerX - titleWidth/2 + 3, 40 + 3), titleSize, 1, new Color(30, 30, 30, 200));
        Raylib.DrawTextEx(rayConnection.MenuFont, ScreenConstants.Title, new Vector2(centerX - titleWidth/2, 40), titleSize, 1, Color.Gold);
        
        // Draw a decorative line under the title - with correct width
        int lineY = 40 + titleSize + 10; // Position it directly under the title with a small gap
        int lineWidth = titleWidth - 20; // Make the line slightly shorter than the title text
        Raylib.DrawRectangle(centerX - lineWidth/2, lineY, lineWidth, 2, Color.Gold);
        
        // Draw center marker
        Raylib.DrawRectangle(centerX - 1, 0, 2, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale, new Color(255, 0, 0, 100));
        
        // Draw menu options centered with more spacing
        int menuStartY = lineY + 30; // Start menu options below the line
        int menuSpacing = 60;
        
        DrawColoredHotkeyText(rayConnection, "Start (A)dventure", centerX - 120, menuStartY);
        DrawColoredHotkeyText(rayConnection, "View (C)haracter Set", centerX - 120, menuStartY + menuSpacing);
        DrawColoredHotkeyText(rayConnection, "(T)oggle CRT Effect", centerX - 120, menuStartY + menuSpacing * 2);
        DrawColoredHotkeyText(rayConnection, "e(X)it Game", centerX - 120, menuStartY + menuSpacing * 3);
        
        // Draw a version number and copyright
        string version = "v0.1 Alpha";
        _screenDrawer.DrawText(rayConnection, version, 20, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 40, new Color(150, 150, 150, 200));
        
        // Draw a small decorative element
        _screenDrawer.DrawCharacter(rayConnection, 2, centerX - 10, menuStartY + menuSpacing * 4, Color.White);
    }

    private void DrawDebugGrid()
    {
        int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        int screenHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
        
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

    private void DrawColoredHotkeyText(IRayConnection rayConnection, string text, int x, int y, ColoredHotkeyOptions? options = null)
    {
        options ??= new ColoredHotkeyOptions();

        int startParenIndex = text.IndexOf('(');
        int endParenIndex = text.IndexOf(')');

        // Guard clause: if no valid parentheses found, draw plain text
        if (startParenIndex == -1 || endParenIndex == -1 || endParenIndex <= startParenIndex + 1)
        {
            _screenDrawer.DrawText(rayConnection, text, x, y, options.BaseColor);
            return;
        }

        // Calculate positions using MeasureTextEx for more accurate spacing
        var currentX = x;

        // Draw text before parenthesis
        var beforeText = text[..startParenIndex];
        _screenDrawer.DrawText(rayConnection, beforeText, currentX, y, options.BaseColor);
        
        // Use MeasureTextEx for more accurate width measurement
        Vector2 beforeSize = Raylib.MeasureTextEx(rayConnection.MenuFont, beforeText, ScreenConstants.MenuFontSize, 1);
        currentX += (int)beforeSize.X - 4;  // Reduce spacing before parenthesis

        // Draw opening parenthesis
        _screenDrawer.DrawText(rayConnection, "(", currentX, y, options.BaseColor);
        Vector2 parenSize = Raylib.MeasureTextEx(rayConnection.MenuFont, "(", ScreenConstants.MenuFontSize, 1);
        currentX += (int)parenSize.X - 2;  // Tighter spacing after opening parenthesis

        // Draw hotkey in different color
        var hotkey = text[(startParenIndex + 1)..endParenIndex];
        _screenDrawer.DrawText(rayConnection, hotkey, currentX, y, options.HotkeyColor);
        Vector2 hotkeySize = Raylib.MeasureTextEx(rayConnection.MenuFont, hotkey, ScreenConstants.MenuFontSize, 1);
        currentX += (int)hotkeySize.X - 2;  // Tighter spacing after hotkey

        // Draw closing parenthesis
        _screenDrawer.DrawText(rayConnection, ")", currentX, y, options.BaseColor);
        Vector2 closeParenSize = Raylib.MeasureTextEx(rayConnection.MenuFont, ")", ScreenConstants.MenuFontSize, 1);
        currentX += (int)closeParenSize.X - 2;  // Tighter spacing after closing parenthesis

        // Draw remaining text
        var afterText = text[(endParenIndex + 1)..];
        _screenDrawer.DrawText(rayConnection, afterText, currentX, y, options.BaseColor);
    }

    private void HandleMenuInput(GameState state)
    {
        while (_keyEvents.Count > 0)
        {
            var key = _keyEvents.Dequeue();
            if (key == KeyboardKey.C)
            {
                state.CurrentScreen = GameScreenEnum.CharacterSet;
                break;
            }
            if (key == KeyboardKey.A)
            {
                state.CurrentScreen = GameScreenEnum.Animation;
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

    private void DrawCharacterSet(IRayConnection rayConnection)
    {
        // Draw all characters in a grid
        for (var charNum = 0; charNum < 256; charNum++)
        {
            var row = charNum / 32;
            var col = charNum % 32;

            _screenDrawer.DrawCharacter(rayConnection, 
                charNum,
                20 + (col * 40),
                20 + (row * 60),
                _colors[charNum % _colors.Length]
            );
        }

        _screenDrawer.DrawText(rayConnection, "Press any key to return", 20, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 40, Color.White);
    }

    private void HandleCharacterSetInput(GameState state)
    {
        if (_keyEvents.Count > 0)
        {
            state.CurrentScreen = GameScreenEnum.Menu;
        }
    }

    private void DrawAnimation(IRayConnection rayConnection, GameState state)
    {
        UpdatePlayerIdleAnimation();
        _healthBarPresenter.Draw(rayConnection, state);
        DrawGoldCounter(rayConnection);
        DrawWorld(rayConnection, state);
        DrawExplosions(state, rayConnection);
        DrawSwordAnimation(rayConnection, state);
        DrawFlyingGold(rayConnection);
        DrawCooldownIndicators(rayConnection, state);
        DrawCrossbowBolts(rayConnection, state);
        DrawChargerHealth(rayConnection);
        DrawInstructions(rayConnection);
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

    private void DrawExplosions(GameState state, IRayConnection rayConnection)
    {
        // Draw explosions (after ground and enemies, but before sword)
        foreach (var explosion in state.Explosions)
        {
            char explosionChar = explosion.Frame switch
            {
                0 => '*',      // Small explosion
                1 => (char)15, // Medium explosion (sun symbol in CP437)
                _ => (char)42  // Large explosion (asterisk)
            };
            
            // Calculate position with camera offset - updated horizontal spacing
            int explosionX = 100 + (int)((explosion.X - _cameraX) * 32) + 400;
            int explosionY = 100 + (int)((explosion.Y - _cameraY) * 40) + 200;
            
            // Only draw if on screen
            if (explosionX >= 0 && explosionX < ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale &&
                explosionY >= 0 && explosionY < ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale)
            {
                _screenDrawer.DrawCharacter(rayConnection, explosionChar, explosionX, explosionY, _explosionColor);
            }
        }
    }

    private void DrawSwordAnimation(IRayConnection rayConnection, GameState state)
    {
        // Handle sword swinging
        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !state.SwordState.IsSwordSwinging && !_swordOnCooldown)
        {
            state.SwordState.IsSwordSwinging = true;
            state.SwordState.SwordSwingTime = 0;
            
            // Check for sword collisions immediately when swing starts
            CheckSwordCollisions(state);
        }

        // Update sword swing animation
        if (state.SwordState.IsSwordSwinging)
        {
            state.SwordState.SwordSwingTime += Raylib.GetFrameTime();
            if (state.SwordState.SwordSwingTime >= GameConstants.SwordSwingDuration)
            {
                state.SwordState.IsSwordSwinging = false;
                _swordOnCooldown = true;  // Start cooldown when swing finishes
                _swordCooldownTimer = 0f;
            }
        }

        // Draw sword if swinging (drawn after ground to appear on top)
        if (state.SwordState.IsSwordSwinging)
        {
            // Calculate animation progress (0.0 to 1.0)
            var progress = state.SwordState.SwordSwingTime / GameConstants.SwordSwingDuration;
            if (progress > 1.0f) progress = 1.0f;

            // Calculate frame (0, 1, or 2)
            var frame = (int)(progress * 3);
            if (frame > 2) frame = 2;

            // Calculate fractional position
            var xOffset = 0f;
            var yOffset = 0f;

            // Determine position based on direction and animation progress
            switch (state.LastDirection)
            {
                case Direction.Left:
                    // Fixed position to the left, sweeping from top to bottom
                    xOffset = -0.9f;
                    yOffset = (progress - 0.5f) * 1.2f;
                    break;
                case Direction.Right:
                    // Fixed position to the right, sweeping from top to bottom
                    xOffset = 0.9f;
                    yOffset = (progress - 0.5f) * 1.2f;
                    break;
                case Direction.Up:
                    // Fixed position above, sweeping from left to right
                    yOffset = -1.2f;
                    xOffset = (progress - 0.5f) * 1.2f;
                    break;
                case Direction.Down:
                    // Fixed position below, sweeping from left to right
                    yOffset = 1.2f;
                    xOffset = (progress - 0.5f) * 1.2f;
                    break;
            }

            // Get sword character based on direction and animation frame
            char swordChar = (state.LastDirection, frame) switch
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

            // Calculate exact pixel position with camera offset - updated horizontal spacing
            float swordX = 100 + ((state.PlayerX + xOffset) - _cameraX) * 32 + 400;
            float swordY = 100 + ((state.PlayerY + yOffset) - _cameraY) * 40 + 200;

            // Draw the sword character with silvery-blue color
            _screenDrawer.DrawCharacter(rayConnection, swordChar, (int)swordX, (int)swordY, ScreenConstants.SwordColor);
        }
    }

    private void DrawFlyingGold(IRayConnection rayConnection)
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
            int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
            string goldText = $"Gold: {_playerGold}";
            int goldTextWidth = Raylib.MeasureText(goldText, ScreenConstants.MenuFontSize);
            int endX = screenWidth - goldTextWidth - 40;  // Position before the text
            int endY = 20;  // Same Y as gold counter
            
            // Interpolate between start and end positions
            int currentX = (int)(gold.StartX + (endX - gold.StartX) * progress);
            int currentY = (int)(gold.StartY + (endY - gold.StartY) * progress);
            
            // Calculate alpha (opacity) based on progress - fade out as it approaches the counter
            byte alpha = (byte)(255 * (1.0f - progress * 0.8f));  // Fade to 20% opacity
            Color fadingGoldColor = new Color(_goldColor.R, _goldColor.G, _goldColor.B, alpha);
            
            // Draw the flying gold character with fading effect
            _screenDrawer.DrawCharacter(rayConnection, GoldChar, currentX, currentY, fadingGoldColor);
        }
    }

    private void DrawCooldownIndicators(IRayConnection rayConnection, GameState state)
    {
        // Draw sword cooldown indicator
        if (_swordOnCooldown)
        {
            // Calculate cooldown progress (0.0 to 1.0)
            float progress = _swordCooldownTimer / _swordCooldown;
            
            // Draw a small cooldown bar above the player
            int barWidth = 30;
            int barHeight = 5;
            
            // Calculate position with camera offset - updated horizontal spacing
            int barX = 100 + (int)((state.PlayerX - _cameraX) * 32) + 400 - barWidth / 2 + 20;  // Center above player
            int barY = 100 + (int)((state.PlayerY - _cameraY) * 40) + 200 - 15;  // Above player
            
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
            
            // Calculate position with camera offset - updated horizontal spacing
            int barX = 100 + (int)((state.PlayerX - _cameraX) * 32) + 400 - barWidth / 2 + 20;  // Center below player
            int barY = 100 + (int)((state.PlayerY - _cameraY) * 40) + 200 + 45;  // Below player
            
            // Background (empty) bar
            Raylib.DrawRectangle(barX, barY, barWidth, barHeight, new Color(50, 50, 50, 200));
            
            // Foreground (filled) bar - grows as cooldown progresses
            Raylib.DrawRectangle(barX, barY, (int)(barWidth * progress), barHeight, new Color(150, 150, 200, 200));
        }
    }

    private void DrawCrossbowBolts(IRayConnection rayConnection, GameState state)
    {
        foreach (var bolt in state.CrossbowBolts)
        {
            // Choose character based on direction
            char boltChar = bolt.Direction switch
            {
                Direction.Left or Direction.Right => '-',
                Direction.Up or Direction.Down => '|',
                _ => '+'
            };
            
            // Draw the bolt at its current position - updated horizontal spacing
            _screenDrawer.DrawCharacter(rayConnection, boltChar, 100 + (int)(bolt.X * 32), 100 + (int)(bolt.Y * 40), _boltColor);
        }
    }

    private void DrawChargerHealth(IRayConnection rayConnection)
    {
        // Draw charger health if active
        if (_chargerActive && _charger != null && _charger.Alive)
        {
            string healthText = $"Charger HP: {_charger.Health}/{ChargerHealth} (Hit {_charger.HitCount} times)";
            _screenDrawer.DrawText(rayConnection, healthText, 20, 60, Color.Red);
        }
    }

    private void DrawInstructions(IRayConnection rayConnection)
    {
        // Move the instructions text up by 20 pixels
        string instructionsText = "Use WASD to move, SPACE to swing sword";
        if (_hasCrossbow)
        {
            instructionsText += ", F to fire crossbow";
        }
        instructionsText += ", ESC to return to menu, (G) for debug gold";
        
        // Changed from Height * ScreenConstants.CharHeight * DisplayScale - 40 to Height * ScreenConstants.CharHeight * DisplayScale - 60
        _screenDrawer.DrawText(rayConnection, instructionsText, 20, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 60, Color.White);
    }

    private void DrawWorld(IRayConnection rayConnection, GameState state)
    {
        // Update camera position to follow player with a dead zone
        UpdateCamera(state);
        
        // Calculate map dimensions
        int mapHeight = _map.Count;
        int mapWidth = _map.Max(line => line.Length);
        
        // Draw the map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Skip tiles that are too far from the camera (optimization)
                if (Math.Abs(x - _cameraX) > 15 || Math.Abs(y - _cameraY) > 10)
                    continue;
                
                // Get the character at this position in the map
                char mapChar = ' '; // Default to empty space (not a dot)
                if (y < _map.Count && x < _map[y].Length)
                {
                    mapChar = _map[y][x];
                }
                
                // Calculate screen position with camera offset
                // Using 32 pixels for horizontal spacing
                int screenX = 100 + (int)((x - _cameraX) * 32) + 400;
                int screenY = 100 + (int)((y - _cameraY) * 40) + 200;
                
                // Draw the appropriate tile based on the map character
                Color tileColor = Color.DarkGray;
                int tileChar = 0; // Default to space (empty) instead of 250 (dot)
                
                var wallColor = Color.Brown;

                switch (mapChar)
                {
                    case '╔': // Top left corner
                        tileChar = 0xC9; // ╔
                        tileColor = wallColor;
                        break;
                    case '╗': // Top right corner
                        tileChar = 0xBB; // ╗
                        tileColor = wallColor;
                        break;
                    case '╚': // Bottom left corner
                        tileChar = 0xC8; // ╚
                        tileColor = wallColor;
                        break;
                    case '╝': // Bottom right corner
                        tileChar = 0xBC; // ╝
                        tileColor = wallColor;
                        break;
                    case '║': // Vertical wall
                    case '|': // Vertical wall
                        tileChar = 0xBA; // ║
                        tileColor = wallColor;
                        break;
                    case '-': // Horizontal wall
                    case '═': // Horizontal wall
                        tileChar = 0xCD; // ═
                        tileColor = wallColor;
                        break;
                    case '╬': // door
                    case '+': // door
                        tileChar = 0xCE; // ╬
                        tileColor = Color.Brown;
                        break;
                    case '.': // Floor
                        tileChar = 250; // ·
                        tileColor = Color.Green;
                        break;
                    case 'X': // Hallway
                        tileChar = 0xB1; // partially filled square
                        tileColor = Color.Gray;
                        break;
                    default:
                        tileChar = 0; // Empty space
                        break;
                }
                
                // Draw the tile
                _screenDrawer.DrawCharacter(rayConnection, tileChar, screenX, screenY, tileColor);
            }
        }
        
        // Update player position calculation with the new spacing
        int playerScreenX = 100 + (int)((state.PlayerX - _cameraX) * 32) + 400;
        int playerScreenY = 100 + (int)((state.PlayerY - _cameraY) * 40) + 200;
        
        // If player is invincible, make them flash
        Color playerColor = _playerColor;
        if (state.IsInvincible && (int)(Raylib.GetTime() * 10) % 2 == 0)
        {
            playerColor = new Color(255, 255, 255, 150); // Semi-transparent white
        }
        
        _screenDrawer.DrawCharacter(rayConnection, (Raylib.GetTime() % 1 < 0.5) ? 2 : 1, playerScreenX, playerScreenY, playerColor);
        
        // Draw enemies - now using camera offset and updated horizontal spacing
        foreach (var enemy in state.Enemies)
        {
            if (enemy.Alive)
            {
                int enemyScreenX = 100 + (int)((enemy.X - _cameraX) * 32) + 400;
                int enemyScreenY = 100 + (int)((enemy.Y - _cameraY) * 40) + 200;
                
                // Only draw if on screen
                if (enemyScreenX >= 0 && enemyScreenX < ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale &&
                    enemyScreenY >= 0 && enemyScreenY < ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale)
                {
                    _screenDrawer.DrawCharacter(rayConnection, ScreenConstants.EnemyChar, enemyScreenX, enemyScreenY, _enemyColor);
                }
            }
        }
        
        // Draw charger if active - with updated horizontal spacing
        if (_chargerActive && _charger != null && _charger.Alive && 
            Math.Abs(_charger.X - _cameraX) < 15 && Math.Abs(_charger.Y - _cameraY) < 10)
        {
            _screenDrawer.DrawCharacter(rayConnection, 6, 100 + (int)((_charger.X - _cameraX) * 32) + 400, 100 + (int)((_charger.Y - _cameraY) * 40) + 200, _chargerColor);
        }
        
        // Draw gold items - with updated horizontal spacing
        foreach (var gold in _goldItems)
        {
            if (Math.Abs(gold.X - _cameraX) < 15 && Math.Abs(gold.Y - _cameraY) < 10)
            {
                _screenDrawer.DrawCharacter(rayConnection, 36, 100 + (int)((gold.X - _cameraX) * 32) + 400, 100 + (int)((gold.Y - _cameraY) * 40) + 200, _goldColor); // $ symbol
            }
        }
        
        // Draw health pickups - with updated horizontal spacing
        foreach (var health in _healthPickups)
        {
            if (Math.Abs(health.X - _cameraX) < 15 && Math.Abs(health.Y - _cameraY) < 10)
            {
                _screenDrawer.DrawCharacter(rayConnection, 3, 100 + (int)((health.X - _cameraX) * 32) + 400, 100 + (int)((health.Y - _cameraY) * 40) + 200, ScreenConstants.HealthColor); // Heart symbol
            }
        }
    }
    
    private void HandleAnimationInput(GameState state)
    {
        // Handle ESC key via event queue for menu navigation
        while (_keyEvents.Count > 0)
        {
            var key = _keyEvents.Dequeue();
            if (key == KeyboardKey.Escape)
            {
                state.CurrentScreen = GameScreenEnum.Menu;
                return;
            }
            if (key == KeyboardKey.Space && !state.SwordState.IsSwordSwinging && !_swordOnCooldown)
            {
                state.SwordState.IsSwordSwinging = true;
                state.SwordState.SwordSwingTime = 0;
                
                // Check for sword collisions immediately when swing starts
                CheckSwordCollisions(state);
            }
            // Add debug option to get free gold with G key
            if (key == KeyboardKey.G)
            {
                _playerGold += 100;
                
                // Optional: Add a visual indicator that gold was added
                int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
                _flyingGold.Add(new FlyingGold { 
                    StartX = screenWidth / 2,
                    StartY = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale / 2,
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
        state.TimeSinceLastMove += Raylib.GetFrameTime();

        // Only move if enough time has passed
        if (state.TimeSinceLastMove >= moveDelay)
        {
            bool moved = false;

            // Check WASD keys
            if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up))
            {
                if (IsWalkableTile(state.PlayerX, state.PlayerY - 1))
                {
                    state.PlayerY -= 1;  // No Math.Max constraint
                }
                state.LastDirection = Direction.Up;
                moved = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down))
            {
                if (IsWalkableTile(state.PlayerX, state.PlayerY + 1))
                {
                    state.PlayerY += 1;  // No Math.Min constraint
                }
                state.LastDirection = Direction.Down;
                moved = true;
            }

            if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left))
            {
                if (IsWalkableTile(state.PlayerX - 1, state.PlayerY))
                {
                    state.PlayerX -= 1;  // No Math.Max constraint
                }
                state.LastDirection = Direction.Left;
                moved = true;
            }
            else if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right))
            {
                if (IsWalkableTile(state.PlayerX + 1, state.PlayerY))
                {
                    state.PlayerX += 1;  // No Math.Min constraint
                }
                state.LastDirection = Direction.Right;
                moved = true;
            }

            // Reset timer if moved
            if (moved)
            {
                state.TimeSinceLastMove = 0;
                
                // Check for gold collection after movement
                CollectGold(state);
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

        // Update enemy movement
        UpdateEnemies(state);

        // Update invincibility timer
        if (state.IsInvincible)
        {
            state.InvincibilityTimer += Raylib.GetFrameTime();
            if (state.InvincibilityTimer >= InvincibilityDuration)
            {
                state.IsInvincible = false;
                state.InvincibilityTimer = 0f;
            }
        }

        // Update explosions
        for (int i = state.Explosions.Count - 1; i >= 0; i--)
        {
            state.Explosions[i].Timer += Raylib.GetFrameTime();
            if (state.Explosions[i].Timer >= GameConstants.ExplosionDuration)
            {
                // Spawn gold at the explosion location when the explosion finishes
                _goldItems.Add(new GoldItem { 
                    X = state.Explosions[i].X, 
                    Y = state.Explosions[i].Y, 
                    Value = _random.Next(3, 8)  // Enemies drop more valuable gold (3-7)
                });
                
                // Remove the explosion
                state.Explosions.RemoveAt(i);
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
            SpawnHealthPickup(state);
            _timeSinceLastHealthSpawn = 0f;
        }
        
        // Check for health pickup collection
        CollectHealth(state);

        // Ensure player is on a walkable tile when game starts
        if (_gameJustStarted)
        {
            _gameJustStarted = false;
            EnsurePlayerOnWalkableTile(state);
        }

        // Open shop with 'B' key
        if (Raylib.IsKeyPressed(KeyboardKey.B))
        {
            _shopOpen = true;
            state.CurrentScreen = GameScreenEnum.Shop;
            _selectedShopItem = 0;
        }

        // Handle crossbow firing with F key
        if (Raylib.IsKeyPressed(KeyboardKey.F) && _hasCrossbow && !_crossbowOnCooldown)
        {
            _isCrossbowFiring = true;
            _crossbowOnCooldown = true;
            _crossbowCooldownTimer = 0f;
            
            // Create a new bolt based on player direction
            float boltX = state.PlayerX;
            float boltY = state.PlayerY;
            Direction boltDirection = state.LastDirection;
            
            state.CrossbowBolts.Add(new CrossbowBoltState
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
        UpdateCrossbowBolts(state);

        // Check for collisions with enemies
        foreach (var enemy in state.Enemies)
        {
            if (enemy.Alive && !state.IsInvincible)
            {
                // Check if player is colliding with this enemy
                if (Math.Abs(state.PlayerX - enemy.X) < 0.5f && Math.Abs(state.PlayerY - enemy.Y) < 0.5f)
                {
                    // Player takes damage
                    state.CurrentHealth--;
                    Console.WriteLine($"Player hit by enemy! Health: {state.CurrentHealth}");
                    
                    // Make player invincible briefly
                    state.IsInvincible = true;
                    state.InvincibilityTimer = 0f;
                    
                    // Apply knockback in the opposite direction of the enemy
                    ApplyKnockback(state, new Vector2(enemy.X, enemy.Y));
                    
                    break; // Only take damage from one enemy per frame
                }
            }
        }
        
        // Check for collisions with charger (deals more damage)
        if (_chargerActive && _charger != null && _charger.Alive && !state.IsInvincible)
        {
            // Check if player is colliding with the charger
            if (Math.Abs(state.PlayerX - _charger.X) < 0.5f && Math.Abs(state.PlayerY - _charger.Y) < 0.5f)
            {
                // Player takes more damage from charger (2 instead of 1)
                state.CurrentHealth -= 2;
                Console.WriteLine($"Player hit by charger! Health: {state.CurrentHealth}");
                
                // Make player invincible briefly
                state.IsInvincible = true;
                state.InvincibilityTimer = 0f;
                
                // Apply stronger knockback from the charger
                ApplyKnockback(state, new Vector2(_charger.X, _charger.Y), 1.0f); // Double knockback distance
            }
        }

        // Update knockback effect
        if (state.IsKnockedBack)
        {
            state.KnockbackTimer += Raylib.GetFrameTime();
            
            // Apply knockback movement during the knockback duration
            if (state.KnockbackTimer < KnockbackDuration)
            {
                // Calculate knockback distance for this frame
                float frameKnockback = KnockbackDistance * Raylib.GetFrameTime() * (1.0f / KnockbackDuration);
                
                // Move player in knockback direction
                switch (state.KnockbackDirection)
                {
                    case Direction.Left:
                        state.PlayerX = Math.Max(0, state.PlayerX - 1);  // Move a full tile left
                        break;
                    case Direction.Right:
                        state.PlayerX = Math.Min(19, state.PlayerX + 1); // Move a full tile right
                        break;
                    case Direction.Up:
                        state.PlayerY = Math.Max(0, state.PlayerY - 1);  // Move a full tile up
                        break;
                    case Direction.Down:
                        state.PlayerY = Math.Min(9, state.PlayerY + 1);  // Move a full tile down
                        break;
                }
            }
            
            // End knockback effect
            if (state.KnockbackTimer >= KnockbackDuration)
            {
                state.IsKnockedBack = false;
                state.KnockbackTimer = 0f;
            }
        }
    }

    private void CollectGold(GameState state)
    {
        // Find any gold within one square of the player's position
        for (int i = _goldItems.Count - 1; i >= 0; i--)
        {
            // Check if gold is at the player's position or one square away
            if (Math.Abs(_goldItems[i].X - state.PlayerX) <= 1 && 
                Math.Abs(_goldItems[i].Y - state.PlayerY) <= 1)
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

    private void CollectHealth(GameState state)
    {
        // Find any health pickup within one square of the player's position
        for (int i = _healthPickups.Count - 1; i >= 0; i--)
        {
            // Check if health pickup is at the player's position or one square away
            if (Math.Abs(_healthPickups[i].X - state.PlayerX) <= 1 && 
                Math.Abs(_healthPickups[i].Y - state.PlayerY) <= 1)
            {
                // Add health to the player
                state.CurrentHealth = Math.Min(ScreenConstants.MaxHealth, state.CurrentHealth + _healthPickups[i].HealAmount);
                
                // Remove the collected health pickup
                _healthPickups.RemoveAt(i);
                
                // Only collect one health pickup per move
                break;
            }
        }
    }

    private void UpdateEnemies(GameState state)
    {
        float frameTime = Raylib.GetFrameTime();
        
        // Update enemy spawn timer
        state.EnemySpawnTimer += frameTime;
        if (state.EnemySpawnTimer >= GameConstants.EnemySpawnDelay)
        {
            state.EnemySpawnTimer = 0f;
            
            // Only spawn if we haven't reached the maximum
            if (state.Enemies.Count(e => e.Alive) < GameConstants.MaxEnemies)
            {
                SpawnEnemy(state);
            }
        }
        
        // Update existing enemies
        foreach (var enemy in state.Enemies)
        {
            if (!enemy.Alive) continue;
            
            enemy.MoveTimer += frameTime;
            if (enemy.MoveTimer >= GameConstants.EnemyMoveDelay)
            {
                enemy.MoveTimer = 0f;
                
                // Calculate direction to player
                int dx = 0;
                int dy = 0;
                
                // Simple AI: move toward player
                if (enemy.X < state.PlayerX) dx = 1;
                else if (enemy.X > state.PlayerX) dx = -1;
                
                if (enemy.Y < state.PlayerY) dy = 1;
                else if (enemy.Y > state.PlayerY) dy = -1;
                
                // Try to move horizontally first
                if (dx != 0)
                {
                    // Check if the new position is walkable
                    if (IsWalkableTile(enemy.X + dx, enemy.Y))
                    {
                        enemy.X += dx;
                    }
                    // If horizontal movement is blocked, try vertical
                    else if (dy != 0 && IsWalkableTile(enemy.X, enemy.Y + dy))
                    {
                        enemy.Y += dy;
                    }
                }
                // If no horizontal movement, try vertical
                else if (dy != 0)
                {
                    // Check if the new position is walkable
                    if (IsWalkableTile(enemy.X, enemy.Y + dy))
                    {
                        enemy.Y += dy;
                    }
                }
                
                // Check for collision with player
                if (Math.Abs(enemy.X - state.PlayerX) < 0.5f && Math.Abs(enemy.Y - state.PlayerY) < 0.5f)
                {
                    // Only damage player if not invincible
                    if (!state.IsInvincible)
                    {
                        state.CurrentHealth--;
                        
                        // Apply knockback
                        ApplyKnockback(state, new Vector2(enemy.X, enemy.Y));
                        
                        // Make player briefly invincible
                        state.IsInvincible = true;
                        state.InvincibilityTimer = 0f;
                    }
                }
            }
        }
        
        // Remove dead enemies
        state.Enemies.RemoveAll(e => !e.Alive);
    }

    // Also update the charger movement logic
    private void UpdateCharger(GameState state)
    {
        if (!_chargerActive || _charger == null || !_charger.Alive)
            return;
        
        float frameTime = Raylib.GetFrameTime();
        
        // Update charger invincibility
        if (_charger.IsInvincible)
        {
            _charger.InvincibilityTimer += frameTime;
            if (_charger.InvincibilityTimer >= 0.5f) // 0.5 seconds of invincibility
            {
                _charger.IsInvincible = false;
            }
        }
        
        // Update charger movement
        _charger.MoveTimer += frameTime;
        if (_charger.MoveTimer >= ChargerSpeed) // Charger moves faster than regular enemies
        {
            _charger.MoveTimer = 0f;
            
            // Calculate direction to player
            float dx = 0;
            float dy = 0;
            
            // Charger AI: move directly toward player
            if (_charger.X < state.PlayerX) dx = 1;
            else if (_charger.X > state.PlayerX) dx = -1;
            
            if (_charger.Y < state.PlayerY) dy = 1;
            else if (_charger.Y > state.PlayerY) dy = -1;
            
            // Try to move horizontally first
            if (dx != 0)
            {
                // Check if the new position is walkable
                if (IsWalkableTile((int)(_charger.X + dx), (int)_charger.Y))
                {
                    _charger.X += dx;
                }
                // If horizontal movement is blocked, try vertical
                else if (dy != 0 && IsWalkableTile((int)_charger.X, (int)(_charger.Y + dy)))
                {
                    _charger.Y += dy;
                }
            }
            // If no horizontal movement, try vertical
            else if (dy != 0)
            {
                // Check if the new position is walkable
                if (IsWalkableTile((int)_charger.X, (int)(_charger.Y + dy)))
                {
                    _charger.Y += dy;
                }
            }
            
            // Check for collision with player
            if (Math.Abs(_charger.X - state.PlayerX) < 0.5f && Math.Abs(_charger.Y - state.PlayerY) < 0.5f)
            {
                // Only damage player if not invincible
                if (!state.IsInvincible)
                {
                    // Charger does 2 damage
                    state.CurrentHealth -= 2;
                    
                    // Apply stronger knockback
                    ApplyKnockback(state, new Vector2(_charger.X, _charger.Y), 1.5f);
                    
                    // Make player briefly invincible
                    state.IsInvincible = true;
                    state.InvincibilityTimer = 0f;
                }
            }
        }
    }

    private void SpawnEnemy(GameState state)
    {
        // Create a list of all valid spawn positions (floor tiles only)
        var validPositions = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (int y = 0; y < _map.Count; y++)
        {
            string line = _map[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '.')  // Only consider floor tiles
                {
                    // Check if position is not occupied by player or other enemies
                    if ((x != state.PlayerX || y != state.PlayerY) &&
                        !state.Enemies.Any(e => e.Alive && e.X == x && e.Y == y))
                    {
                        validPositions.Add((x, y));
                    }
                }
            }
        }
        
        // If no valid positions found, don't spawn
        if (validPositions.Count == 0)
            return;
        
        // Pick a random valid position
        var randomIndex = _random.Next(validPositions.Count);
        var (newX, newY) = validPositions[randomIndex];
        
        // Spawn the enemy
        state.Enemies.Add(new Enemy { X = newX, Y = newY, Alive = true });
    }

    private void SpawnGoldItem(GameState state)
    {
        // Create a list of all valid spawn positions (floor tiles only)
        var validPositions = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (int y = 0; y < _map.Count; y++)
        {
            string line = _map[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '.')  // Only consider floor tiles
                {
                    // Check if position is not occupied by player, enemies, or other gold
                    if ((x != state.PlayerX || y != state.PlayerY) &&
                        !state.Enemies.Any(e => e.Alive && e.X == x && e.Y == y) &&
                        !_goldItems.Any(g => g.X == x && g.Y == y))
                    {
                        validPositions.Add((x, y));
                    }
                }
            }
        }
        
        // If no valid positions found, don't spawn
        if (validPositions.Count == 0)
            return;
        
        // Pick a random valid position
        var randomIndex = _random.Next(validPositions.Count);
        var (newX, newY) = validPositions[randomIndex];
        
        // Spawn the gold
        _goldItems.Add(new GoldItem { X = newX, Y = newY, Value = _random.Next(1, 6) });  // Gold worth 1-5
    }

    private void SpawnHealthPickup(GameState state)
    {
        // Create a list of all valid spawn positions (floor tiles only)
        var validPositions = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (int y = 0; y < _map.Count; y++)
        {
            string line = _map[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '.')  // Only consider floor tiles
                {
                    // Check if position is not occupied by player, enemies, gold, or other health pickups
                    if ((x != state.PlayerX || y != state.PlayerY) &&
                        !state.Enemies.Any(e => e.Alive && e.X == x && e.Y == y) &&
                        !_goldItems.Any(g => g.X == x && g.Y == y) &&
                        !_healthPickups.Any(h => h.X == x && h.Y == y))
                    {
                        validPositions.Add((x, y));
                    }
                }
            }
        }
        
        // If no valid positions found, don't spawn
        if (validPositions.Count == 0)
            return;
        
        // Pick a random valid position
        var randomIndex = _random.Next(validPositions.Count);
        var (newX, newY) = validPositions[randomIndex];
        
        // Spawn the health pickup
        _healthPickups.Add(new HealthPickup { X = newX, Y = newY, HealAmount = 20 });  // Restore 20 health
    }

    private void DrawGoldCounter(IRayConnection rayConnection)
    {
        // Draw gold counter at the top-right of the screen
        int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        string goldText = $"Gold: {_playerGold}";
        int goldTextWidth = Raylib.MeasureText(goldText, ScreenConstants.MenuFontSize);
        
        _screenDrawer.DrawText(rayConnection, goldText, screenWidth - goldTextWidth - 20, 20, _goldColor);
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

    private void InitializeShop(GameState state)
    {
        // Regular items section
        _shopInventory.Add(new ShopItem
        {
            Name = "Health Potion",
            Description = "Restores 25 health",
            Price = 10,
            OnPurchase = () => { state.CurrentHealth = Math.Min(10, state.CurrentHealth + 25); },
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
                state.IsInvincible = true;
                state.InvincibilityTimer = 0f;
                state.InvincibilityTimer = 5f;
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

    private void DrawShop(IRayConnection rayConnection)
    {
        // Draw shop background
        Raylib.DrawRectangle(50, 50, ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale - 100, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 100, new Color(30, 30, 30, 230));
        Raylib.DrawRectangleLines(50, 50, ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale - 100, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 100, Color.Gold);
        
        // Draw shop title
        string shopTitle = "ADVENTURER'S SHOP";
        Vector2 titleSize = Raylib.MeasureTextEx(rayConnection.MenuFont, shopTitle, 32, 1);
        Raylib.DrawTextEx(rayConnection.MenuFont, shopTitle, new Vector2((ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale) / 2 - titleSize.X / 2, 70), 32, 1, Color.Gold);
        
        // Draw player's gold
        string goldText = $"Your Gold: {_playerGold}";
        _screenDrawer.DrawText(rayConnection, goldText, 70, 120, _goldColor);
        
        // Draw shop items
        int itemY = 170;
        int itemSpacing = 50;
        
        for (int i = 0; i < _shopInventory.Count; i++)
        {
            var item = _shopInventory[i];
            Color itemColor = i == _selectedShopItem ? Color.White : new Color(200, 200, 200, 200);
            
            // Draw selection indicator
            if (i == _selectedShopItem)
            {
                Raylib.DrawRectangle(60, itemY - 5, ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale - 120, 40, new Color(60, 60, 60, 150));
            }
            
            // Draw item name and price
            _screenDrawer.DrawText(rayConnection, item.Name, 70, itemY, itemColor);
            string priceText = $"{item.Price} gold";
            _screenDrawer.DrawText(rayConnection, priceText, ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale - 200, itemY, itemColor);
            
            // Draw item description
            _screenDrawer.DrawText(rayConnection, item.Description, 70, itemY + 20, new Color(150, 150, 150, 200));
            
            itemY += itemSpacing;
        }
        
        // Draw instructions
        _screenDrawer.DrawText(rayConnection, "Use UP/DOWN to select, ENTER to buy, ESC to exit shop", 70, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 100, Color.White);
    }

    private void HandleShopInput(GameState state)
    {
        while (_keyEvents.Count > 0)
        {
            var key = _keyEvents.Dequeue();
            
            if (key == KeyboardKey.Escape)
            {
                _shopOpen = false;
                state.CurrentScreen = GameScreenEnum.Animation;
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

    private void UpdateCrossbowBolts(GameState state)
    {
        float frameTime = Raylib.GetFrameTime();
        
        for (int i = state.CrossbowBolts.Count - 1; i >= 0; i--)
        {
            var bolt = state.CrossbowBolts[i];
            
            // Move the bolt based on its direction
            float moveDistance = GameConstants.BoltSpeed * frameTime;
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
            foreach (var enemy in state.Enemies)
            {
                if (!enemy.Alive) continue;
                
                // Check if bolt is within 0.5 tiles of enemy (for hit detection)
                if (Math.Abs(bolt.X - enemy.X) < 0.5f && Math.Abs(bolt.Y - enemy.Y) < 0.5f)
                {
                    // Kill the enemy
                    enemy.Alive = false;
                    
                    // Create explosion at enemy position
                    state.Explosions.Add(new ExplosionState { 
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
                state.CrossbowBolts.RemoveAt(i);
            }
            // Also remove if it went off-screen
            else if (bolt.X < 0 || bolt.X > 19 || bolt.Y < 0 || bolt.Y > 9)
            {
                state.CrossbowBolts.RemoveAt(i);
            }
        }
    }

    

    // Create a completely new method for handling charger damage
    private void HandleChargerDamage(GameState state, bool fromSword)
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
                state.Explosions.Add(new ExplosionState { 
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
    private void SpawnCharger(GameState state)
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

            isPositionValid = (Math.Abs(newX - state.PlayerX) > 3 || Math.Abs(newY - state.PlayerY) > 3) &&
                              !state.Enemies.Any(e => e.Alive && Math.Abs(e.X - newX) < 0.5f && Math.Abs(e.Y - newY) < 0.5f);

            if (isPositionValid)
                break;
        }

        if (!isPositionValid)
            return;

        // Create a new charger with hardcoded health value
        _charger = new ChargerEnemyState { 
            X = newX, 
            Y = newY, 
            Health = 5, // Hardcoded to 5
            Alive = true 
        };
        _chargerActive = true;
        
        Console.WriteLine($"NEW IMPLEMENTATION: Spawned charger with {_charger.Health} health");
    }

    // Add this new method for applying knockback
    private void ApplyKnockback(GameState state, Vector2 sourcePosition, float multiplier = 1.0f)
    {
        // Determine knockback direction (away from the source)
        float dx = state.PlayerX - sourcePosition.X;
        float dy = state.PlayerY - sourcePosition.Y;
        
        // If player is exactly on the enemy, use the player's facing direction
        if (Math.Abs(dx) < 0.1f && Math.Abs(dy) < 0.1f)
        {
            state.KnockbackDirection = state.LastDirection switch
            {
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                _ => Direction.Right
            };
        }
        else if (Math.Abs(dx) > Math.Abs(dy))
        {
            // Knockback horizontally
            state.KnockbackDirection = dx > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            // Knockback vertically
            state.KnockbackDirection = dy > 0 ? Direction.Down : Direction.Up;
        }
        
        // Start knockback effect
        state.IsKnockedBack = true;
        state.KnockbackTimer = 0f;
    }

    // Add a helper method to check if a tile is walkable
    private bool IsWalkableTile(int x, int y)
    {
        // Check if position is within map bounds
        if (y < 0 || y >= _map.Count)
            return false; // Out of bounds vertically
        
        // Check if x is within the bounds of the current line
        if (x < 0 || x >= _map[y].Length)
            return false; // Out of bounds horizontally
        
        // Check if the tile is a wall or other non-walkable object
        char mapChar = _map[y][x];

        var walkableTiles = new List<char> { '.', 'X', '╬' };

        return walkableTiles.Contains(mapChar);
    }

    // Add a new method to update the camera position
    private void UpdateCamera(GameState state)
    {
        // Calculate how far the player is from the camera center
        float deltaX = state.PlayerX - _cameraX;
        float deltaY = state.PlayerY - _cameraY;
        
        // If player is outside the dead zone, move the camera
        if (Math.Abs(deltaX) > CameraDeadZone)
        {
            // Move camera in the direction of the player
            _cameraX += deltaX > 0 ? 
                Math.Min(deltaX - CameraDeadZone, 0.5f) : 
                Math.Max(deltaX + CameraDeadZone, -0.5f);
        }
        
        if (Math.Abs(deltaY) > CameraDeadZone)
        {
            // Move camera in the direction of the player
            _cameraY += deltaY > 0 ? 
                Math.Min(deltaY - CameraDeadZone, 0.5f) : 
                Math.Max(deltaY + CameraDeadZone, -0.5f);
        }
    }

    private void CheckSwordCollisions(GameState state)
    {
        // Calculate sword position based on player position and direction
        float swordX = state.PlayerX;
        float swordY = state.PlayerY;
        
        // Adjust position based on direction and sword reach
        switch (state.LastDirection)
        {
            case Direction.Left:
                swordX -= _swordReach;
                break;
            case Direction.Right:
                swordX += _swordReach;
                break;
            case Direction.Up:
                swordY -= _swordReach;
                break;
            case Direction.Down:
                swordY += _swordReach;
                break;
        }
        
        // Check for collision with regular enemies
        foreach (var enemy in state.Enemies)
        {
            if (enemy.Alive)
            {
                // Check if sword is within 0.5 tiles of enemy (for hit detection)
                if (Math.Abs(swordX - enemy.X) < 0.5f && Math.Abs(swordY - enemy.Y) < 0.5f)
                {
                    // Kill the enemy
                    enemy.Alive = false;
                    
                    // Create explosion at enemy position
                    state.Explosions.Add(new ExplosionState { 
                        X = enemy.X, 
                        Y = enemy.Y, 
                        Timer = 0f 
                    });
                    
                    // Increment kill counter
                    _enemiesKilled++;
                    
                    // Check if we should spawn a charger
                    if (_enemiesKilled >= KillsForCharger && !_chargerActive)
                    {
                        SpawnCharger(state);
                    }
                    
                    break; // Only hit one enemy per swing
                }
            }
        }
        
        // Check for collision with charger
        if (_chargerActive && _charger != null && _charger.Alive)
        {
            // Check if sword is within 0.5 tiles of charger
            if (Math.Abs(swordX - _charger.X) < 0.5f && Math.Abs(swordY - _charger.Y) < 0.5f)
            {
                // Handle charger damage
                HandleChargerDamage(state, true);
            }
        }
    }

    // Add this new method to ensure player spawns on a walkable tile
    private void EnsurePlayerOnWalkableTile(GameState state)
    {
        // If player is already on a walkable tile, do nothing
        if (IsWalkableTile((int)state.PlayerX, (int)state.PlayerY))
        {
            return;
        }
        
        // Search for a walkable tile in an expanding spiral pattern
        int maxSearchRadius = 20;  // Maximum search distance
        
        for (int radius = 1; radius <= maxSearchRadius; radius++)
        {
            // Check in a square pattern around the player
            for (int offsetX = -radius; offsetX <= radius; offsetX++)
            {
                for (int offsetY = -radius; offsetY <= radius; offsetY++)
                {
                    // Skip if not on the perimeter of the square
                    if (Math.Abs(offsetX) != radius && Math.Abs(offsetY) != radius)
                        continue;
                    
                    int testX = (int)state.PlayerX + offsetX;
                    int testY = (int)state.PlayerY + offsetY;
                    
                    if (IsWalkableTile(testX, testY))
                    {
                        // Found a walkable tile, move player there
                        state.PlayerX = testX;
                        state.PlayerY = testY;
                        return;
                    }
                }
            }
        }
        
        // If no walkable tile found, force create a room at player position
        // This is a fallback that should rarely be needed
        int roomWidth = 7;
        int roomHeight = 5;
        int roomX = (int)state.PlayerX - roomWidth / 2;
        int roomY = (int)state.PlayerY - roomHeight / 2;
        
        // Create a simple room
        for (int y = 0; y < roomHeight; y++)
        {
            while (_map.Count <= roomY + y)
            {
                _map.Add(new string(' ', roomX + roomWidth));
            }
            
            string line = _map[roomY + y];
            while (line.Length <= roomX + roomWidth)
            {
                line += ' ';
            }
            
            char[] lineChars = line.ToCharArray();
            
            for (int x = 0; x < roomWidth; x++)
            {
                if (y == 0 || y == roomHeight - 1)
                {
                    // Top and bottom walls
                    lineChars[roomX + x] = (y == 0 && x == 0) ? '╔' :
                                          (y == 0 && x == roomWidth - 1) ? '╗' :
                                          (y == roomHeight - 1 && x == 0) ? '╚' :
                                          (y == roomHeight - 1 && x == roomWidth - 1) ? '╝' : '═';
                }
                else if (x == 0 || x == roomWidth - 1)
                {
                    // Side walls
                    lineChars[roomX + x] = '║';
                }
                else
                {
                    // Floor
                    lineChars[roomX + x] = '.';
                }
            }
            
            _map[roomY + y] = new string(lineChars);
        }
        
        // Place player in the center of the new room
        state.PlayerX = roomX + roomWidth / 2;
        state.PlayerY = roomY + roomHeight / 2;
    }

    // Add this method to initialize the player position on a floor tile
    private void InitializePlayerPosition(GameState state)
    {
        // Create a list of all valid spawn positions (floor tiles only)
        var floorTiles = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (int y = 0; y < _map.Count; y++)
        {
            string line = _map[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '.')  // Only consider floor tiles
                {
                    floorTiles.Add((x, y));
                }
            }
        }
        
        // If no floor tiles found, keep default position
        if (floorTiles.Count == 0)
            return;
        
        // Pick a random floor tile
        var randomIndex = _random.Next(floorTiles.Count);
        var (newX, newY) = floorTiles[randomIndex];
        
        // Set player position
        state.PlayerX = newX;
        state.PlayerY = newY;
        
        // Immediately center camera on player for initial spawn
        _cameraX = state.PlayerX;
        _cameraY = state.PlayerY;
    }
}