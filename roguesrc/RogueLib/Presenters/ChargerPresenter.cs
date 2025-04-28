using System.Runtime.CompilerServices;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IChargerPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class ChargerPresenter: IChargerPresenter
{    
    private readonly IDrawUtil _drawUtil;
    
    public ChargerPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }
    
    public void Draw(IRayConnection rayConnection, GameState state)
    {
        if (!state.Charger.IsAlive)
            return;

        // Is it close enough to view?
        if (Math.Abs(state.Charger.X - state.CameraState.X) >= 15 || Math.Abs(state.Charger.Y - state.CameraState.Y) >= 10)
            return;

        
        _drawUtil.DrawCharacter(rayConnection, 
            ScreenConstants.ChargerCharacter, 
            (int)(100 + (state.Charger.X - state.CameraState.X) * 32 + 400), 
            (int)(100 + (state.Charger.Y - state.CameraState.Y) * 40 + 200), 
            ScreenConstants.ChargerColor);
    }
}