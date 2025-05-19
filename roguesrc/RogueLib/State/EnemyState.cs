using RogueLib.Models;

namespace RogueLib;


public interface IEnemyContext { }

public class EnemyState
{
    public Coord2dFloat Position { get; set; } = new(0f, 0f);
    public Coord2dFloat TargetPosition { get; set; } = new(0f, 0f);

    public bool IsAlive { get; set; } = true;
    public bool IsMoving { get; set; } = false;
    public float MoveTimer { get; set; } = 0f;

    public required EnemyEnum EnemyType { get; set; }
}

public class EnemyState<TEnemyContext> : EnemyState where TEnemyContext : IEnemyContext
{
    public required TEnemyContext Context { get; set; }
}

public class SpinnerEnemyContext : IEnemyContext
{
    public float SpinAngle { get; set; } = 0f;
    public float DirectionAngle { get; set; } = 0f;
    public float MoveSpeed { get; set; } = 2.5f; // Slightly slower than charger
    public bool IsMoving { get; set; } = false; // Only moves after being hit
}