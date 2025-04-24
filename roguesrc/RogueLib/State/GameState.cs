namespace RogueLib.State;

public class GameState
{
    public GameScreenEnum CurrentScreen { get; set; } = GameScreenEnum.Menu;

    public int PlayerGold { get; set; } = 0;
    
    public float PlayerX { get; set; } = 10f;
    public float PlayerY { get; set; } = 5f;
    public float PreviousX { get; set; } = 10f;
    public float PreviousY { get; set; } = 5f;
    public float MovementStartTime { get; set; } = 0f;
    public int CurrentHealth { get; set; } = 7;    
    
    // Add velocity properties
    public float VelocityX { get; set; } = 0f;
    public float VelocityY { get; set; } = 0f;
    
    public SwordState SwordState { get; set; } = new SwordState();

    public Direction LastDirection { get; set; } = Direction.Right;

    public Direction KnockbackDirection { get; set; } = Direction.Right;
    public bool IsKnockedBack { get; set; } = false;
    public float KnockbackTimer { get; set; } = 0f;

    public float TimeSinceLastMove { get; set; }

    // Add wobble animation state
    public float WobbleTimer { get; set; } = 0f;
    // Constants moved to GameConstants

    public List<Enemy> Enemies { get; } = [];
    public float EnemySpawnTimer { get; set; }
    
    public bool IsInvincible { get; set; } = false;
    public float InvincibilityTimer { get; set; } = 0f;
    public List<ExplosionState> Explosions { get; } = [];

    public readonly List<CrossbowBoltState> CrossbowBolts = [];
    
    public ShopState ShopState { get; set; } = new ShopState();

    public CameraState CameraState { get; set; } = new CameraState();

    public List<string> Map { get; set; } = [];

    public bool IsChargerActive { get; set; }
    public ChargerEnemyState? Charger { get; set; }
    
    // Banner state
    public bool IsBannerVisible { get; set; }
    public string BannerText { get; set; } = "";
    public float BannerTimer { get; set; }

    // Crossbow state
    public bool HasCrossbow { get; set; }
    public float CrossbowCooldown { get; set; } = 2.0f;
    public float CrossbowCooldownTimer { get; set; }
    public bool CrossbowOnCooldown { get; set; }
}