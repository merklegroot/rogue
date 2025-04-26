using RogueLib.Constants;

namespace RogueLib.Utils;

public interface IMapUtil
{
    bool IsWalkableTile(List<string> map, int x, int y);
}

public class MapUtil : IMapUtil
{
    public bool IsWalkableTile(List<string> map, int x, int y)
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