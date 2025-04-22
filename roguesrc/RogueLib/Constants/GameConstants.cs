namespace RogueLib.Constants;

public static class GameConstants
{
    public const float SwordSwingDuration = 0.25f;  // Reduced from 0.3f to make animation even faster
    
    public const float EnemyMoveDelay = 0.8f;  // Move every 0.8 seconds
    public const float EnemySpawnDelay = 1.0f;  // Back to original: spawn every 1.0 seconds
    public const int MaxEnemies = 3;  // Back to original: maximum of 3 enemies
    public const float ExplosionDuration = 0.5f;  // How long each explosion lasts

    public const float BoltSpeed = 8.0f; // Bolts move 8 tiles per second
    
    public const int MaxGoldItems = 3;  // Reduced from 5 to 3

    public const float InvincibilityDuration = 1.0f;  
}