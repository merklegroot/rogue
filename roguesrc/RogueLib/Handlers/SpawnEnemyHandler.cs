using RogueLib.State;
using RogueLib.Constants;
using RogueLib.Models;

namespace RogueLib.Handlers;

public interface ISpawnEnemyHandler
{
    void Handle(GameState state);
}

public class SpawnEnemyHandler : ISpawnEnemyHandler
{
    private static readonly Random _random = new Random();

    public void Handle(GameState state)
    {
        // Check total enemy limit first
        if (state.Enemies.Count(e => e.IsAlive) >= GameConstants.MaxEnemies)
            return;

        // Calculate player's chunk coordinates
        int playerChunkX = (int)state.PlayerPosition.X / GameConstants.ChunkSize;
        int playerChunkY = (int)state.PlayerPosition.Y / GameConstants.ChunkSize;

        // Create a list of all valid spawn positions (floor tiles only)
        var validPositions = new List<(int x, int y)>();
        
        // Scan the entire map for floor tiles ('.')
        for (int y = 0; y < state.Map.Count; y++)
        {
            string line = state.Map[y];
            for (int x = 0; x < line.Length; x++)
            {
                if (line[x] == '.')  // Only consider floor tiles
                {
                    // Calculate chunk coordinates for this position
                    int chunkX = x / GameConstants.ChunkSize;
                    int chunkY = y / GameConstants.ChunkSize;

                    // Only consider positions in player's chunk or adjacent chunks
                    if (Math.Abs(chunkX - playerChunkX) <= 1 && Math.Abs(chunkY - playerChunkY) <= 1)
                    {
                        // Check if position is not occupied by player or other enemies
                        if ((x != state.PlayerPosition.X || y != state.PlayerPosition.Y) &&
                            !state.Enemies.Any(e => e.IsAlive && e.Position.X == x && e.Position.Y == y))
                        {
                            // Check per-chunk limit
                            int enemiesInChunk = state.Enemies.Count(e => 
                                e.IsAlive && 
                                e.Position.X / GameConstants.ChunkSize == chunkX && 
                                e.Position.Y / GameConstants.ChunkSize == chunkY);

                            // Allow up to 2 enemies per chunk
                            if (enemiesInChunk < 2)
                            {
                                validPositions.Add((x, y));
                            }
                        }
                    }
                }
            }
        }
        
        // If no valid positions found, don't spawn
        if (validPositions.Count == 0)
            return;
        
        // Pick a random valid position
        var randomIndex = _random.Next(validPositions.Count);
        var (newX, newY) = validPositions[randomIndex];
        
        // Spawn the enemy
        state.Enemies.Add(new Enemy { Position = new Coord2dFloat(newX, newY), IsAlive = true });
    }
}