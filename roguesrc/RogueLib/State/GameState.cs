using Raylib_cs;
using RogueLib.Models;

namespace RogueLib.State;

public class GameState
{
    public Queue<KeyboardKey> KeyEvents { get; set; } = new();

    public GameScreenEnum CurrentScreen { get; set; } = GameScreenEnum.Menu;

    public int PlayerGold { get; set; } = 0;
    public Coord2dFloat PlayerPosition { get; set; } = new(10f, 5f);

    public Coord2dFloat PreviousPlayerPosition { get; set; } = new(10f, 5f);
    public float MovementStartTime { get; set; } = 0f;
    public int CurrentHealth { get; set; } = 7;    
    
    // Add velocity properties
    public float VelocityX { get; set; } = 0f;
    public float VelocityY { get; set; } = 0f;
    
    public SwordState SwordState { get; set; } = new SwordState();

    public Direction ActionDirection { get; set; } = Direction.Right;

    public Direction KnockbackDirection { get; set; } = Direction.Right;
    public bool IsKnockedBack { get; set; } = false;
    public float KnockbackTimer { get; set; } = 0f;

    public float TimeSinceLastMove { get; set; }

    // Add wobble animation state
    public float WobbleTimer { get; set; } = 0f;
    // Constants moved to GameConstants

    public List<EnemyState> Enemies { get; } = [];
    public List<SpinnerEnemyState> Spinners { get; } = [];
    public float EnemySpawnTimer { get; set; }
    
    public bool IsInvincible { get; set; } = false;
    public float InvincibilityTimer { get; set; } = 0f;
    public bool IsEnemySpawnEnabled { get; set; } = true;  // Default to true
    public bool IsEnemyMovementEnabled { get; set; } = true;  // Default to true
    public List<ExplosionState> Explosions { get; } = [];

    public readonly List<CrossbowBoltState> CrossbowBolts = [];
    
    public ShopState ShopState { get; set; } = new ShopState();

    public Coord2dFloat CameraState { get; set; } = new Coord2dFloat();

    public List<string> Map { get; set; } = [];

    public ChargerEnemyState Charger { get; set; } = new ChargerEnemyState();
    
    // Banner state
    public bool IsBannerVisible { get; set; }
    public string BannerText { get; set; } = "";
    public float BannerTimer { get; set; }

    public readonly List<GoldItem> GoldItems = [];
    
    public readonly List<FlyingGold> FlyingGold = [];

    public CrossbowState CrossbowState { get; set; } = new CrossbowState();

    public HealthPickupState HealthPickupState { get; set; } = new HealthPickupState();

    public bool ShouldEnableCrtEffect { get; set; } = true;  // Default to true

    public bool ShouldShowDebugPanel { get; set; }

    public string? CurrentSarcasticRemark { get; set; }
    public float ShaderTime { get; set; }
    public int EnemiesKilled { get; set; }
    public bool DidGameJustStart { get; set; } = true;

    public int SelectedCharIndex { get; set; } = 0;
}