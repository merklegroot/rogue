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

            if (enemy.EnemyType == EnemyEnum.Spinner)
            {
                // TODO: Why does its screen position work differently than the other enemies?
                // TODO: This isn't a good thing.
                var spinnerScreenPos = _screenUtil.ToScreenCoord(enemy.Position, state.CameraState);
                var spinnerContext = (enemy as EnemyState<SpinnerEnemyContext>)!.Context;
                _drawEnemyUtil.Draw(rayConnection, enemy.EnemyType, spinnerScreenPos, spinnerContext);
                continue;
            }

            _drawEnemyUtil.Draw(rayConnection, enemy.EnemyType, screenPos, null);
        }
    }
} 