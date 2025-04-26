using System.Numerics;
using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using RogueLib.Handlers;
using RogueLib.Models;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface IScreenPresenter
{
    void Initialize(IRayConnection rayConnection, GameState state);
    void Update(GameState state);
    void Draw(IRayConnection rayConnection, GameState state);
    bool WindowShouldClose();
}

public class ScreenPresenter : IScreenPresenter
{
    private readonly Random _random = new();


    private bool _shouldEnableCrtEffect = true;
    private float _shaderTime = 0f;

    private int _enemiesKilled = 0;
    private bool _isChargerActive = false;
    private ChargerEnemyState? _chargerState = null;
    private bool _didGameJustStart = true;
    private readonly IRayLoader _rayLoader;
    private readonly IDrawUtil _screenDrawUtil;    
    private readonly IHealthBarPresenter _healthBarPresenter;
    private readonly IShopPresenter _shopPresenter;
    private readonly IChunkPresenter _chunkPresenter;
    private readonly IDebugPanelPresenter _debugPanelPresenter;
    private readonly ISpawnEnemyHandler _spawnEnemyHandler;
    private readonly IPlayerPresenter _playerPresenter;
    private readonly IBannerPresenter _bannerPresenter;
    private readonly IMenuPresenter _menuPresenter;
    private readonly ICharacterSetPresenter _characterSetPresenter;
    private readonly ISwordPresenter _swordPresenter;
    private readonly IInstructionsPresenter _instructionsPresenter;
    private readonly IGoldCounterPresenter _goldCounterPresenter;
    private readonly IFlyingGoldPresenter _flyingGoldPresenter;
    private readonly ICooldownIndicatorPresenter _cooldownIndicatorPresenter;
    private readonly IUpdateEnemiesHandler _updateEnemiesHandler;
    private readonly IMenuInputHandler _menuInputHandler;
    private readonly IEnemyPresenter _enemyPresenter;
    private readonly IExplosionPresenter _explosionPresenter;
    private readonly IMapPresenter _mapPresenter;
    private readonly IMapUtil _mapUtil;

    public ScreenPresenter(
        IRayLoader rayLoader, 
        IDrawUtil drawUtil, 
        IHealthBarPresenter healthBarPresenter,
        IShopPresenter shopPresenter,
        IChunkPresenter chunkPresenter,
        IDebugPanelPresenter debugPanelPresenter,
        ISpawnEnemyHandler spawnEnemyHandler,
        IPlayerPresenter playerPresenter,
        IBannerPresenter bannerPresenter,
        IMenuPresenter menuPresenter,
        ICharacterSetPresenter characterSetPresenter,
        ISwordPresenter swordPresenter,
        IInstructionsPresenter instructionsPresenter,
        IGoldCounterPresenter goldCounterPresenter,
        IFlyingGoldPresenter flyingGoldPresenter,
        ICooldownIndicatorPresenter cooldownIndicatorPresenter,
        IUpdateEnemiesHandler updateEnemiesHandler,
        IMenuInputHandler menuInputHandler,
        IEnemyPresenter enemyPresenter,
        IExplosionPresenter explosionPresenter,
        IMapPresenter mapPresenter,
        IMapUtil mapUtil)
    {
        _rayLoader = rayLoader;
        _screenDrawUtil = drawUtil;
        _healthBarPresenter = healthBarPresenter;
        _shopPresenter = shopPresenter;
        _chunkPresenter = chunkPresenter;
        _debugPanelPresenter = debugPanelPresenter;
        _spawnEnemyHandler = spawnEnemyHandler;
        _playerPresenter = playerPresenter;
        _bannerPresenter = bannerPresenter;
        _menuPresenter = menuPresenter;
        _characterSetPresenter = characterSetPresenter;
        _swordPresenter = swordPresenter;
        _instructionsPresenter = instructionsPresenter;
        _goldCounterPresenter = goldCounterPresenter;
        _flyingGoldPresenter = flyingGoldPresenter;
        _cooldownIndicatorPresenter = cooldownIndicatorPresenter;
        _updateEnemiesHandler = updateEnemiesHandler;
        _menuInputHandler = menuInputHandler;
        _enemyPresenter = enemyPresenter;
        _explosionPresenter = explosionPresenter;
        _mapPresenter = mapPresenter;
        _mapUtil = mapUtil;
    }

    public void Initialize(IRayConnection rayConnection, GameState state)
    {
        // Load the map from the embedded resource
        state.Map = _rayLoader.LoadMap();
        
        // Initialize player position on a floor tile
        InitializePlayerPosition(state);
        
        // Rest of initialization code...
        _spawnEnemyHandler.Handle(state);
        
        // Spawn initial gold items
        for (int i = 0; i < GameConstants.MaxGoldItems; i++)
        {
            SpawnGoldItem(state);
        }

        // Initialize shop inventory
        InitializeShop(state);
    }

