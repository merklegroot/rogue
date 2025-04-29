using RogueLib.Models;
using RogueLib.State;

namespace RogueLib.Utils;

public interface IScreenUtil
{
    public Coord2dInt ToScreenCoord(Coord2dFloat coord, Coord2dInt cameraPos);
    public Coord2dInt ToScreenCoord(Coord2dInt coord, Coord2dInt cameraPos);
    public Coord2dInt ToScreenCoord(Coord2dInt coord, GameState state);
    public Coord2dInt ToScreenCoord(Coord2dFloat coord, GameState state);
}

public class ScreenUtil : IScreenUtil
{
    public Coord2dInt ToScreenCoord(Coord2dInt coord, Coord2dInt cameraPos)
    {
        var screenX = 32 * (coord.X - cameraPos.X) + 500;
        var screenY = 40 * (coord.Y - cameraPos.Y) + 300;
        
        return new Coord2dInt(screenX, screenY);
    }

    public Coord2dInt ToScreenCoord(Coord2dInt coord, GameState state) =>
        ToScreenCoord(
            coord, 
            new Coord2dInt((int)state.CameraState.X, (int)state.CameraState.Y));
    
    public Coord2dInt ToScreenCoord(Coord2dFloat coord, Coord2dInt cameraPos) =>
        ToScreenCoord(new Coord2dInt((int)coord.X, (int)coord.Y), cameraPos);

    public Coord2dInt ToScreenCoord(Coord2dFloat coord, GameState state) =>
        ToScreenCoord(
            coord, 
            new Coord2dInt((int)state.CameraState.X, (int)state.CameraState.Y));
}