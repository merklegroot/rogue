using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IPlayerPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class PlayerPresenter : IPlayerPresenter
{
    private readonly IDrawUtil _drawUtil;

    public PlayerPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Calculate screen positions
        int playerScreenX = 100 + (int)((state.PlayerPosition.X - state.CameraState.X) * 32) + 400;
        int playerScreenY = 100 + (int)((state.PlayerPosition.Y - state.CameraState.Y) * 40) + 200;

        // Calculate wobble effect
        state.WobbleTimer += Raylib.GetFrameTime();
        float phase = (float)(state.WobbleTimer * (2 * Math.PI / (GameConstants.WobbleFrequency / 1000.0f)));
        float modifiedSine = (float)(Math.Sin(phase) > 0 ? 2 * Math.Sin(phase) : Math.Sin(phase));
        float wobbleScale = 1.0f + (float)(modifiedSine / 1.5f) * GameConstants.WobbleAmount;

        var currentPlayerColor = GetCurrentPlayerColor(state);

        _drawUtil.DrawCharacter(rayConnection, 1, playerScreenX, playerScreenY, currentPlayerColor, false, wobbleScale);
    }

    private Color GetCurrentPlayerColor(GameState state)
    {
        if (state.IsInvincible && (int)(Raylib.GetTime() * 10) % 2 == 0)
            return ScreenConstants.InvinciblePlayerColor;

        return ScreenConstants.PlayerColor;
    }
}