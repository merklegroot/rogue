using System;
using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface IGoldPickupPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class GoldPickupPresenter : IGoldPickupPresenter
{
    private readonly IDrawUtil _drawUtil;

    public GoldPickupPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Draw gold items - with updated horizontal spacing
        foreach (var gold in state.GoldItems)
        {
            if (Math.Abs(gold.Position.X - state.CameraState.X) < 15 && Math.Abs(gold.Position.Y - state.CameraState.Y) < 10)
            {
                _drawUtil.DrawCharacter(rayConnection, 36, 100 + (int)((gold.Position.X - state.CameraState.X) * 32) + 400, 100 + (int)((gold.Position.Y - state.CameraState.Y) * 40) + 200, ScreenConstants.GoldColor); // $ symbol
            }
        }
    }
} 