using Raylib_cs;
using RogueLib.State;
using RogueLib.Constants;

namespace RogueLib.Presenters;

public interface IEnemyPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class EnemyPresenter : IEnemyPresenter
{
    private readonly IDrawUtil _drawUtil;

    public EnemyPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        foreach (var enemy in state.Enemies)
        {
            if (enemy.IsAlive)
            {
                int enemyScreenX = 100 + (int)((enemy.Position.X - state.CameraState.X) * 32) + 400;
                int enemyScreenY = 100 + (int)((enemy.Position.Y - state.CameraState.Y) * 40) + 200;
                
                // Only draw if on screen
                if (enemyScreenX >= 0 && enemyScreenX < ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale &&
                    enemyScreenY >= 0 && enemyScreenY < ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale)
                {
                    _drawUtil.DrawCharacter(rayConnection, ScreenConstants.EnemyChar, enemyScreenX, enemyScreenY, ScreenConstants.EnemyColor);
                }
            }
        }
    }
} 