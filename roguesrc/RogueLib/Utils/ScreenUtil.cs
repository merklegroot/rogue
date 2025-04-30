using RogueLib.Models;

namespace RogueLib.Utils;

public interface IScreenUtil
{
    Coord2dFloat ToScreenCoord(Coord2dFloat coord, Coord2dFloat cameraPos);
    float ToScreenX(float coordX, float cameraPosX);
    float ToScreenY(float coordY, float cameraPosY);
    
    float ScreenDelX { get; }
    float ScreenDelY { get; }
    float ScreenOffsetX { get; }
    float ScreenOffsetY { get; }
}

public class ScreenUtil : IScreenUtil
{
    public float ScreenDelX => 32.0f;
    public float ScreenDelY => 40.0f;
    public float ScreenOffsetX => 500.0f;
    public float ScreenOffsetY => 300.0f;

    public Coord2dFloat ToScreenCoord(Coord2dFloat coord, Coord2dFloat cameraPos) =>
        new Coord2dFloat(
            ToScreenX(coord.X, cameraPos.X), 
            ToScreenY(coord.Y, cameraPos.Y));
    
    public float ToScreenX(float coordX, float cameraPosX) =>
        ScreenDelX * (coordX - cameraPosX) + ScreenOffsetX;
    

    public float ToScreenY(float coordY, float cameraPosY) =>
        ScreenDelY * (coordY - cameraPosY) + ScreenOffsetY;
}