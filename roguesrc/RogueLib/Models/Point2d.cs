namespace RogueLib.Models;

public record Coord2d<TNumber>
{
    public TNumber X { get; set; }
    public TNumber Y { get; set; }

    public Coord2d(TNumber x, TNumber y)
    {
        X = x;
        Y = y;
    }
}

public record Coord2dInt : Coord2d<int>
{
    public Coord2dInt(int x, int y) : base(x, y)
    {
    }
}

public record Coord2dFloat : Coord2d<float>
{
    public Coord2dFloat(float x, float y) : base(x, y)
    {
    }
}
