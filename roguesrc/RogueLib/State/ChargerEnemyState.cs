namespace RogueLib.State;

public class ChargerEnemyState
{
    public float X { get; set; }
    public float Y { get; set; }
    public bool Alive { get; set; } = true;
    public float MoveTimer { get; set; } = 0f;
    public int Health { get; set; } = 5; // Health display
    public int HitCount { get; set; } = 0; // Count hits separately
    public bool IsInvincible { get; set; } = false; // Add invincibility flag
    public float InvincibilityTimer { get; set; } = 0f; // Add invincibility timer
}