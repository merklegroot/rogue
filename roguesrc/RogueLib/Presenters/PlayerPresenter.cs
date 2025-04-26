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


        DrawInTransitPlayer(rayConnection, state, playerScreenX, playerScreenY, wobbleScale);
    }

    private void DrawInTransitPlayer(IRayConnection rayConnection, GameState state, int playerScreenX, int playerScreenY, float wobbleScale)
    {
        Color playerColor = ScreenConstants.PlayerColor;
        if (state.IsInvincible && (int)(Raylib.GetTime() * 10) % 2 == 0)
        {
            playerColor = ScreenConstants.InvinciblePlayerColor;
        }

        // Draw in-transit ghost (yellow opaque)
        const float moveDuration = 0.2f; // seconds between moves
        float moveProgress = Math.Clamp((float)(Raylib.GetTime() - state.MovementStartTime) / moveDuration, 0f, 1f);

        // Easement
        var effectiveMoveProgress = Math.Min(moveProgress, 1.0f);
        float ghostX = state.PreviousPlayerPosition.X + (state.PlayerPosition.X - state.PreviousPlayerPosition.X) * effectiveMoveProgress;
        float ghostY = state.PreviousPlayerPosition.Y + (state.PlayerPosition.Y - state.PreviousPlayerPosition.Y) * effectiveMoveProgress;
        int ghostScreenX2 = 100 + (int)((ghostX - state.CameraState.X) * 32) + 400;
        int ghostScreenY2 = 100 + (int)((ghostY - state.CameraState.Y) * 40) + 200;
        _drawUtil.DrawCharacter(rayConnection, 1, ghostScreenX2, ghostScreenY2, playerColor, false, wobbleScale);
    }
} 