using Raylib_cs;
using RogueLib.State;

namespace RogueLib.Presenters;

public class DebugPanelPresenter
{
    private readonly IScreenDrawer _screenDrawer;

    public DebugPanelPresenter(IScreenDrawer screenDrawer)
    {
        _screenDrawer = screenDrawer;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        int debugY = 20;
        
        // Show player position first
        string playerText = $"Player at ({state.PlayerX}, {state.PlayerY})";
        _screenDrawer.DrawText(rayConnection, playerText, 20, debugY, Color.Yellow);
        debugY += 20;
        
        // Show enemy positions
        foreach (var enemy in state.Enemies)
        {
            if (enemy.Alive)
            {
                string debugText = $"Enemy at ({enemy.X}, {enemy.Y})";
                _screenDrawer.DrawText(rayConnection, debugText, 20, debugY, Color.White);
                debugY += 20;
            }
        }
    }
} 