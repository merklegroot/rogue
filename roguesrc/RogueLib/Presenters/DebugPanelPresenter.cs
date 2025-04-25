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
    private const int LineHeight = 20;
    private const int PanelPadding = 10;
    private const int PanelWidth = 300;
    private const int PanelX = 10;
    private const int PanelY = 100;

    // Panel colors
    private static readonly Color PanelBorderColor = new(220, 220, 220, 200);  // Semi-transparent white border
    private static readonly Color PanelBackgroundColor = new(100, 100, 100, 100);  // Semi-transparent gray background

    public DebugPanelPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    private class LineInfo
    {
        public required string Contents { get; set; }
        public required Color Color { get; set; }
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        if (rayConnection == null || state == null)
            return;

        var debugLines = CollectDebugLines(state);
        DrawDebugPanel(rayConnection, debugLines);
    }

    private List<LineInfo> CollectDebugLines(GameState state)
    {
        var lines = new List<LineInfo>
        {
            new LineInfo
            {
                Contents = $"Player at ({state.PlayerX}, {state.PlayerY})",
                Color = Color.Yellow
            }
        };

        if (state.IsChargerActive && state.Charger.IsAlive)
        {
            lines.Add(new LineInfo
            {
                Contents = $"Charger at ({state.Charger.X}, {state.Charger.Y})",
                Color = Color.Orange
            });
        }

        foreach (var enemy in state.Enemies)
        {
            if (!enemy.IsAlive)
                continue;

            lines.Add(new LineInfo
            {
                Contents = $"Enemy at ({enemy.X}, {enemy.Y})",
                Color = Color.White
            });
        }

        return lines;
    }

    private void DrawDebugPanel(IRayConnection rayConnection, List<LineInfo> debugLines)
    {
        var panelHeight = (debugLines.Count * LineHeight) + (PanelPadding * 2);
        
        DrawPanelBackground(panelHeight);
        DrawDebugLines(rayConnection, debugLines);
    }

    private void DrawPanelBackground(int panelHeight)
    {
        Raylib.DrawRectangle(
            PanelX - 2, 
            PanelY - 2, 
            PanelWidth + 4, 
            panelHeight + 4, 
            PanelBorderColor
        );
        
        Raylib.DrawRectangle(
            PanelX, 
            PanelY, 
            PanelWidth, 
            panelHeight, 
            PanelBackgroundColor
        );
    }

    private void DrawDebugLines(IRayConnection rayConnection, List<LineInfo> debugLines)
    {
        var currentY = PanelY + PanelPadding;
        
        foreach (var line in debugLines)
        {
            _drawUtil.DrawText(rayConnection, line.Contents, PanelX + PanelPadding, currentY, line.Color);
            currentY += LineHeight;
        }
    }
} 
