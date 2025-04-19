namespace RogueLib;

public class Enemy
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool Alive { get; set; } = true;
    public float MoveTimer { get; set; } = 0f;
}