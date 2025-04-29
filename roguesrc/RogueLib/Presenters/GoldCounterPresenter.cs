using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface IGoldCounterPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class GoldCounterPresenter : IGoldCounterPresenter
{
    private readonly IDrawUtil _drawUtil;

    public GoldCounterPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Draw gold counter at the top-right of the screen
        int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        string goldText = $"Gold: {state.PlayerGold}";
        int goldTextWidth = Raylib.MeasureText(goldText, ScreenConstants.MenuFontSize);
        
        _drawUtil.DrawText(rayConnection, goldText, screenWidth - goldTextWidth - 20, 20, ScreenConstants.GoldColor);
    }
} 