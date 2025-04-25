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

        // Calculate rotation based on swing direction and progress
        float rotation = 0f;
        switch (state.SwordState.SwingDirection)
        {
            case Direction.Left:
                rotation = 270f - (progress - 0.5f) * 180f; // Start at 270°, swing 180°
                break;
            case Direction.Right:
                rotation = 90f + (progress - 0.5f) * 180f; // Start at 90°, swing 180°
                break;
            case Direction.Up:
                rotation = 0f + (progress - 0.5f) * 180f; // Start at 0°, swing 180°
                break;
            case Direction.Down:
                rotation = 180f - (progress - 0.5f) * 180f; // Start at 180°, swing 180°
                break;
        }

        // Calculate base position with camera offset
        float baseX = 100 + (state.PlayerX - state.CameraState.X) * 32 + 400;
        float baseY = 100 + (state.PlayerY - state.CameraState.Y) * 40 + 200;

        // Scale the sword to match player character size
        float scaledWidth = ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        float scaledHeight = ScreenConstants.CharHeight * ScreenConstants.DisplayScale;

        // Adjust position based on direction to center the hilt
        float swordX = baseX;
        float swordY = baseY;

        if(state.SwordState.SwingDirection == Direction.Left || state.SwordState.SwingDirection == Direction.Right)
        {
            swordY += scaledHeight / 2 - ScreenConstants.CharHeight / 2;
        }

        if (state.SwordState.SwingDirection == Direction.Right)
        {
            swordX += scaledWidth;
        }

        if (state.SwordState.SwingDirection == Direction.Up || state.SwordState.SwingDirection == Direction.Down)
        {
            swordX += scaledWidth / 2 - ScreenConstants.CharWidth / 2;
        }

        if (state.SwordState.SwingDirection == Direction.Down)
        {
            swordY += scaledHeight - ScreenConstants.CharHeight;
        }

        // Create source and destination rectangles
        Rectangle source = new(0, 0, rayConnection.SwordTexture.Width, rayConnection.SwordTexture.Height);
        Rectangle dest = new(swordX, swordY, scaledWidth, scaledHeight);
        Vector2 origin = new(scaledWidth / 2, scaledHeight); // Rotate around the hilt (bottom center)

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