using RogueLib.State;
using RogueLib.Constants;

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
        int playerChunkX = (int)state.PlayerX / GameConstants.ChunkSize;
        int playerChunkY = (int)state.PlayerY / GameConstants.ChunkSize;

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
                        if ((x != state.PlayerX || y != state.PlayerY) &&
                            !state.Enemies.Any(e => e.IsAlive && e.X == x && e.Y == y))
                        {
                            // Check per-chunk limit
                            int enemiesInChunk = state.Enemies.Count(e => 
                                e.IsAlive && 
                                e.X / GameConstants.ChunkSize == chunkX && 
                                e.Y / GameConstants.ChunkSize == chunkY);

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
        state.Enemies.Add(new Enemy { X = newX, Y = newY, IsAlive = true });
    }

}