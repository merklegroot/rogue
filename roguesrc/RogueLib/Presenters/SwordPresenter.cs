using System.Numerics;
using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface ISwordPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
    void Update(GameState state);
}

public class SwordPresenter : ISwordPresenter
{
    private readonly IDrawUtil _drawUtil;

    public SwordPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        if (!state.SwordState.IsSwordSwinging) return;

        // Calculate animation progress (0.0 to 1.0)
        var progress = state.SwordState.SwordSwingTime / GameConstants.SwordSwingDuration;
        if (progress > 1.0f) progress = 1.0f;

        // Calculate rotation based on direction and progress
        float rotation = 0f;
        switch (state.LastDirection)
        {
            case Direction.Left:
                rotation = 180f + (progress - 0.5f) * 180f; // Start at 180°, swing 180°
                break;
            case Direction.Right:
                rotation = 0f + (progress - 0.5f) * 180f; // Start at 0°, swing 180°
                break;
            case Direction.Up:
                rotation = 270f + (progress - 0.5f) * 180f; // Start at 270°, swing 180°
                break;
            case Direction.Down:
                rotation = 90f + (progress - 0.5f) * 180f; // Start at 90°, swing 180°
                break;
        }

        // Calculate position based on direction
        float xOffset = 0f;
        float yOffset = 0f;
        switch (state.LastDirection)
        {
            case Direction.Left:
                xOffset = -0.9f;
                yOffset = (progress - 0.5f) * 1.2f;
                break;
            case Direction.Right:
                xOffset = 0.9f;
                yOffset = (progress - 0.5f) * 1.2f;
                break;
            case Direction.Up:
                yOffset = -1.2f;
                xOffset = (progress - 0.5f) * 1.2f;
                break;
            case Direction.Down:
                yOffset = 1.2f;
                xOffset = (progress - 0.5f) * 1.2f;
                break;
        }

        // Calculate exact pixel position with camera offset
        float swordX = 100 + ((state.PlayerX + xOffset) - state.CameraState.X) * 32 + 400;
        float swordY = 100 + ((state.PlayerY + yOffset) - state.CameraState.Y) * 40 + 200;

        // Get texture dimensions
        float textureWidth = rayConnection.SwordTexture.Width;
        float textureHeight = rayConnection.SwordTexture.Height;

        // Create source and destination rectangles
        Rectangle source = new(0, 0, textureWidth, textureHeight);
        Rectangle dest = new(swordX, swordY, textureWidth, textureHeight);
        Vector2 origin = new(textureWidth / 2, textureHeight / 2);

        // Draw the sword texture with rotation and color tint
        Raylib.DrawTexturePro(
            rayConnection.SwordTexture,
            source,
            dest,
            origin,
            rotation,
            ScreenConstants.SwordColor
        );
    }

    public void Update(GameState state)
    {
        if (state.SwordState.IsSwordSwinging)
        {
            state.SwordState.SwordSwingTime += Raylib.GetFrameTime();
            if (state.SwordState.SwordSwingTime >= GameConstants.SwordSwingDuration)
            {
                state.SwordState.IsSwordSwinging = false;
                state.SwordState.SwordOnCooldown = true;  // Start cooldown when swing finishes
                state.SwordState.SwordCooldownTimer = 0f;
            }
        }
    }
} 