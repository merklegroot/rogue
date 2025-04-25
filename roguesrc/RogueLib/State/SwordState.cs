namespace RogueLib.State;

public class SwordState
{
    public bool IsSwordSwinging { get; set; } = false;
    public float SwordSwingTime { get; set; } = 0f;
    public float SwordCooldownTimer { get; set; } = 0f;
    public float SwordCooldown { get; set; } = 1.0f;
    public bool SwordOnCooldown { get; set; } = false;
    public int SwordReach { get; set; } = 2;
    public Direction SwingDirection { get; set; } = Direction.Right;
}