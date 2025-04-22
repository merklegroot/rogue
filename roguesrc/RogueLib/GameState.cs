namespace RogueLib;

public class GameState
{
    public int PlayerX { get; set; } = 10;
    public int PlayerY { get; set; } = 5;
    public int CurrentHealth { get; set; } = 7;    
    
    // Added sword-related properties moved from ScreenPresenter
    public bool IsSwordSwinging { get; set; } = false;
    public float SwordSwingTime { get; set; } = 0f;

    public Direction LastDirection { get; set; } = Direction.Right;

    public Direction KnockbackDirection { get; set; } = Direction.Right;
    public bool IsKnockedBack { get; set; } = false;
    public float KnockbackTimer { get; set; } = 0f;
}