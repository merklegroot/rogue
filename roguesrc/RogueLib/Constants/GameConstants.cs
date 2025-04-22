namespace RogueLib.Constants;

public static class GameConstants
{
    public const float SwordSwingDuration = 0.25f;  // Reduced from 0.3f to make animation even faster
    
    public const float EnemyMoveDelay = 0.8f;  // Move every 0.8 seconds
    public const float EnemySpawnDelay = 1.0f;  // Back to original: spawn every 1.0 seconds
    public const int MaxEnemies = 3;  // Back to original: maximum of 3 enemies
    public const float ExplosionDuration = 0.5f;  // How long each explosion lasts
}