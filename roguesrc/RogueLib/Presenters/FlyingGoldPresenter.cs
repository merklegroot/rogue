using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IFlyingGoldPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class FlyingGoldPresenter : IFlyingGoldPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const float GoldFlyDuration = 0.3f;

    public FlyingGoldPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Draw flying gold (after everything else so it appears on top)
        foreach (var gold in state.FlyingGold)
        {
            // Calculate progress (0.0 to 1.0)
            float progress = gold.Timer / GoldFlyDuration;
            if (progress > 1.0f) progress = 1.0f;
            
            // Calculate current position using easing function
            // Using a quadratic ease-out for smooth deceleration
            progress = 1 - (1 - progress) * (1 - progress);
            
            // Calculate end position (gold counter location)
            int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
            string goldText = $"Gold: {state.PlayerGold}";
            int goldTextWidth = Raylib.MeasureText(goldText, ScreenConstants.MenuFontSize);
            int endX = screenWidth - goldTextWidth - 40;  // Position before the text
            int endY = 20;  // Same Y as gold counter
            
            // Interpolate between start and end positions
            int currentX = (int)(gold.StartX + (endX - gold.StartX) * progress);
            int currentY = (int)(gold.StartY + (endY - gold.StartY) * progress);
            
            // Calculate alpha (opacity) based on progress - fade out as it approaches the counter
            byte alpha = (byte)(255 * (1.0f - progress * 0.8f));  // Fade to 20% opacity
            Color fadingGoldColor = new Color(ScreenConstants.GoldColor.R, ScreenConstants.GoldColor.G, ScreenConstants.GoldColor.B, alpha);
            
            // Draw the flying gold character with fading effect
            _drawUtil.DrawCharacter(rayConnection, ScreenConstants.GoldChar, currentX, currentY, fadingGoldColor);
        }
    }
} 