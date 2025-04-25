using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface ICooldownIndicatorPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class CooldownIndicatorPresenter : ICooldownIndicatorPresenter
{
    private readonly IDrawUtil _drawUtil;

    public CooldownIndicatorPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Draw sword cooldown indicator
        if (state.SwordState.SwordOnCooldown)
        {
            // Calculate cooldown progress (0.0 to 1.0)
            float progress = state.SwordState.SwordCooldownTimer / state.SwordState.SwordCooldown;
            
            // Draw a small cooldown bar above the player
            int barWidth = 30;
            int barHeight = 5;
            
            // Calculate position with camera offset - updated horizontal spacing
            int barX = 100 + (int)((state.PlayerPosition.X - state.CameraState.X) * 32) + 400 - barWidth / 2 + 20;  // Center above player
            int barY = 100 + (int)((state.PlayerPosition.Y - state.CameraState.Y) * 40) + 200 - 15;  // Above player
            
            // Background (empty) bar
            Raylib.DrawRectangle(barX, barY, barWidth, barHeight, new Color(50, 50, 50, 200));
            
            // Foreground (filled) bar - grows as cooldown progresses
            Raylib.DrawRectangle(barX, barY, (int)(barWidth * progress), barHeight, new Color(200, 200, 200, 200));
        }
        
        // Draw crossbow cooldown indicator if player has crossbow
        if (state.CrossbowState.HasCrossbow && state.CrossbowState.CrossbowOnCooldown)
        {
            // Calculate cooldown progress (0.0 to 1.0)
            float progress = state.CrossbowState.CrossbowCooldownTimer / state.CrossbowState.CrossbowCooldown;
            
            // Draw a small cooldown bar below the player
            int barWidth = 30;
            int barHeight = 5;
            
            // Calculate position with camera offset - updated horizontal spacing
            int barX = 100 + (int)((state.PlayerPosition.X - state.CameraState.X) * 32) + 400 - barWidth / 2 + 20;  // Center below player
            int barY = 100 + (int)((state.PlayerPosition.Y - state.CameraState.Y) * 40) + 200 + 45;  // Below player
            
            // Background (empty) bar
            Raylib.DrawRectangle(barX, barY, barWidth, barHeight, new Color(50, 50, 50, 200));
            
            // Foreground (filled) bar - grows as cooldown progresses
            Raylib.DrawRectangle(barX, barY, (int)(barWidth * progress), barHeight, new Color(150, 150, 200, 200));
        }
    }
} 