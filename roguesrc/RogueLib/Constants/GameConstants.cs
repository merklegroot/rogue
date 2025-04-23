namespace RogueLib.Constants;

public static class GameConstants
{
    public const float SwordSwingDuration = 0.2f;
    public const float SwordCooldown = 0.5f;
    public const float MoveDelay = 0.2f;  // Controls how often the player can make a new movement
    public const float EnemyMoveDelay = 0.5f;
    public const float EnemySpawnDelay = 1.0f;  // Back to original: spawn every 1.0 seconds
    public const int MaxEnemies = 8;  // Increased to allow for more enemies while maintaining chunk distribution
    public const float ExplosionDuration = 0.5f;  // How long each explosion lasts

    public const float BoltSpeed = 8.0f; // Bolts move 8 tiles per second
    
    public const int MaxGoldItems = 3;  // Reduced from 5 to 3

    public const float InvincibilityDuration = 1.0f;  

    // Chunk constants
    public const int ChunkSize = 20;  // 20 spaces in each direction

    // Wobble animation constants
    public const float WobbleFrequency = 1000.0f; // Time in milliseconds for one complete wobble cycle
    public const float WobbleAmount = 0.025f; // How much to scale

    public const int ChargerHealth = 5;
}