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
        // Calculate how many lines we'll need (1 for player + number of alive enemies)
        int lineCount = 1 + state.Enemies.Count(e => e.Alive);
        int panelHeight = lineCount * 20 + 20; // 20 pixels per line + 10px padding top and bottom
        int panelWidth = 300; // Increased width to make it more visible
        
        // Position the panel lower on the screen
        int panelX = 10;
        int panelY = 50;
        
        // Draw off-white border
        Raylib.DrawRectangle(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4, new Color(220, 220, 220, 255));
        
        // Draw black background panel
        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(0, 0, 0, 255));
        
        int debugY = panelY + 10;
        
        // Show player position first
        string playerText = $"Player at ({state.PlayerX}, {state.PlayerY})";
        _screenDrawer.DrawText(rayConnection, playerText, panelX + 10, debugY, Color.Yellow);
        debugY += 20;
        
        // Show enemy positions
        foreach (var enemy in state.Enemies)
        {
            if (enemy.Alive)
            {
                string debugText = $"Enemy at ({enemy.X}, {enemy.Y})";
                _screenDrawer.DrawText(rayConnection, debugText, panelX + 10, debugY, Color.White);
                debugY += 20;
            }
        }
    }
} 