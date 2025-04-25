using Raylib_cs;
using RogueLib.State;
using RogueLib.Constants;

namespace RogueLib.Presenters;

public interface IExplosionPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class ExplosionPresenter : IExplosionPresenter
{
    private readonly IDrawUtil _drawUtil;

    public ExplosionPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        foreach (var explosion in state.Explosions)
        {
            char explosionChar = explosion.Frame switch
            {
                0 => '*',      // Small explosion
                1 => (char)15, // Medium explosion (sun symbol in CP437)
                _ => (char)42  // Large explosion (asterisk)
            };
            
            // Calculate position with camera offset - updated horizontal spacing
            int explosionX = 100 + (int)((explosion.Position.X - state.CameraState.X) * 32) + 400;
            int explosionY = 100 + (int)((explosion.Position.Y - state.CameraState.Y) * 40) + 200;
            
            // Only draw if on screen
            if (explosionX >= 0 && explosionX < ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale &&
                explosionY >= 0 && explosionY < ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale)
            {
                _drawUtil.DrawCharacter(rayConnection, explosionChar, explosionX, explosionY, ScreenConstants.ExplosionColor);
            }
        }
    }
} 