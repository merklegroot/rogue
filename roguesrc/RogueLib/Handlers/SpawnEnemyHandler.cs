using RogueLib.State;
using RogueLib.Constants;
using RogueLib.Models;
using Raylib_cs;

namespace RogueLib.Handlers;

public interface ISpawnEnemyHandler
{
    void Handle(GameState state);
}

public class SpawnEnemyHandler : ISpawnEnemyHandler
{
    private static readonly Random Random = new();

    public void Handle(GameState state)
    {
        if (!state.IsEnemySpawnEnabled)
            return;
            
        // Update spawn timer
        state.EnemySpawnTimer += Raylib.GetFrameTime();

        // Only spawn if timer has reached the delay
        if (state.EnemySpawnTimer < EnemyConstants.EnemySpawnDelay)
            return;

        // Reset timer
        state.EnemySpawnTimer = 0f;

        // Check total enemy limit first
        if (state.Enemies.Count(e => e.IsAlive) >= EnemyConstants.MaxEnemies)
            return;
        
        // Check total spinner limit (same as enemies for now)
        if (state.Spinners.Count(s => s.IsAlive) >= EnemyConstants.MaxEnemies)
            return;

        // Calculate player's chunk coordinates
        var playerChunkX = (int)state.PlayerPosition.X / GameConstants.ChunkSize;
        var playerChunkY = (int)state.PlayerPosition.Y / GameConstants.ChunkSize;

        // Create a list of all valid spawn positions (floor tiles only)
        var validPositions = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (var y = 0; y < state.Map.Count; y++)
        {
            var line = state.Map[y];
            for (var x = 0; x < line.Length; x++)
            {
                if (line[x] != '.') continue; // Only consider floor tiles
                
                // Calculate chunk coordinates for this position
                var chunkX = x / GameConstants.ChunkSize;
                var chunkY = y / GameConstants.ChunkSize;

                // Only consider positions in player's chunk or adjacent chunks
                if (Math.Abs(chunkX - playerChunkX) > 1 || Math.Abs(chunkY - playerChunkY) > 1) continue;
                // Check if position is not occupied by player, enemies, or spinners
                var occupied = (x == (int)state.PlayerPosition.X && y == (int)state.PlayerPosition.Y)
                               || state.Enemies.Any(e => e.IsAlive && (int)e.Position.X == x && (int)e.Position.Y == y)
                               || state.Spinners.Any(s => s.IsAlive && (int)s.X == x && (int)s.Y == y);
                if (occupied) continue;
                
                // Check per-chunk limit for enemies and spinners combined
                var mobsInChunk = state.Enemies.Count(e => 
                                      e.IsAlive && 
                                      (int)e.Position.X / GameConstants.ChunkSize == chunkX && 
                                      (int)e.Position.Y / GameConstants.ChunkSize == chunkY)
                                  + state.Spinners.Count(s => 
                                      s.IsAlive && 
                                      (int)s.X / GameConstants.ChunkSize == chunkX && 
                                      (int)s.Y / GameConstants.ChunkSize == chunkY);

                // Allow up to 2 mobs per chunk
                if (mobsInChunk < 2)
                {
                    validPositions.Add((x, y));
                }
            }
        }
        
        // If no valid positions found, don't spawn
        if (validPositions.Count == 0)
            return;
        
        // Pick a random valid position
        var randomIndex = Random.Next(validPositions.Count);
        var (newX, newY) = validPositions[randomIndex];
        
        // 20% chance to spawn a minotaur, otherwise regular enemy
        var shouldSpawnMinotaur = Random.NextDouble() < 0.2;
        var enemyType = shouldSpawnMinotaur ? EnemyEnum.Minotaur : EnemyEnum.Cedilla;
        state.Enemies.Add(new Enemy { Position = new Coord2dFloat(newX, newY), IsAlive = true, EnemyType = enemyType });
        
        // Spawn a spinner at the same position, with a random direction
        var angle = (float)(Random.NextDouble() * 2 * Math.PI);
        state.Spinners.Add(new SpinnerEnemyState { X = newX + 0.5f, Y = newY + 0.5f, DirectionAngle = angle, SpinAngle = 0f, IsAlive = true });
    }
}