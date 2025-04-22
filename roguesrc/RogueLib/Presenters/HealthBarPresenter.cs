using Raylib_cs;
using RogueLib.Constants;

namespace RogueLib.Presenters;

public interface IHealthBarPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class HealthBarPresenter : IHealthBarPresenter
{
    private readonly Color _emptyHealthColor = new(100, 100, 100, 255);  // Gray color for empty hearts

    private readonly IScreenDrawer _screenDrawer;

    public HealthBarPresenter(IScreenDrawer screenDrawer)
    {
        _screenDrawer = screenDrawer;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        const int heartChar = 3;  // ASCII/CP437 code for heart symbol (♥)
        const int heartSpacing = 30;  // Pixels between hearts
        const int startX = 20;
        const int startY = 20;

        for (int i = 0; i < ScreenConstants.MaxHealth; i++)
        {
            // Determine if this heart should be filled or empty
            Color heartColor = (i < state.CurrentHealth) ? ScreenConstants.HealthColor : _emptyHealthColor;

            // Draw the heart
            _screenDrawer.DrawCharacter(rayConnection, heartChar, startX + (i * heartSpacing), startY, heartColor);
        }
    }
}