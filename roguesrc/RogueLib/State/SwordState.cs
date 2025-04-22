namespace RogueLib.State;

public class SwordState
{
    // Added sword-related properties moved from ScreenPresenter
    public bool IsSwordSwinging { get; set; } = false;
    public float SwordSwingTime { get; set; } = 0f;
}