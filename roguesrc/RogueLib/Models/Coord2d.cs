namespace RogueLib.Models;

public interface ICoord2d<TNumber>
{
    TNumber X { get; set; }
    TNumber Y { get; set; }
}

public record struct Coord2d<TNumber> : ICoord2d<TNumber>
{
    public TNumber X { get; set; }
    public TNumber Y { get; set; }

    public Coord2d(TNumber x, TNumber y)
    {
        X = x;
        Y = y;
    }
}

public record struct Coord2dInt : ICoord2d<int>
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coord2dInt(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public record struct Coord2dFloat : ICoord2d<float>
{
    public float X { get; set; }
    public float Y { get; set; }

    public Coord2dFloat(float x, float y)
    {
        X = x;
        Y = y;
    }
}
