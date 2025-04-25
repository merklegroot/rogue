namespace RogueLib.Constants;

public static class GameConstants
{
    public const float SwordSwingDuration = 0.4f;
    public const float SwordCooldown = 0.5f;
    public const float EnemyMoveDelay = 0.5f;
    public const float EnemySpawnDelay = 1.0f;  // Back to original: spawn every 1.0 seconds
    public const int MaxEnemies = 8;  // Increased to allow for more enemies while maintaining chunk distribution
    public const float ExplosionDuration = 0.5f;  // How long each explosion lasts
    public const float BoltSpeed = 8.0f; // Bolts move 8 tiles per second
    public const int MaxGoldItems = 3;  // Reduced from 5 to 3
    public const float InvincibilityDuration = 1.0f;  
    public const float PlayerMoveSpeed = 2.0f;
    public const float CameraDeadZone = 5.0f;
    public const float PlayerMaxVelocity = 15.0f;  // Maximum velocity in any direction
    public const float PlayerFriction = 0.9f;      // Friction factor (0.9 means 10% reduction per frame)
    public const int ChunkSize = 20;  // 20 spaces in each direction
    public const float WobbleFrequency = 1000.0f; // Time in milliseconds for one complete wobble cycle
    public const float WobbleAmount = 0.025f; // How much to scale
    public const int ChargerHealth = 5;
    public const float GoldFlyDuration = 0.3f;
    public const float HealthSpawnInterval = 30f;
    public const int KillsForCharger = 10;
    public const float ChargerSpeed = 0.3f; // Charger moves faster than regular enemies
    public const float KnockbackDuration = 0.08f;
    public const float KnockbackDistance = 0.5f;
}