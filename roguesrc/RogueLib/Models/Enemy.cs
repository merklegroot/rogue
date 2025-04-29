using RogueLib.Models;

namespace RogueLib;

public class Enemy
{
    public Coord2dFloat Position { get; set; } = new(0f, 0f);
    public Coord2dFloat TargetPosition { get; set; } = new(0f, 0f);

    public bool IsAlive { get; set; } = true;
    public bool IsMoving { get; set; } = false;
    public float MoveTimer { get; set; } = 0f;
}