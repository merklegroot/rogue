using System.Numerics;
using Raylib_cs;
using RogueLib.State;
using RogueLib.Constants;
using RogueLib.Utils;
using RogueLib.Models;
namespace RogueLib.Handlers;

public interface IUpdateEnemiesHandler
{
    void Handle(GameState state);
}

public class UpdateEnemiesHandler : IUpdateEnemiesHandler
{
    private readonly Random _random = new();
    private readonly IMapUtil _mapUtil;

    public UpdateEnemiesHandler(IMapUtil mapUtil)
    {
        _mapUtil = mapUtil;
    }

    public void Handle(GameState state)
    {
        if (!state.IsEnemyMovementEnabled)
            return;

        float frameTime = Raylib.GetFrameTime();       

        foreach (var enemy in state.Enemies)
        {
            HandleEnemy(state, enemy, frameTime);
        }
        foreach (var spinner in state.Spinners)
        {
            HandleSpinner(state, spinner, frameTime);
        }
    }

    private void HandleEnemy(GameState state, EnemyState enemy, float frameTime)
    {
        if (!enemy.IsAlive)
            return;

        // Update movement timer
        enemy.MoveTimer += frameTime;
        var moveAmount = EnemyConstants.EnemyMoveSpeed * frameTime;

        // If not currently moving and timer expired, choose new direction
        if (!enemy.IsMoving && enemy.MoveTimer >= EnemyConstants.EnemyMoveFrequency)
        {
            enemy.MoveTimer = 0f;
            ChooseNewDirection(state, enemy);
        }

        // If moving, continue movement
        if (enemy.IsMoving)
        {
            // Calculate movement step
            float stepX = enemy.TargetPosition.X - enemy.Position.X;
            float stepY = enemy.TargetPosition.Y - enemy.Position.Y;
            float distance = (float)Math.Sqrt(stepX * stepX + stepY * stepY);

            // Normalize movement if not zero
            if (distance > 0)
            {
                stepX = (stepX / distance) * moveAmount;
                stepY = (stepY / distance) * moveAmount;
            }

            // Calculate new position
            float newX = enemy.Position.X + stepX;
            float newY = enemy.Position.Y + stepY;

            // Check if we've reached the target or hit an obstacle
            bool hasReachedTarget = Math.Abs(newX - enemy.TargetPosition.X) < 0.1f && 
                                Math.Abs(newY - enemy.TargetPosition.Y) < 0.1f;
            
            bool hasHitObstacle = !_mapUtil.IsWalkableTile(state.Map, (int)Math.Floor(newX), (int)Math.Floor(newY));

            if (hasReachedTarget || hasHitObstacle)
            {
                // Stop movement
                enemy.IsMoving = false;
                if (hasReachedTarget)
                {
                    enemy.Position = new Coord2dFloat(
                        enemy.TargetPosition.X,
                        enemy.TargetPosition.Y
                    );
                }
            }
            else
            {
                // Continue movement
                enemy.Position = new Coord2dFloat(newX, newY);
            }
        }

        HandleCollisionWithPlayer(state, enemy);
        
    }

