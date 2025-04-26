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
    private readonly ISpawnEnemyHandler _spawnEnemyHandler;
    private readonly IMapUtil _mapUtil;

    public UpdateEnemiesHandler(ISpawnEnemyHandler spawnEnemyHandler, IMapUtil mapUtil)
    {
        _spawnEnemyHandler = spawnEnemyHandler;
        _mapUtil = mapUtil;
    }

    public void Handle(GameState state)
    {
        if (!state.IsEnemyMovementEnabled)
            return;

        float frameTime = Raylib.GetFrameTime();
        float moveAmount = GameConstants.EnemyMoveSpeed * frameTime;

        foreach (var enemy in state.Enemies)
        {
            if (enemy.IsAlive)
            {
                // Update movement timer
                enemy.MoveTimer += frameTime;

                // If not currently moving and timer expired, choose new direction
                if (!enemy.IsMoving && enemy.MoveTimer >= GameConstants.EnemyMoveDelay)
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
                    bool reachedTarget = Math.Abs(newX - enemy.TargetPosition.X) < 0.1f && 
                                       Math.Abs(newY - enemy.TargetPosition.Y) < 0.1f;
                    
                    bool hitObstacle = !_mapUtil.IsWalkableTile(state.Map, (int)Math.Floor(newX), (int)Math.Floor(newY));

                    if (reachedTarget || hitObstacle)
                    {
                        // Stop movement
                        enemy.IsMoving = false;
                        if (reachedTarget)
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

                // Check for collision with player
                if (Math.Abs(enemy.Position.X - state.PlayerPosition.X) < 0.5f && 
                    Math.Abs(enemy.Position.Y - state.PlayerPosition.Y) < 0.5f)
                {
                    // Only damage player if not invincible
                    if (!state.IsInvincible)
                    {
                        state.CurrentHealth--;
                        Console.WriteLine($"Player hit by enemy! Health: {state.CurrentHealth}");
                        
                        // Apply knockback
                        ApplyKnockback(state, new Vector2(enemy.Position.X, enemy.Position.Y));
                    }
                }
            }
        }
    }

    private void ChooseNewDirection(GameState state, Enemy enemy)
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