using System.Runtime.CompilerServices;
using RogueLib.Constants;
using RogueLib.Models;
using RogueLib.State;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface IChargerPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class ChargerPresenter: IChargerPresenter
{    
    private readonly IDrawUtil _drawUtil;
    private readonly IDrawEnemyUtil _drawEnemyUtil;
    
    public ChargerPresenter(IDrawUtil drawUtil, IDrawEnemyUtil drawEnemyUtil)
    {
        _drawUtil = drawUtil;
        _drawEnemyUtil = drawEnemyUtil;
    }
    
    public void Draw(IRayConnection rayConnection, GameState state)
    {
        if (!state.Charger.IsAlive)
            return;

        DrawChargerCharacter(rayConnection, state);
        DrawChargerHealth(rayConnection, state);
    }

    private void DrawChargerCharacter(IRayConnection rayConnection, GameState state)
    {
        // Is it close enough to view?
        if (Math.Abs(state.Charger.X - state.CameraState.X) >= 15 || Math.Abs(state.Charger.Y - state.CameraState.Y) >= 10)
            return;

        var screenPos = new Coord2dFloat(
            100 + (state.Charger.X - state.CameraState.X) * 32 + 400,
            100 + (state.Charger.Y - state.CameraState.Y) * 40 + 200);
        
        _drawEnemyUtil.Draw(rayConnection, EnemyEnum.Charger, screenPos, null);
    }

    private void DrawChargerHealth(IRayConnection rayConnection, GameState state)
    {
        var healthText = $"Charger HP: {state.Charger.Health}/{ChargerConstants.ChargerHealth} (Hit {state.Charger.HitCount} times)";
        _drawUtil.DrawText(rayConnection, healthText, 20, 60, ScreenConstants.ChargerColor);
    }
}