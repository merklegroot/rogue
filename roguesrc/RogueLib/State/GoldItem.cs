using RogueLib.Models;

namespace RogueLib.State;

public class GoldItem
{
    public Coord2dFloat Position { get; set; }
    public int Value { get; set; }  // How much this gold is worth
}