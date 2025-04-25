using RogueLib.Models;

namespace RogueLib;

public class Enemy
{
    public Coord2dFloat Position { get; set; } = new(0f, 0f);

    public bool IsAlive { get; set; } = true;
    public float MoveTimer { get; set; } = 0f;
}