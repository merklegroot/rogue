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
        int playerScreenX = 100 + (int)((state.PlayerX - state.CameraState.X) * 32) + 400;
        int playerScreenY = 100 + (int)((state.PlayerY - state.CameraState.Y) * 40) + 200;

        // Calculate wobble effect
        state.WobbleTimer += Raylib.GetFrameTime();
        float phase = (float)(state.WobbleTimer * (2 * Math.PI / (GameConstants.WobbleFrequency / 1000.0f)));
        float modifiedSine = (float)(Math.Sin(phase) > 0 ? 2 * Math.Sin(phase) : Math.Sin(phase));
        float wobbleScale = 1.0f + (float)(modifiedSine / 1.5f) * GameConstants.WobbleAmount;


        DrawOldPositionGhost(rayConnection, state, playerScreenX, playerScreenY, wobbleScale);
        DrawInTransitPlayer(rayConnection, state, playerScreenX, playerScreenY, wobbleScale);
        DrawNewPositionGhost(rayConnection, state, playerScreenX, playerScreenY, wobbleScale);
    }

    private void DrawNewPositionGhost(IRayConnection rayConnection, GameState state, int playerScreenX, int playerScreenY, float wobbleScale)
    {
        // Draw ghost at new position
        int ghostScreenX3 = 100 + (int)((state.PlayerX - state.CameraState.X) * 32) + 400;
        int ghostScreenY3 = 100 + (int)((state.PlayerY - state.CameraState.Y) * 40) + 200;
        _drawUtil.DrawCharacter(rayConnection, 1, ghostScreenX3, ghostScreenY3, ScreenConstants.NewPositionGhostColor, false, wobbleScale);
    }

    private void DrawOldPositionGhost(IRayConnection rayConnection, GameState state, int playerScreenX, int playerScreenY, float wobbleScale)
    {
        // Draw ghost at previous position (gray translucent)
        int ghostScreenX = 100 + (int)((state.PreviousX - state.CameraState.X) * 32) + 400;
        int ghostScreenY = 100 + (int)((state.PreviousY - state.CameraState.Y) * 40) + 200;
        _drawUtil.DrawCharacter(rayConnection, 1, ghostScreenX, ghostScreenY, ScreenConstants.OldPositionGhostColor, false, wobbleScale);
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
        float ghostX = state.PreviousX + (state.PlayerX - state.PreviousX) * effectiveMoveProgress;
        float ghostY = state.PreviousY + (state.PlayerY - state.PreviousY) * effectiveMoveProgress;
        int ghostScreenX2 = 100 + (int)((ghostX - state.CameraState.X) * 32) + 400;
        int ghostScreenY2 = 100 + (int)((ghostY - state.CameraState.Y) * 40) + 200;
        _drawUtil.DrawCharacter(rayConnection, 1, ghostScreenX2, ghostScreenY2, playerColor, false, wobbleScale);
    }
} 