using RogueLib.State;
using RogueLib.Models;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface IEnemyPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class EnemyPresenter : IEnemyPresenter
{
    private readonly IDrawUtil _drawUtil;
    private readonly IDrawEnemyUtil _drawEnemyUtil;
    private readonly IScreenUtil _screenUtil;

    public EnemyPresenter(IDrawUtil drawUtil, IDrawEnemyUtil drawEnemyUtil, IScreenUtil screenUtil)
    {
        _drawUtil = drawUtil;
        _drawEnemyUtil = drawEnemyUtil;
        _screenUtil = screenUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        foreach (var enemy in state.Enemies)
        {
            if (!enemy.IsAlive)
                continue;
            
            var screenPos = _screenUtil.ToScreenCoord(enemy.Position, state.CameraState);
            _drawEnemyUtil.Draw(rayConnection, enemy.EnemyType, screenPos);
        }
        
        foreach (var spinner in state.Spinners)
        {
            if (!spinner.IsAlive)
                continue;
            
            var spinnerScreenPos = _screenUtil.ToScreenCoord(spinner.Position, state.CameraState);
            _drawEnemyUtil.DrawSpinner(rayConnection, spinnerScreenPos, spinner.SpinAngle);
        }
    }
} 