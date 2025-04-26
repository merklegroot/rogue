using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using System.Numerics;

namespace RogueLib.Presenters;

public interface IPlayerPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class PlayerPresenter : IPlayerPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const float FaceMovementAmount = 4.0f;  // Amount to move the face texture in pixels

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

        // Draw the player using SmileyBorderTexture and SmileyNeutralTexture
        float scaledWidth = ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        float scaledHeight = ScreenConstants.CharHeight * ScreenConstants.DisplayScale * wobbleScale;
        
        // Calculate the difference in height to keep bottom anchored
        float heightDiff = scaledHeight - (ScreenConstants.CharHeight * ScreenConstants.DisplayScale);
        
        // Adjust Y position to keep bottom anchored
        float adjustedY = playerScreenY - heightDiff;

        // Draw the border texture
        Rectangle source = new(0, 0, rayConnection.SmileyBorderTexture.Width, rayConnection.SmileyBorderTexture.Height);
        Rectangle dest = new(playerScreenX, adjustedY, scaledWidth, scaledHeight);

        Raylib.DrawTexturePro(
            rayConnection.SmileyBorderTexture,
            source,
            dest,
            Vector2.Zero,
            0,
            currentPlayerColor
        );

        // Calculate offset for neutral texture based on ActionDirection
        float offsetX = 0;
        float offsetY = 0;
        switch (state.ActionDirection)
        {
            case Direction.Left:
                offsetX = -FaceMovementAmount;
                break;
            case Direction.Right:
                offsetX = FaceMovementAmount;
                break;
            case Direction.Up:
                offsetY = -FaceMovementAmount;
                break;
            case Direction.Down:
                offsetY = FaceMovementAmount;
                break;
        }

        // Draw the neutral texture on top with offset
        source = new(0, 0, rayConnection.SmileyNeutralTexture.Width, rayConnection.SmileyNeutralTexture.Height);
        dest = new(playerScreenX + offsetX, adjustedY + offsetY, scaledWidth, scaledHeight);
        Raylib.DrawTexturePro(
            rayConnection.SmileyNeutralTexture,
            source,
            dest,
            Vector2.Zero,
            0,
            currentPlayerColor
        );
    }

    private Color GetCurrentPlayerColor(GameState state)
    {
        if (state.IsInvincible && (int)(Raylib.GetTime() * 10) % 2 == 0)
            return ScreenConstants.InvinciblePlayerColor;

        return ScreenConstants.PlayerColor;
    }
}