    private void HandleSpinner(GameState state, SpinnerEnemyState spinner, float frameTime)
    {
        if (!spinner.IsAlive)
            return;

        // Spin the blades
        float spinSpeed = 4.0f; // radians per second
        spinner.SpinAngle += spinSpeed * frameTime;
        if (spinner.SpinAngle > MathF.PI * 2) spinner.SpinAngle -= MathF.PI * 2;

        if (!spinner.IsMoving)
            return;

        // Move in current direction
        float dx = MathF.Cos(spinner.DirectionAngle) * spinner.MoveSpeed * frameTime;
        float dy = MathF.Sin(spinner.DirectionAngle) * spinner.MoveSpeed * frameTime;
        float newX = spinner.Position.X + dx;
        float newY = spinner.Position.Y + dy;

        // Bounce off walls (non-walkable tiles)
        bool bounced = false;
        if (!_mapUtil.IsWalkableTile(state.Map, (int)MathF.Floor(newX), (int)MathF.Floor(newY)))
        {
            // Try horizontal bounce
            float testX = spinner.Position.X + dx;
            float testY = spinner.Position.Y;
            if (_mapUtil.IsWalkableTile(state.Map, (int)MathF.Floor(testX), (int)MathF.Floor(testY)))
            {
                spinner.DirectionAngle = MathF.PI - spinner.DirectionAngle;
                bounced = true;
            }
            // Try vertical bounce
            else if (_mapUtil.IsWalkableTile(state.Map, (int)MathF.Floor(spinner.Position.X), (int)MathF.Floor(spinner.Position.Y + dy)))
            {
                spinner.DirectionAngle = -spinner.DirectionAngle;
                bounced = true;
            }
            else
            {
                // Reverse direction if stuck
                spinner.DirectionAngle += MathF.PI;
                bounced = true;
            }
            // Normalize
            if (spinner.DirectionAngle > MathF.PI * 2) spinner.DirectionAngle -= MathF.PI * 2;
            if (spinner.DirectionAngle < 0) spinner.DirectionAngle += MathF.PI * 2;
            if (bounced)
            {
                // Move a small step in the new direction to avoid getting stuck
                const float step = 0.1f;
                spinner.Position = new Coord2dFloat
                {
                    X = spinner.Position.X + MathF.Cos(spinner.DirectionAngle) * step,
                    Y = spinner.Position.Y + MathF.Sin(spinner.DirectionAngle) * step
                };
                
            }
            return;
        }

        spinner.Position = new Coord2dFloat(newX, newY);
    }

    private void HandleCollisionWithPlayer(GameState state, EnemyState enemy)
    {
        if (state.IsInvincible || !IsCollidingWithPlayer(state, enemy))
            return;
        
        state.CurrentHealth--;
        Console.WriteLine($"Player hit by enemy! Health: {state.CurrentHealth}");
        
        // Apply knockback
        ApplyKnockback(state, new Vector2(enemy.Position.X, enemy.Position.Y));
    }

    private bool IsCollidingWithPlayer(GameState state, EnemyState enemy)
    {
        return Math.Abs(enemy.Position.X - state.PlayerPosition.X) < 0.5f && 
               Math.Abs(enemy.Position.Y - state.PlayerPosition.Y) < 0.5f;
    }

    private void ChooseNewDirection(GameState state, EnemyState enemy)
    {
        // List of possible directions (including diagonals)
        var directions = new List<(int dx, int dy)>
        {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1),           (0, 1),
            (1, -1),  (1, 0),  (1, 1)
        };

        // Shuffle directions
        directions = directions.OrderBy(x => _random.Next()).ToList();

        // Try each direction until we find a valid one
        foreach (var (dx, dy) in directions)
        {
            var newX = (int)Math.Floor(enemy.Position.X) + dx;
            var newY = (int)Math.Floor(enemy.Position.Y) + dy;

            if (_mapUtil.IsWalkableTile(state.Map, newX, newY))
            {
                // Set target position (center of the tile)
                enemy.TargetPosition = new Coord2dFloat(newX + 0.5f, newY + 0.5f);
                enemy.IsMoving = true;
                return;
            }
        }

        // If no valid direction found, stay in place
        enemy.IsMoving = false;
    }

    private void ApplyKnockback(GameState state, Vector2 sourcePosition)
    {
        // Determine knockback direction (away from the source)
        float dx = state.PlayerPosition.X - sourcePosition.X;
        float dy = state.PlayerPosition.Y - sourcePosition.Y;
        
        // If player is exactly on the enemy, use the player's facing direction
        if (Math.Abs(dx) < 0.1f && Math.Abs(dy) < 0.1f)
        {
            state.KnockbackDirection = state.ActionDirection switch
            {
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                _ => Direction.Right
            };
        }
        else if (Math.Abs(dx) > Math.Abs(dy))
        {
            // Knockback horizontally
            state.KnockbackDirection = dx > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            // Knockback vertically
            state.KnockbackDirection = dy > 0 ? Direction.Down : Direction.Up;
        }
        
        // Start knockback effect
        state.IsKnockedBack = true;
        state.KnockbackTimer = 0f;
    }
} 