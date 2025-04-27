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

        // Calculate if player is moving based on velocity
        bool isMoving = Math.Abs(state.VelocityX) > 0.1f || Math.Abs(state.VelocityY) > 0.1f;

        // Apply shear transformation for left/right movement only when moving
        if (isMoving && (state.ActionDirection == Direction.Left || state.ActionDirection == Direction.Right))
        {
            // Calculate shear amount (reversed direction)
            float shearAmount = state.ActionDirection == Direction.Left ? 0.3f : -0.3f;
            
            // Draw the border texture with shear
            for (int y = 0; y < scaledHeight; y++)
            {
                float offset = shearAmount * y;
                // Map the current y position to the texture's height
                int sourceY = (int)((float)y / scaledHeight * rayConnection.SmileyBorderTexture.Height);
                Rectangle source = new(0, sourceY, rayConnection.SmileyBorderTexture.Width, 1);
                Rectangle dest = new(playerScreenX + offset, adjustedY + y, scaledWidth, 1);
                
                // Calculate the destination width to maintain aspect ratio
                float aspectRatio = (float)rayConnection.SmileyBorderTexture.Width / rayConnection.SmileyBorderTexture.Height;
                dest.Width = scaledHeight * aspectRatio;
                
                Raylib.DrawTexturePro(
                    rayConnection.SmileyBorderTexture,
                    source,
                    dest,
                    Vector2.Zero,
                    0,
                    currentPlayerColor
                );
            }

            // Draw the neutral texture with shear
            for (int y = 0; y < scaledHeight; y++)
            {
                float offset = shearAmount * y;
                // Map the current y position to the texture's height
                int sourceY = (int)((float)y / scaledHeight * rayConnection.SmileyNeutralTexture.Height);
                Rectangle source = new(0, sourceY, rayConnection.SmileyNeutralTexture.Width, 1);
                Rectangle dest = new(playerScreenX + offset, adjustedY + y, scaledWidth, 1);
                
                // Calculate the destination width to maintain aspect ratio
                float aspectRatio = (float)rayConnection.SmileyNeutralTexture.Width / rayConnection.SmileyNeutralTexture.Height;
                dest.Width = scaledHeight * aspectRatio;
                
                Raylib.DrawTexturePro(
                    rayConnection.SmileyNeutralTexture,
                    source,
                    dest,
                    Vector2.Zero,
                    0,
                    currentPlayerColor
                );
            }
        }
        else
        {
            // Draw without shear for up/down movement or when not moving
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

            // Draw the neutral texture
            source = new(0, 0, rayConnection.SmileyNeutralTexture.Width, rayConnection.SmileyNeutralTexture.Height);
            dest = new(playerScreenX, adjustedY, scaledWidth, scaledHeight);
            Raylib.DrawTexturePro(
                rayConnection.SmileyNeutralTexture,
                source,
                dest,
                Vector2.Zero,
                0,
                currentPlayerColor
            );
        }
    }

    private Color GetCurrentPlayerColor(GameState state)
    {
        if (state.IsInvincible && (int)(Raylib.GetTime() * 10) % 2 == 0)
            return ScreenConstants.InvinciblePlayerColor;

        return ScreenConstants.PlayerColor;
    }
}