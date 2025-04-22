namespace RogueLib.State;

public class GameState
{
    public GameScreenEnum CurrentScreen { get; set; } = GameScreenEnum.Menu;

    public int PlayerGold { get; set; } = 0;
    
    public int PlayerX { get; set; } = 10;
    public int PlayerY { get; set; } = 5;
    public int CurrentHealth { get; set; } = 7;    
    
    public SwordState SwordState { get; set; } = new SwordState();

    public Direction LastDirection { get; set; } = Direction.Right;

    public Direction KnockbackDirection { get; set; } = Direction.Right;
    public bool IsKnockedBack { get; set; } = false;
    public float KnockbackTimer { get; set; } = 0f;

    public float TimeSinceLastMove { get; set; }

    public List<Enemy> Enemies { get; } = [];
    public float EnemySpawnTimer { get; set; }
    
    public bool IsInvincible { get; set; } = false;
    public float InvincibilityTimer { get; set; } = 0f;
    public List<ExplosionState> Explosions { get; } = [];

    public readonly List<CrossbowBoltState> CrossbowBolts = [];
    
    public ShopState ShopState { get; set; } = new ShopState();
}