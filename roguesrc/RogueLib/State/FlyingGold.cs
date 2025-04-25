using RogueLib.Models;

namespace RogueLib.State;

public class FlyingGold
{
    public Coord2dFloat StartPosition { get; set; }
    public int Value { get; set; }   // How much this gold is worth
    public float Timer { get; set; } // Animation timer
}