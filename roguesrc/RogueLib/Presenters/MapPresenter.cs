using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IMapPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class MapPresenter : IMapPresenter
{
    private readonly IDrawUtil _drawUtil;

    public MapPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Calculate map dimensions
        int mapHeight = state.Map.Count;
        int mapWidth = state.Map.Max(line => line.Length);

        // Draw the map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Skip tiles that are too far from the camera (optimization)
                if (Math.Abs(x - state.CameraState.X) > 25 || Math.Abs(y - state.CameraState.Y) > 20)
                    continue;
                
                // Get the character at this position in the map
                char mapChar = ' '; // Default to empty space (not a dot)
                if (y < state.Map.Count && x < state.Map[y].Length)
                {
                    mapChar = state.Map[y][x];
                }
                
                // Calculate screen position with camera offset
                // Using 32 pixels for horizontal spacing
                int screenX = 100 + (int)((x - state.CameraState.X) * 32) + 400;
                int screenY = 100 + (int)((y - state.CameraState.Y) * 40) + 200;
                
                // Draw the appropriate tile based on the map character
                Color tileColor = Color.DarkGray;
                int tileChar = 0; // Default to space (empty) instead of 250 (dot)
                
                var wallColor = Color.Brown;

                switch (mapChar)
                {
                    case '╔': // Top left corner
                        tileChar = 0xC9; // ╔
                        tileColor = wallColor;
                        break;
                    case '╗': // Top right corner
                        tileChar = 0xBB; // ╗
                        tileColor = wallColor;
                        break;
                    case '╚': // Bottom left corner
                        tileChar = 0xC8; // ╚
                        tileColor = wallColor;
                        break;
                    case '╝': // Bottom right corner
                        tileChar = 0xBC; // ╝
                        tileColor = wallColor;
                        break;
                    case '║': // Vertical wall
                    case '|': // Vertical wall
                        tileChar = 0xBA; // ║
                        tileColor = wallColor;
                        break;
                    case '-': // Horizontal wall
                    case '═': // Horizontal wall
                        tileChar = 0xCD; // ═
                        tileColor = wallColor;
                        break;
                    case '╬': // door
                    case '+': // door
                        tileChar = 0xCE; // ╬
                        tileColor = Color.Brown;
                        break;
                    case '.': // Floor
                        tileChar = 250; // ·
                        tileColor = Color.Green;
                        break;
                    case 'X': // Hallway
                        tileChar = 0xB1; // partially filled square
                        tileColor = Color.Gray;
                        break;
                    default:
                        tileChar = 0; // Empty space
                        break;
                }
                
                // Draw the tile
                _drawUtil.DrawCharacter(rayConnection, tileChar, screenX, screenY, tileColor);
            }
        }
    }
} 