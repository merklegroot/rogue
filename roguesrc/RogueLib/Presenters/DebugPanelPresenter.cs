using Raylib_cs;
using RogueLib.State;
using System.Collections.Generic;

namespace RogueLib.Presenters;

public interface IDebugPanelPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class DebugPanelPresenter : IDebugPanelPresenter
{
    private readonly IDrawUtil _drawUtil;

    public DebugPanelPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    private class LineInfo
    {
        public string Contents { get; set; }
        public Color Color { get; set; }
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Create a collection of LineInfo objects to store all debug lines
        var debugLines = new List<LineInfo>();

        // Add player position line
        debugLines.Add(new LineInfo
        {
            Contents = $"Player at ({state.PlayerX}, {state.PlayerY})",
            Color = Color.Yellow
        });

        // Add charger position line if active
        if (state.IsChargerActive && state.Charger.IsAlive)
        {
            debugLines.Add(new LineInfo
            {
                Contents = $"Charger at ({state.Charger.X}, {state.Charger.Y})",
                Color = Color.Orange
            });
        }

        // Add enemy position lines
        foreach (var enemy in state.Enemies)
        {
            if (enemy.IsAlive)
            {
                debugLines.Add(new LineInfo
                {
                    Contents = $"Enemy at ({enemy.X}, {enemy.Y})",
                    Color = Color.White
                });
            }
        }

        // Calculate panel dimensions based on number of lines
        int lineCount = debugLines.Count;
        int panelHeight = lineCount * 20 + 20; // 20 pixels per line + 10px padding top and bottom
        int panelWidth = 300; // Fixed width for the panel
        
        // Position the panel
        int panelX = 10;
        int panelY = 100;
        
        // Draw panel border
        Raylib.DrawRectangle(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4, new Color(220, 220, 220, 255));
        
        // Draw panel background
        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(100, 100, 100, 180));
        
        // Draw each line of debug information
        int currentY = panelY + 10;
        foreach (var line in debugLines)
        {
            _drawUtil.DrawText(rayConnection, line.Contents, panelX + 10, currentY, line.Color);
            currentY += 20;
        }
    }
} 