    public void Update(GameState state)
    {
        // Collect all key events that occurred
        int key;
        while ((key = Raylib.GetKeyPressed()) != 0)
        {
            state.KeyEvents.Enqueue((KeyboardKey)key);
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
        Raylib.ClearBackground(ScreenConstants.BackgroundColor);

        switch (state.CurrentScreen)
        {
            case GameScreenEnum.Menu:
                _menuPresenter.Draw(rayConnection);
                _menuInputHandler.Handle(state);
                break;

            case GameScreenEnum.CharacterSet:
                _characterSetPresenter.Draw(rayConnection);
                HandleCharacterSetInput(state);
                break;

            case GameScreenEnum.Adventure:
                DrawAdventure(rayConnection, state);
                HandleAdventureInput(state);
                break;

            case GameScreenEnum.Shop:
                _shopPresenter.Draw(rayConnection, state);
                HandleShopInput(state);
                break;
        }
        
        Raylib.EndTextureMode();
        
        // Draw render texture to screen with shader
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        
        
        if (_shouldEnableCrtEffect)
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
        state.KeyEvents.Clear();
    }

    private void HandleCharacterSetInput(GameState state)
    {
        if (state.KeyEvents.Count > 0)
        {
            state.CurrentScreen = GameScreenEnum.Menu;
        }
    }

    private void DrawAdventure(IRayConnection rayConnection, GameState state)
    {
        _healthBarPresenter.Draw(rayConnection, state);
        _goldCounterPresenter.Draw(rayConnection, state);
        UpdateCamera(state);
        _mapPresenter.Draw(rayConnection, state);
        _enemyPresenter.Draw(rayConnection, state);
        DrawCharger(rayConnection, state);        
        DrawGoldItems(rayConnection, state);
        DrawHealthPickups(rayConnection, state);
        _playerPresenter.Draw(rayConnection, state);
        _debugPanelPresenter.Draw(rayConnection, state);
        _explosionPresenter.Draw(rayConnection, state);
        _swordPresenter.Draw(rayConnection, state);
        _flyingGoldPresenter.Draw(rayConnection, state);
        _cooldownIndicatorPresenter.Draw(rayConnection, state);
        DrawCrossbowBolts(rayConnection, state);
        DrawChargerHealth(rayConnection, state);
        _bannerPresenter.Draw(rayConnection, state);
        _instructionsPresenter.Draw(rayConnection, state);
        _chunkPresenter.Draw(rayConnection, state);
        _cooldownIndicatorPresenter.Draw(rayConnection, state);
        _enemyPresenter.Draw(rayConnection, state);
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
            _screenDrawUtil.DrawCharacter(rayConnection, boltChar, 100 + (int)(bolt.X * 32), 100 + (int)(bolt.Y * 40), ScreenConstants.BoltColor);
        }
    }

    private void DrawChargerHealth(IRayConnection rayConnection, GameState state)
    {
        // Draw charger health if active
        if (state.IsChargerActive && state.Charger != null && state.Charger.IsAlive)
        {
            string healthText = $"Charger HP: {state.Charger.Health}/{GameConstants.ChargerHealth} (Hit {state.Charger.HitCount} times)";
            _screenDrawUtil.DrawText(rayConnection, healthText, 20, 60, ScreenConstants.ChargerColor);
        }
    }

    private void DrawHealthPickups(IRayConnection rayConnection, GameState state)
    {
        // Draw health pickups - with updated horizontal spacing
        foreach (var health in state.HealthPickupState.HealthPickups)
        {
            if (Math.Abs(health.X - state.CameraState.X) < 15 && Math.Abs(health.Y - state.CameraState.Y) < 10)
            {
                _screenDrawUtil.DrawCharacter(rayConnection, 3, 100 + (int)((health.X - state.CameraState.X) * 32) + 400, 100 + (int)((health.Y - state.CameraState.Y) * 40) + 200, ScreenConstants.HealthColor); // Heart symbol
            }
        }
    }

    private void DrawGoldItems(IRayConnection rayConnection, GameState state)
    {
        // Draw gold items - with updated horizontal spacing
        foreach (var gold in state.GoldItems)
        {
            if (Math.Abs(gold.Position.X - state.CameraState.X) < 15 && Math.Abs(gold.Position.Y - state.CameraState.Y) < 10)
            {
                _screenDrawUtil.DrawCharacter(rayConnection, 36, 100 + (int)((gold.Position.X - state.CameraState.X) * 32) + 400, 100 + (int)((gold.Position.Y - state.CameraState.Y) * 40) + 200, ScreenConstants.GoldColor); // $ symbol
            }
        }
    }

    private void DrawCharger(IRayConnection rayConnection, GameState state)
    {
        // Draw charger if active - with updated horizontal spacing
        if (_isChargerActive && _chargerState != null && _chargerState.IsAlive && 
            Math.Abs(_chargerState.X - state.CameraState.X) < 15 && Math.Abs(_chargerState.Y - state.CameraState.Y) < 10)
        {
            _screenDrawUtil.DrawCharacter(rayConnection, 6, 100 + (int)((_chargerState.X - state.CameraState.X) * 32) + 400, 100 + (int)((_chargerState.Y - state.CameraState.Y) * 40) + 200, ScreenConstants.ChargerColor);
        }
    }

