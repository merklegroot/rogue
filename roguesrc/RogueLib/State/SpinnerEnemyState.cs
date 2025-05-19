using RogueLib.Models;

namespace RogueLib.State;

public class SpinnerEnemyState
{
    public Coord2dFloat Position { get; set; } = new(0f, 0f);
    public float DirectionAngle { get; set; } // In radians
    public float SpinAngle { get; set; } // For blade rotation, in radians
    public bool IsAlive { get; set; } = true;
    public float MoveSpeed { get; set; } = 2.5f; // Slightly slower than charger
    public bool IsMoving { get; set; } = false; // Only moves after being hit
}