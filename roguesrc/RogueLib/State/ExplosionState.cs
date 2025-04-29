using RogueLib.Constants;
using RogueLib.Models;

namespace RogueLib;

public class ExplosionState
{
    public Coord2dFloat Position { get; set; }
    public float Timer { get; set; }
    public int Frame => (int)((Timer / GameConstants.ExplosionDuration) * 3);  // 3 frames of animation
}