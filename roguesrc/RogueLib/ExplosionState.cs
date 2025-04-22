using RogueLib.Constants;

namespace RogueLib;

public class ExplosionState
{
    public int X { get; set; }
    public int Y { get; set; }
    public float Timer { get; set; }
    public int Frame => (int)((Timer / GameConstants.ExplosionDuration) * 3);  // 3 frames of animation
}