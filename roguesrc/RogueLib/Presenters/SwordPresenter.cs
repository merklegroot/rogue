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

        // Calculate frame (0, 1, or 2)
        var frame = (int)(progress * 3);
        if (frame > 2) frame = 2;

        // Calculate fractional position
        var xOffset = 0f;
        var yOffset = 0f;

        // Determine position based on direction and animation progress
        switch (state.LastDirection)
        {
            case Direction.Left:
                // Fixed position to the left, sweeping from top to bottom
                xOffset = -0.9f;
                yOffset = (progress - 0.5f) * 1.2f;
                break;
            case Direction.Right:
                // Fixed position to the right, sweeping from top to bottom
                xOffset = 0.9f;
                yOffset = (progress - 0.5f) * 1.2f;
                break;
            case Direction.Up:
                // Fixed position above, sweeping from left to right
                yOffset = -1.2f;
                xOffset = (progress - 0.5f) * 1.2f;
                break;
            case Direction.Down:
                // Fixed position below, sweeping from left to right
                yOffset = 1.2f;
                xOffset = (progress - 0.5f) * 1.2f;
                break;
        }

        // Calculate exact pixel position with camera offset - updated horizontal spacing
        float swordX = 100 + ((state.PlayerX + xOffset) - state.CameraState.X) * 32 + 400;
        float swordY = 100 + ((state.PlayerY + yOffset) - state.CameraState.Y) * 40 + 200;

        // Get sword character based on direction and animation frame
        char swordChar = (state.LastDirection, frame) switch
        {
            // Left side: \ → - → /
            (Direction.Left, 0) => '\\',
            (Direction.Left, 1) => '-',
            (Direction.Left, 2) => '/',

            // Right side: / → - → \
            (Direction.Right, 0) => '/',
            (Direction.Right, 1) => '-',
            (Direction.Right, 2) => '\\',

            // Up: \ → | → /
            (Direction.Up, 0) => '\\',
            (Direction.Up, 1) => '|',
            (Direction.Up, 2) => '/',

            // Down: / → | → \
            (Direction.Down, 0) => '/',
            (Direction.Down, 1) => '|',
            (Direction.Down, 2) => '\\',

            // Fallback
            _ => '+'
        };

        // Draw the sword character with silvery-blue color
        _drawUtil.DrawCharacter(rayConnection, swordChar, (int)swordX, (int)swordY, ScreenConstants.SwordColor);
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