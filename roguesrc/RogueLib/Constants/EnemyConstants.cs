namespace RogueLib.Constants;

public static class EnemyConstants
{
    public const float EnemyMoveFrequency = 0.5f;
    public const float EnemySpawnDelay = 1.0f;  // Back to original: spawn every 1.0 seconds
    public const float EnemyMoveSpeed = 3.0f;  // Distance enemies move per step
    public const int MaxEnemies = 20;  // Increased to allow for more enemies while maintaining chunk distribution
}