using System.Numerics;
using Raylib_cs;
using RogueLib.State;
using RogueLib.Constants;

namespace RogueLib.Handlers;

public interface IUpdateEnemiesHandler
{
    void Handle(GameState state);
}

public class UpdateEnemiesHandler : IUpdateEnemiesHandler
{
    private readonly Random _random = new();
    private readonly ISpawnEnemyHandler _spawnEnemyHandler;

    public UpdateEnemiesHandler(ISpawnEnemyHandler spawnEnemyHandler)
    {
        _spawnEnemyHandler = spawnEnemyHandler;
    }

    public void Handle(GameState state)
    {
        float frameTime = Raylib.GetFrameTime();
        
        // Update enemy spawn timer
        state.EnemySpawnTimer += frameTime;
        if (state.EnemySpawnTimer >= GameConstants.EnemySpawnDelay)
        {
            state.EnemySpawnTimer = 0f;
            
            // Only spawn if we haven't reached the maximum
            if (state.Enemies.Count(e => e.IsAlive) < GameConstants.MaxEnemies)
            {
                _spawnEnemyHandler.Handle(state);
            }
        }
        
        // Update existing enemies
        foreach (var enemy in state.Enemies)
        {
            if (!enemy.IsAlive) continue;
            
            enemy.MoveTimer += frameTime;
            if (enemy.MoveTimer >= GameConstants.EnemyMoveDelay)
            {
                enemy.MoveTimer = 0f;
                
                // Generate random direction (-1, 0, or 1 for both x and y)
                float dx = _random.Next(-1, 2) * GameConstants.PlayerMoveSpeed;
                float dy = _random.Next(-1, 2) * GameConstants.PlayerMoveSpeed;
                
                // Try to move horizontally first
                if (dx != 0)
                {
                    // Check if the new position is walkable
                    if (IsWalkableTile(state.Map, (int)Math.Floor((double)enemy.Position.X + (double)dx), (int)Math.Floor((double)enemy.Position.Y)))
                    {
                        enemy.Position.X = (int)(enemy.Position.X + dx);
                    }
                    // If horizontal movement is blocked, try vertical
                    else if (dy != 0 && IsWalkableTile(state.Map, (int)Math.Floor((double)enemy.Position.X), (int)Math.Floor((double)enemy.Position.Y + (double)dy)))
                    {
                        enemy.Position.Y = (int)(enemy.Position.Y + dy);
                    }
                    // If both are blocked, try diagonal
                    else if (IsWalkableTile(state.Map, (int)Math.Floor((double)enemy.Position.X + (double)dx), (int)Math.Floor((double)enemy.Position.Y + (double)dy)))
                    {
                        enemy.Position.X = (int)(enemy.Position.X + dx);
                        enemy.Position.Y = (int)(enemy.Position.Y + dy);
                    }
                }
                // If no horizontal movement, try vertical
                else if (dy != 0)
                {
                    // Check if the new position is walkable
                    if (IsWalkableTile(state.Map, (int)Math.Floor((double)enemy.Position.X), (int)Math.Floor((double)enemy.Position.Y + (double)dy)))
                    {
                        enemy.Position.Y = (int)(enemy.Position.Y + dy);
                    }
                }
                
                // Check for collision with player
                if (Math.Abs(enemy.Position.X - state.PlayerPosition.X) < 0.5f && Math.Abs(enemy.Position.Y - state.PlayerPosition.Y) < 0.5f)
                {
                    // Only damage player if not invincible
                    if (!state.IsInvincible)
                    {
                        state.CurrentHealth--;
                        Console.WriteLine($"Player hit by enemy! Health: {state.CurrentHealth}");
                        
                        // Apply knockback
                        ApplyKnockback(state, new Vector2(enemy.Position.X, enemy.Position.Y));
                        
                        // Make player briefly invincible
                        state.IsInvincible = true;
                        state.InvincibilityTimer = 0f;
                    }
                }
            }
        }
        
        // Remove dead enemies
        state.Enemies.RemoveAll(e => !e.IsAlive);
    }

    private void ApplyKnockback(GameState state, Vector2 sourcePosition, float multiplier = 1.0f)
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

    private bool IsWalkableTile(List<string> map, int x, int y)
    {
        // Check if position is within map bounds
        if (y < 0 || y >= map.Count)
            return false; // Out of bounds vertically
        
        // Check if x is within the bounds of the current line
        if (x < 0 || x >= map[y].Length)
            return false; // Out of bounds horizontally
        
        // Check if the tile is a wall or other non-walkable object
        char mapChar = map[y][x];

        var walkableTiles = new List<char> { '.', 'X', '╬' };

        return walkableTiles.Contains(mapChar);
    }
} 