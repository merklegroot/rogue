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
                               || state.Enemies.Any(e => e.IsAlive && (int)e.Position.X == x && (int)e.Position.Y == y);
                if (occupied) continue;
                
                // Check per-chunk limit for enemies and spinners combined
                var mobsInChunk = state.Enemies.Count(e => 
                                      e.IsAlive && 
                                      (int)e.Position.X / GameConstants.ChunkSize == chunkX && 
                                      (int)e.Position.Y / GameConstants.ChunkSize == chunkY);

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
        var enemyType = GetEnemyTypeToSpawn();
        if(enemyType == EnemyEnum.Spinner)
        {
            var angle = (float)(Random.NextDouble() * 2 * Math.PI);

        var spinner = new EnemyState<SpinnerEnemyContext>
        {
            Position = new Coord2dFloat(newX + 0.5f, newY + 0.5f),
            IsAlive = true,
            EnemyType = EnemyEnum.Spinner,
            Context = new SpinnerEnemyContext { DirectionAngle = angle, SpinAngle = 0f }
        };

            state.Enemies.Add(spinner);
            return;
        }

        
        state.Enemies.Add(new EnemyState { Position = new Coord2dFloat(newX, newY), IsAlive = true, EnemyType = enemyType });        
    }

    private EnemyEnum GetEnemyTypeToSpawn()
    {
        var randomValue = Random.NextDouble();

        return randomValue switch {
            < 0.2 => EnemyEnum.Minotaur,
            < 0.4 => EnemyEnum.Spinner,
            _ => EnemyEnum.Cedilla
        };
    }
}