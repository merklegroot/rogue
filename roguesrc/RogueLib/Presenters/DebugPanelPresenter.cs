using Raylib_cs;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IDebugPanelPresenter
{
    void Draw(IRayConnection rayConnection, GameState state, bool chargerActive, ChargerEnemyState? charger);
}

public class DebugPanelPresenter : IDebugPanelPresenter
{
    private readonly IDrawUtil _drawUtil;

    public DebugPanelPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state, bool chargerActive, ChargerEnemyState? charger)
    {
        // Calculate how many lines we'll need (1 for player + number of alive enemies + 1 for charger if active)
        int lineCount = 1 + state.Enemies.Count(e => e.Alive);
        if (chargerActive && charger != null)
        {
            lineCount++;
        }
        int panelHeight = lineCount * 20 + 20; // 20 pixels per line + 10px padding top and bottom
        int panelWidth = 300; // Increased width to make it more visible
        
        // Position the panel lower on the screen
        int panelX = 10;
        int panelY = 100;
        
        // Draw off-white border
        Raylib.DrawRectangle(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4, new Color(220, 220, 220, 255));
        
        // Draw translucent gray background panel
        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(100, 100, 100, 180));
        
        int debugY = panelY + 10;
        
        // Show player position first
        string playerText = $"Player at ({state.PlayerX}, {state.PlayerY})";
        _drawUtil.DrawText(rayConnection, playerText, panelX + 10, debugY, Color.Yellow);
        debugY += 20;
        
        // Show charger position if active
        if (chargerActive && charger != null)
        {
            string chargerText = $"Charger at ({charger.X}, {charger.Y})";
            _drawUtil.DrawText(rayConnection, chargerText, panelX + 10, debugY, Color.Orange);
            debugY += 20;
        }
        
        // Show enemy positions
        foreach (var enemy in state.Enemies)
        {
            if (enemy.Alive)
            {
                string debugText = $"Enemy at ({enemy.X}, {enemy.Y})";
                _drawUtil.DrawText(rayConnection, debugText, panelX + 10, debugY, Color.White);
                debugY += 20;
            }
        }
    }
} 