using RogueLib.Models;

namespace RogueLib.Utils;

public interface IScreenUtil
{
    public Coord2dFloat ToScreenCoord(Coord2dFloat coord, Coord2dFloat cameraPos);
}

public class ScreenUtil : IScreenUtil
{
    public Coord2dFloat ToScreenCoord(Coord2dFloat coord, Coord2dFloat cameraPos)
    {
        var screenX = 32.0f * (coord.X - cameraPos.X) + 500.0f;
        var screenY = 40.0f * (coord.Y - cameraPos.Y) + 300.0f;
        
        return new Coord2dFloat(screenX, screenY);
    }
}