    private void HandleAdventureInput(GameState state)
    {
        // Handle ESC key via event queue for menu navigation
        while (state.KeyEvents.Count > 0)
        {
            var key = state.KeyEvents.Dequeue();
            if (key == KeyboardKey.Escape)
            {
                state.CurrentScreen = GameScreenEnum.Menu;
                return;
            }
            if (key == KeyboardKey.Space && !state.SwordState.IsSwordSwinging && !state.SwordState.SwordOnCooldown)
            {
                state.SwordState.IsSwordSwinging = true;
                state.SwordState.SwordSwingTime = 0;
                state.SwordState.SwingDirection = state.ActionDirection;
                
                // Check for sword collisions immediately when swing starts
                CheckSwordCollisions(state, true);
            }

            // Add debug option to get free gold with G key
            if (key == KeyboardKey.G)
            {
                GenerateFreeGold(state);
            }

            // Add debug option to spawn charger with C key
            if (key == KeyboardKey.C)
            {
                SpawnCharger(state);
            }
        }

        // Handle movement with direct key state checks
        // This allows for continuous movement when keys are held down

        float moveAmount = GameConstants.PlayerMoveSpeed * Raylib.GetFrameTime();

        // Check for diagonal movement first
        bool upPressed = Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up);
        bool downPressed = Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down);
        bool leftPressed = Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left);
        bool rightPressed = Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right);

        // Apply acceleration based on input
        if (upPressed) state.VelocityY -= moveAmount;
        if (downPressed) state.VelocityY += moveAmount;
        if (leftPressed) state.VelocityX -= moveAmount;
        if (rightPressed) state.VelocityX += moveAmount;

        // Apply friction
        state.VelocityX *= GameConstants.PlayerFriction;
        state.VelocityY *= GameConstants.PlayerFriction;

        // Clamp velocity to maximum
        float currentSpeed = (float)Math.Sqrt(state.VelocityX * state.VelocityX + state.VelocityY * state.VelocityY);
        if (currentSpeed > GameConstants.PlayerMaxVelocity)
        {
            float ratio = GameConstants.PlayerMaxVelocity / currentSpeed;
            state.VelocityX *= ratio;
            state.VelocityY *= ratio;
        }

        // Apply velocity to position if the new position is walkable
        bool moved = false;
        float newX = state.PlayerPosition.X + state.VelocityX;
        float newY = state.PlayerPosition.Y + state.VelocityY;

        if (_mapUtil.IsWalkableTile(state.Map, (int)Math.Floor(newX), (int)Math.Floor(state.PlayerPosition.Y)))
        {
            state.PreviousPlayerPosition.X = state.PlayerPosition.X;
            state.PlayerPosition.X = newX;
            moved = true;
        }
        else
        {
            // Stop horizontal movement if we hit a wall
            state.VelocityX = 0;
        }

        if (_mapUtil.IsWalkableTile(state.Map, (int)Math.Floor(state.PlayerPosition.X), (int)Math.Floor(newY)))
        {
            state.PreviousPlayerPosition.Y = state.PlayerPosition.Y;
            state.PlayerPosition.Y = newY;
            moved = true;
        }
        else
        {
            // Stop vertical movement if we hit a wall
            state.VelocityY = 0;
        }

        // Update direction based on velocity
        if (Math.Abs(state.VelocityX) > Math.Abs(state.VelocityY))
        {
            state.ActionDirection = state.VelocityX > 0 ? Direction.Right : Direction.Left;
        }
        else if (state.VelocityY != 0)
        {
            state.ActionDirection = state.VelocityY > 0 ? Direction.Down : Direction.Up;
        }

        // Reset timer if moved
        if (moved)
        {
            state.MovementStartTime = (float)Raylib.GetTime();
            
            // Check for gold collection after movement
            CollectGold(state);
            
            // Check for sword collisions during movement
            CheckSwordCollisions(state, false);
        }

        // Update sword cooldown
        if (state.SwordState.SwordOnCooldown)
        {
            state.SwordState.SwordCooldownTimer += Raylib.GetFrameTime();
            if (state.SwordState.SwordCooldownTimer >= state.SwordState.SwordCooldown)
            {
                state.SwordState.SwordOnCooldown = false;
                state.SwordState.SwordCooldownTimer = 0f;
            }
        }

        // Update enemy movement
        _updateEnemiesHandler.Handle(state);
        UpdateCharger(state);

        AdvanceInvisibility(state);
        AdvanceExplosions(state);
        AdvanceFlyingGold(state);

        // Update health pickup spawn timer
        state.HealthPickupState._timeSinceLastHealthSpawn += Raylib.GetFrameTime();
        if (state.HealthPickupState._timeSinceLastHealthSpawn >= GameConstants.HealthSpawnInterval)
        {
            SpawnHealthPickup(state);
            state.HealthPickupState._timeSinceLastHealthSpawn = 0f;
        }
        
        // Check for health pickup collection
        CollectHealthPickup(state);

        // Ensure player is on a walkable tile when game starts
        if (_didGameJustStart)
        {
            _didGameJustStart = false;
            EnsurePlayerOnWalkableTile(state);
        }

        // Open shop with 'B' key
        if (Raylib.IsKeyPressed(KeyboardKey.B))
        {
            state.CurrentScreen = GameScreenEnum.Shop;
            state.ShopState.SelectedShopItem = 0;
        }

        // Handle crossbow firing with F key
        if (Raylib.IsKeyPressed(KeyboardKey.F) && state.CrossbowState.HasCrossbow && !state.CrossbowState.CrossbowOnCooldown)
        {
            state.CrossbowState.CrossbowOnCooldown = true;
            state.CrossbowState.CrossbowCooldownTimer = 0f;
            
            // Create a new bolt based on player direction
            float boltX = state.PlayerPosition.X;
            float boltY = state.PlayerPosition.Y;
            Direction boltDirection = state.ActionDirection;
            
            state.CrossbowBolts.Add(new CrossbowBoltState
            {
                X = boltX,
                Y = boltY,
                Direction = boltDirection,
                DistanceTraveled = 0f
            });
        }
        
        // Update crossbow cooldown
        if (state.CrossbowState.CrossbowOnCooldown)
        {
            state.CrossbowState.CrossbowCooldownTimer += Raylib.GetFrameTime();
            if (state.CrossbowState.CrossbowCooldownTimer >= state.CrossbowState.CrossbowCooldown)
            {
                state.CrossbowState.CrossbowOnCooldown = false;
                state.CrossbowState.CrossbowCooldownTimer = 0f;
            }
        }
        
        // Update crossbow bolts
        UpdateCrossbowBolts(state);
        HandlePlayerBumpsEnemies(state);
        
        // Check for collisions with charger (deals more damage)
        if (_isChargerActive && _chargerState != null && _chargerState.IsAlive && !state.IsInvincible)
        {
            // Check if player is colliding with the charger
            if (Math.Abs(state.PlayerPosition.X - _chargerState.X) < 0.5f && Math.Abs(state.PlayerPosition.Y - _chargerState.Y) < 0.5f)
            {
                // Player takes more damage from charger (2 instead of 1)
                state.CurrentHealth -= 2;
                Console.WriteLine($"Player hit by charger! Health: {state.CurrentHealth}");
                
                // Apply stronger knockback
                ApplyKnockback(state, new Vector2(_chargerState.X, _chargerState.Y), 1.0f); // Double knockback distance
            }
        }

        AdvanceKnockback(state);

        _swordPresenter.Update(state);
    }

    private void AdvanceInvisibility(GameState state)
    {
        if (!state.IsInvincible)
            return;
        
        state.InvincibilityTimer += Raylib.GetFrameTime();
        if (state.InvincibilityTimer >= GameConstants.InvincibilityDuration)
        {
            state.IsInvincible = false;
            state.InvincibilityTimer = 0f;
        }
    }

    private void AdvanceExplosions(GameState state)
    {
        for (int i = state.Explosions.Count - 1; i >= 0; i--)
        {
            state.Explosions[i].Timer += Raylib.GetFrameTime();
            if (state.Explosions[i].Timer >= GameConstants.ExplosionDuration)
            {
                // Spawn gold at the explosion location when the explosion finishes
                state.GoldItems.Add(new GoldItem { 
                    Position = state.Explosions[i].Position with {},
                    Value = _random.Next(3, 8)  // Enemies drop more valuable gold (3-7)
                });
                
                // Remove the explosion
                state.Explosions.RemoveAt(i);
            }
        }
    }

    private void AdvanceFlyingGold(GameState state)
    {
        for (int i = state.FlyingGold.Count - 1; i >= 0; i--)
        {
            state.FlyingGold[i].Timer += Raylib.GetFrameTime();
            if (state.FlyingGold[i].Timer >= GameConstants.GoldFlyDuration)
            {
                // Add the gold value to player's total when animation completes
                state.PlayerGold += state.FlyingGold[i].Value;
                
                // Remove the flying gold
                state.FlyingGold.RemoveAt(i);
            }
        }
    }

    private void HandlePlayerBumpsEnemies(GameState state)
    {
        if (state.IsInvincible)
            return;

        foreach (var enemy in state.Enemies)
        {
            if (!enemy.IsAlive)
                continue;

            if (Math.Abs(state.PlayerPosition.X - enemy.Position.X) < 0.5f && Math.Abs(state.PlayerPosition.Y - enemy.Position.Y) < 0.5f)
            {
                state.CurrentHealth--;
                Console.WriteLine($"Player hit by enemy! Health: {state.CurrentHealth}");
                
                ApplyKnockback(state, new Vector2(enemy.Position.X, enemy.Position.Y));
                
                break;
            }
        
        }
    }

    private void AdvanceKnockback(GameState state)
    {
        // Update knockback effect
        if (!state.IsKnockedBack)
            return;

        state.KnockbackTimer += Raylib.GetFrameTime();
        
        // Apply knockback movement during the knockback duration
        if (state.KnockbackTimer < GameConstants.KnockbackDuration)
        {
            // Calculate knockback distance for this frame
            float frameKnockback = GameConstants.KnockbackDistance * Raylib.GetFrameTime() * (1.0f / GameConstants.KnockbackDuration);
            
            // Calculate target position
            float targetX = state.PlayerPosition.X;
            float targetY = state.PlayerPosition.Y;
            
            // Determine target position based on knockback direction
            switch (state.KnockbackDirection)
            {
                case Direction.Left:
                    targetX = state.PlayerPosition.X - frameKnockback;
                    break;
                case Direction.Right:
                    targetX = state.PlayerPosition.X + frameKnockback;
                    break;
                case Direction.Up:
                    targetY = state.PlayerPosition.Y - frameKnockback;
                    break;
                case Direction.Down:
                    targetY = state.PlayerPosition.Y + frameKnockback;
                    break;
            }
            
            // Only move if the target position is walkable
            int checkX = (int)Math.Floor(targetX);
            int checkY = (int)Math.Floor(targetY);
            if (_mapUtil.IsWalkableTile(state.Map, checkX, checkY))
            {
                state.PlayerPosition.X = (int)targetX;
                state.PlayerPosition.Y = (int)targetY;
            }
            else
            {
                // If we hit a barrier, stop the knockback
                state.IsKnockedBack = false;
                state.KnockbackTimer = 0f;
            }
        }
        
        // End knockback effect
        if (state.KnockbackTimer >= GameConstants.KnockbackDuration)
        {
            state.IsKnockedBack = false;
            state.KnockbackTimer = 0f;
        }
    }

    private void GenerateFreeGold(GameState state)
    {
        state.PlayerGold += 100;
                    
        // Optional: Add a visual indicator that gold was added
        int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        state.FlyingGold.Add(new FlyingGold { 
            StartPosition = new Coord2dFloat(
                screenWidth / 2,
                ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale / 2),
            Value = 100,
            Timer = 0f
        });
    }

    private void CollectGold(GameState state)
    {
        // Find any gold within one square of the player's position
        for (int i = state.GoldItems.Count - 1; i >= 0; i--)
        {
            // Check if gold is at the player's position or one square away
            if (Math.Abs(state.GoldItems[i].Position.X - state.PlayerPosition.X) <= 1 && 
                Math.Abs(state.GoldItems[i].Position.Y - state.PlayerPosition.Y) <= 1)
            {
                // Create flying gold animation
                state.FlyingGold.Add(new FlyingGold { 
                    StartPosition = new Coord2dFloat(
                        100 + state.GoldItems[i].Position.X * 40,
                        100 + state.GoldItems[i].Position.Y * 40),
                    Value = state.GoldItems[i].Value,
                    Timer = 0f
                });
                
                // Remove the collected gold from the map
                state.GoldItems.RemoveAt(i);
                
                // Only collect one gold piece per move (in case multiple end up in same spot)
                break;
            }
        }
    }

    private void CollectHealthPickup(GameState state)
    {
        // Find any health pickup within one square of the player's position
        for (int i = state.HealthPickupState.HealthPickups.Count - 1; i >= 0; i--)
        {
            // Check if health pickup is at the player's position or one square away
            if (Math.Abs(state.HealthPickupState.HealthPickups[i].X - state.PlayerPosition.X) <= 1 && 
                Math.Abs(state.HealthPickupState.HealthPickups[i].Y - state.PlayerPosition.Y) <= 1)
            {
                // Add health to the player
                state.CurrentHealth = Math.Min(ScreenConstants.MaxHealth, state.CurrentHealth + state.HealthPickupState.HealthPickups[i].HealAmount);
                
                // Remove the collected health pickup
                state.HealthPickupState.HealthPickups.RemoveAt(i);
                
                // Only collect one health pickup per move
                break;
            }
        }
    }

    // Also update the charger movement logic
    private void UpdateCharger(GameState state)
    {
        if (!_isChargerActive || _chargerState == null || !_chargerState.IsAlive)
            return;
        
        float frameTime = Raylib.GetFrameTime();
        
        // Update charger invincibility
        if (_chargerState.IsInvincible)
        {
            _chargerState.InvincibilityTimer += frameTime;
            if (_chargerState.InvincibilityTimer >= 0.5f) // 0.5 seconds of invincibility
            {
                _chargerState.IsInvincible = false;
            }
        }
        
        // Update charger movement
        _chargerState.MoveTimer += frameTime;
        if (_chargerState.MoveTimer >= GameConstants.ChargerSpeed) // Charger moves faster than regular enemies
        {
            _chargerState.MoveTimer = 0f;
            
            // Calculate direction to player
            float dx = 0;
            float dy = 0;
            
            // Charger AI: move directly toward player
            if (_chargerState.X < state.PlayerPosition.X) dx = GameConstants.PlayerMoveSpeed;
            else if (_chargerState.X > state.PlayerPosition.X) dx = -GameConstants.PlayerMoveSpeed;
            
            if (_chargerState.Y < state.PlayerPosition.Y) dy = GameConstants.PlayerMoveSpeed;
            else if (_chargerState.Y > state.PlayerPosition.Y) dy = -GameConstants.PlayerMoveSpeed;
            
            // Try to move horizontally first
            if (dx != 0)
            {
                // Check if the new position is walkable
                if (_mapUtil.IsWalkableTile(state.Map, (int)Math.Floor((double)_chargerState.X + (double)dx), (int)Math.Floor((double)_chargerState.Y)))
                {
                    _chargerState.X += dx;
                }
                // If horizontal movement is blocked, try vertical
                else if (dy != 0 && _mapUtil.IsWalkableTile(state.Map, (int)Math.Floor((double)_chargerState.X), (int)Math.Floor((double)_chargerState.Y + (double)dy)))
                {
                    _chargerState.Y += dy;
                }
            }
            // If no horizontal movement, try vertical
            else if (dy != 0)
            {
                // Check if the new position is walkable
                if (_mapUtil.IsWalkableTile(state.Map, (int)Math.Floor((double)_chargerState.X), (int)Math.Floor((double)_chargerState.Y + (double)dy)))
                {
                    _chargerState.Y += dy;
                }
            }
            
            // Check for collision with player
            if (Math.Abs(_chargerState.X - state.PlayerPosition.X) < 0.5f && Math.Abs(_chargerState.Y - state.PlayerPosition.Y) < 0.5f)
            {
                // Only damage player if not invincible
                if (!state.IsInvincible)
                {
                    // Charger does 2 damage
                    state.CurrentHealth -= 2;
                    
                    // Apply stronger knockback
                    ApplyKnockback(state, new Vector2(_chargerState.X, _chargerState.Y), 1.5f);
                    
                    // Make player briefly invincible
                    state.IsInvincible = true;
                    state.InvincibilityTimer = 0f;
                }
            }
        }
    }

    private void SpawnGoldItem(GameState state)
    {
        // Create a list of all valid spawn positions (floor tiles only)
        var validPositions = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (int y = 0; y < state.Map.Count; y++)
        {
            string line = state.Map[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '.')  // Only consider floor tiles
                {
                    // Check if position is not occupied by player, enemies, or other gold
                    if ((x != state.PlayerPosition.X || y != state.PlayerPosition.Y) &&
                        !state.Enemies.Any(e => e.IsAlive && e.Position.X == x && e.Position.Y == y) &&
                        !state.GoldItems.Any(g => g.Position.X == x && g.Position.Y == y))
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
        state.GoldItems.Add(new GoldItem
        {
            Position = new Coord2dFloat(newX, newY),
            Value = _random.Next(1, 6)
        });  // Gold worth 1-5
    }

    private void SpawnHealthPickup(GameState state)
    {
        // Create a list of all valid spawn positions (floor tiles only)
        var validPositions = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (int y = 0; y < state.Map.Count; y++)
        {
            string line = state.Map[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '.')  // Only consider floor tiles
                {
                    // Check if position is not occupied by player, enemies, gold, or other health pickups
                    if ((x != state.PlayerPosition.X || y != state.PlayerPosition.Y) &&
                        !state.Enemies.Any(e => e.IsAlive && e.Position.X == x && e.Position.Y == y) &&
                        !state.GoldItems.Any(g => g.Position.X == x && g.Position.Y == y) &&
                        !state.HealthPickupState.HealthPickups.Any(h => h.X == x && h.Y == y))
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
        state.HealthPickupState.HealthPickups.Add(new HealthPickup { X = newX, Y = newY, HealAmount = 20 });  // Restore 20 health
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

    private void InitializeShop(GameState state)
    {
        // Regular items section
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Name = "Health Potion",
            Description = "Restores 25 health",
            Price = 10,
            OnPurchase = () => { state.CurrentHealth = Math.Min(10, state.CurrentHealth + 25); },
            Category = ShopCategory.Consumable
        });
        
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Name = "Faster Sword",
            Description = "Reduces sword cooldown by 0.2s",
            Price = 25,
            OnPurchase = () => { 
                if (state.SwordState.SwordCooldown > 0.3f) // Don't let it go below 0.3s
                    state.SwordState.SwordCooldown -= 0.2f; 
            },
            Category = ShopCategory.Upgrade
        });
        
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Name = "Longer Sword",
            Description = "Increases sword reach",
            Price = 30,
            OnPurchase = () => { state.SwordState.SwordReach++; },
            Category = ShopCategory.Upgrade
        });
        
        state.ShopState.ShopInventory.Add(new ShopItem
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
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Name = "--- Weapons ---",
            Description = "Coming soon...",
            Price = 0,
            OnPurchase = () => { /* Do nothing, this is just a header */ },
            Category = ShopCategory.Header
        });
        
        // Add crossbow to weapons section
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Name = "Crossbow",
            Description = "Fires bolts at enemies from a distance",
            Price = 75,
            OnPurchase = () => { state.CrossbowState.HasCrossbow = true; },
            Category = ShopCategory.Weapon
        });
    }


    private void HandleShopInput(GameState state)
    {
        while (state.KeyEvents.Count > 0)
        {
            var key = state.KeyEvents.Dequeue();
            
            if (key == KeyboardKey.Escape)
            {
                state.CurrentScreen = GameScreenEnum.Adventure;
            }
            else if (key == KeyboardKey.Up)
            {
                // Move selection up, skipping headers
                int newSelection = state.ShopState.SelectedShopItem - 1;
                while (newSelection >= 0 && state.ShopState.ShopInventory[newSelection].Category == ShopCategory.Header)
                {
                    newSelection--;
                }
                state.ShopState.SelectedShopItem = Math.Max(0, newSelection);
            }
            else if (key == KeyboardKey.Down)
            {
                // Move selection down, skipping headers
                int newSelection = state.ShopState.SelectedShopItem + 1;
                while (newSelection < state.ShopState.ShopInventory.Count && state.ShopState.ShopInventory[newSelection].Category == ShopCategory.Header)
                {
                    newSelection++;
                }
                state.ShopState.SelectedShopItem = Math.Min(state.ShopState.ShopInventory.Count - 1, newSelection);
            }
            else if (key == KeyboardKey.Enter)
            {
                // Try to purchase the selected item
                if (state.ShopState.SelectedShopItem >= 0 && state.ShopState.SelectedShopItem < state.ShopState.ShopInventory.Count)
                {
                    var item = state.ShopState.ShopInventory[state.ShopState.SelectedShopItem];
                    // Don't allow purchasing headers
                    if (item.Category != ShopCategory.Header && state.PlayerGold >= item.Price)
                    {
                        // Deduct gold
                        state.PlayerGold -= item.Price;
                        
                        // Apply the purchase effect
                        item.OnPurchase();
                    }
                }
            }
        }
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
                if (!enemy.IsAlive) continue;
                
                // Check if bolt is within 0.5 tiles of enemy (for hit detection)
                if (Math.Abs(bolt.X - enemy.Position.X) < 0.5f && Math.Abs(bolt.Y - enemy.Position.Y) < 0.5f)
                {
                    // Kill the enemy
                    enemy.IsAlive = false;
                    
                    // Create explosion at enemy position
                    state.Explosions.Add(new ExplosionState { 
                        Position = enemy.Position with {},
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
    private void HandleChargerDamage(GameState state, bool fromSwinging)
    {
        if (_isChargerActive && _chargerState != null && _chargerState.IsAlive && !_chargerState.IsInvincible)
        {
            // Increment hit counter
            _chargerState.HitCount++;
            
            // Update displayed health
            _chargerState.Health = GameConstants.ChargerHealth - _chargerState.HitCount;
            
            Console.WriteLine($"COLLISION FIX: Charger hit {_chargerState.HitCount} times. Health display: {_chargerState.Health}");
            
            // Make charger briefly invincible to prevent multiple hits
            _chargerState.IsInvincible = true;
            _chargerState.InvincibilityTimer = 0f;
            
            // Only kill if hit exactly 5 times
            if (_chargerState.HitCount >= 5)
            {
                Console.WriteLine("COLLISION FIX: Charger defeated after 5 hits!");
                _chargerState.IsAlive = false;
                
                // Create explosion at charger position
                state.Explosions.Add(new ExplosionState {
                    Position = new Coord2dFloat(_chargerState.X, _chargerState.Y),
                    Timer = 0f 
                });
                
                // Spawn more valuable gold for killing the charger
                for (int i = 0; i < 3; i++)
                {
                    state.GoldItems.Add(new GoldItem {
                        Position = new Coord2dFloat(_chargerState.X + i, _chargerState.Y),
                        Value = _random.Next(10, 21)  // 10-20 gold per drop, 3 drops
                    });
                }
                
                _isChargerActive = false;
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

            isPositionValid = (Math.Abs(newX - state.PlayerPosition.X) > 3 || Math.Abs(newY - state.PlayerPosition.Y) > 3) &&
                              !state.Enemies.Any(e => e.IsAlive && Math.Abs(e.Position.X - newX) < 0.5f && Math.Abs(e.Position.Y - newY) < 0.5f);

            if (isPositionValid)
                break;
        }

        if (!isPositionValid)
            return;

        // Create a new charger with hardcoded health value
        state.Charger = new ChargerEnemyState { 
            X = newX, 
            Y = newY, 
            Health = GameConstants.ChargerHealth,
            IsAlive = true 
        };
        state.IsChargerActive = true;
        
        // Show the boss banner
        state.BannerText = "\u0001 Everybody's gangsta until the charger appears \u0001";
        state.IsBannerVisible = true;
        state.BannerTimer = 0;
        
        Console.WriteLine($"NEW IMPLEMENTATION: Spawned charger with {state.Charger.Health} health");
    }

    // Add this new method for applying knockback
    private void ApplyKnockback(GameState state, Vector2 sourcePosition, float multiplier = 1.0f)
    {
        // Determine knockback direction (away from the source)
        float dx = state.PlayerPosition.X - sourcePosition.X;
        float dy = state.PlayerPosition.Y - sourcePosition.Y;
        
        // If player is exactly on the enemy, use the player's facing direction
        if (Math.Abs(dx) < 0.1f && Math.Abs(dy) < 0.1f)
        {
            state.KnockbackDirection = state.ActionDirection switch
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

    // Add a new method to update the camera position
    private void UpdateCamera(GameState state)
    {
        // Calculate how far the player is from the camera center
        float deltaX = state.PlayerPosition.X - state.CameraState.X;
        float deltaY = state.PlayerPosition.Y - state.CameraState.Y;
        
        // If player is outside the dead zone, move the camera
        if (Math.Abs(deltaX) > GameConstants.CameraDeadZone)
        {
            // Move camera in the direction of the player
            state.CameraState.X += deltaX > 0 ? 
                Math.Min(deltaX - GameConstants.CameraDeadZone, 0.5f) : 
                Math.Max(deltaX + GameConstants.CameraDeadZone, -0.5f);
        }
        
        if (Math.Abs(deltaY) > GameConstants.CameraDeadZone)
        {
            // Move camera in the direction of the player
            state.CameraState.Y += deltaY > 0 ? 
                Math.Min(deltaY - GameConstants.CameraDeadZone, 0.5f) : 
                Math.Max(deltaY + GameConstants.CameraDeadZone, -0.5f);
        }
    }

    private void CheckSwordCollisions(GameState state, bool isSwinging)
    {
        if (!isSwinging)
            return;
        
        // Calculate sword position based on player position and direction
        float swordX = state.PlayerPosition.X;
        float swordY = state.PlayerPosition.Y;
        
        // Adjust position based on direction and sword reach
        switch (state.ActionDirection)
        {
            case Direction.Left:
                swordX -= state.SwordState.SwordReach;
                break;
            case Direction.Right:
                swordX += state.SwordState.SwordReach;
                break;
            case Direction.Up:
                swordY -= state.SwordState.SwordReach;
                break;
            case Direction.Down:
                swordY += state.SwordState.SwordReach;
                break;
        }
        
        // Check for collision with regular enemies
        foreach (var enemy in state.Enemies)
        {
            if (enemy.IsAlive)
            {
                // Check if sword is within 0.5 tiles of enemy (for hit detection)
                if (Math.Abs(swordX - enemy.Position.X) < 0.5f && Math.Abs(swordY - enemy.Position.Y) < 0.5f)
                {
                    // Kill the enemy
                    enemy.IsAlive = false;
                    
                    // Create explosion at enemy position
                    state.Explosions.Add(new ExplosionState {
                        Position = enemy.Position with {},
                        Timer = 0f 
                    });
                    
                    // Increment kill counter
                    _enemiesKilled++;
                    
                    // Check if we should spawn a charger
                    if (_enemiesKilled >= GameConstants.KillsForCharger && !_isChargerActive)
                    {
                        SpawnCharger(state);
                    }
                    
                    // If this was a swing hit, start cooldown
                    if (isSwinging)
                    {
                        state.SwordState.SwordOnCooldown = true;
                        state.SwordState.SwordCooldownTimer = 0f;
                    }
                    
                    break; // Only hit one enemy per swing/movement
                }
            }
        }
        
        // Check for collision with charger
        if (_isChargerActive && _chargerState != null && _chargerState.IsAlive)
        {
            // Check if sword is within 0.5 tiles of charger
            if (Math.Abs(swordX - _chargerState.X) < 0.5f && Math.Abs(swordY - _chargerState.Y) < 0.5f)
            {
                // Handle charger damage
                HandleChargerDamage(state, isSwinging);
                
                // If this was a swing hit, start cooldown
                if (isSwinging)
                {
                    state.SwordState.SwordOnCooldown = true;
                    state.SwordState.SwordCooldownTimer = 0f;
                }
            }
        }
    }

    // Add this new method to ensure player spawns on a walkable tile
    private void EnsurePlayerOnWalkableTile(GameState state)
    {
        // If player is already on a walkable tile, do nothing
        if (_mapUtil.IsWalkableTile(state.Map, (int)Math.Floor(state.PlayerPosition.X), (int)Math.Floor(state.PlayerPosition.Y)))
        {
            return;
        }
        
        // Search for a walkable tile in an expanding spiral pattern
        var maxSearchRadius = 20;  // Maximum search distance
        
        for (var radius = 1; radius <= maxSearchRadius; radius++)
        {
            // Check in a square pattern around the player
            for (var offsetX = -radius; offsetX <= radius; offsetX++)
            {
                for (var offsetY = -radius; offsetY <= radius; offsetY++)
                {
                    // Skip if not on the perimeter of the square
                    if (Math.Abs(offsetX) != radius && Math.Abs(offsetY) != radius)
                        continue;
                    
                    var testX = (int)Math.Floor(state.PlayerPosition.X + offsetX);
                    var testY = (int)Math.Floor(state.PlayerPosition.Y + offsetY);
                    
                    if (_mapUtil.IsWalkableTile(state.Map, testX, testY))
                    {
                        // Found a walkable tile, move player there
                        state.PlayerPosition.X = testX + 0.5f; // Center the player in the tile
                        state.PlayerPosition.Y = testY + 0.5f;
                        return;
                    }
                }
            }
        }
        
        // If no walkable tile found, force create a room at player position
        // This is a fallback that should rarely be needed
        int roomWidth = 7;
        int roomHeight = 5;
        int roomX = (int)Math.Floor(state.PlayerPosition.X) - roomWidth / 2;
        int roomY = (int)Math.Floor(state.PlayerPosition.Y) - roomHeight / 2;
        
        // Create a simple room
        for (int y = 0; y < roomHeight; y++)
        {
            while (state.Map.Count <= roomY + y)
            {
                state.Map.Add(new string(' ', roomX + roomWidth));
            }
            
            string line = state.Map[roomY + y];
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
            
            state.Map[roomY + y] = new string(lineChars);
        }
        
        // Place player in the center of the new room
        state.PlayerPosition.X = roomX + roomWidth / 2 + 0.5f;
        state.PlayerPosition.Y = roomY + roomHeight / 2 + 0.5f;
    }

    // Add this method to initialize the player position on a floor tile
    private void InitializePlayerPosition(GameState state)
    {
        // Create a list of all valid spawn positions (floor tiles only)
        var floorTiles = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (int y = 0; y < state.Map.Count; y++)
        {
            string line = state.Map[y];
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
        
        // Set player position (centered in the tile)
        state.PlayerPosition.X = newX + 0.5f;
        state.PlayerPosition.Y = newY + 0.5f;
        
        // Immediately center camera on player for initial spawn
        state.CameraState.X = state.PlayerPosition.X;
        state.CameraState.Y = state.PlayerPosition.Y;

        if (_mapUtil.IsWalkableTile(state.Map, (int)Math.Floor(state.PlayerPosition.X), (int)Math.Floor(state.PlayerPosition.Y)))
        {
            return;
        }
        
        // Search for a walkable tile in an expanding spiral pattern
        var maxSearchRadius = 20;  // Maximum search distance
        
        for (var radius = 1; radius <= maxSearchRadius; radius++)
        {
            // Check in a square pattern around the player
            for (var offsetX = -radius; offsetX <= radius; offsetX++)
            {
                for (var offsetY = -radius; offsetY <= radius; offsetY++)
                {
                    // Skip if not on the perimeter of the square
                    if (Math.Abs(offsetX) != radius && Math.Abs(offsetY) != radius)
                        continue;
                    
                    var testX = (int)Math.Floor(state.PlayerPosition.X + offsetX);
                    var testY = (int)Math.Floor(state.PlayerPosition.Y + offsetY);
                    
                    if (_mapUtil.IsWalkableTile(state.Map, testX, testY))
                    {
                        // Found a walkable tile, move player there
                        state.PlayerPosition.X = testX + 0.5f; // Center the player in the tile
                        state.PlayerPosition.Y = testY + 0.5f;
                        return;
                    }
                }
            }
        }
        
        // If no walkable tile found, force create a room at player position
        // This is a fallback that should rarely be needed
        int roomWidth = 7;
        int roomHeight = 5;
        int roomX = (int)Math.Floor(state.PlayerPosition.X) - roomWidth / 2;
        int roomY = (int)Math.Floor(state.PlayerPosition.Y) - roomHeight / 2;
        
        // Create a simple room
        for (int y = 0; y < roomHeight; y++)
        {
            while (state.Map.Count <= roomY + y)
            {
                state.Map.Add(new string(' ', roomX + roomWidth));
            }
            
            string line = state.Map[roomY + y];
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
            
            state.Map[roomY + y] = new string(lineChars);
        }
        
        // Place player in the center of the new room
        state.PlayerPosition.X = roomX + roomWidth / 2 + 0.5f;
        state.PlayerPosition.Y = roomY + roomHeight / 2 + 0.5f;
    }
}