using Raylib_cs;
using RogueLib.Models;
using System.Collections.Generic;

namespace RogueLib.Presenters;

public interface IPanelPresenter
{
    void Draw(IRayConnection rayConnection, int x, int y, int width, IEnumerable<LineInfo> lines);
}

public class PanelPresenter : IPanelPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const int LineHeight = 20;
    private const int PanelPadding = 10;

    // Panel colors
    private static readonly Color PanelBorderColor = new(220, 220, 220, 200);  // Semi-transparent white border
    private static readonly Color PanelBackgroundColor = new(100, 100, 100, 100);  // Semi-transparent gray background

    public PanelPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, int x, int y, int width, IEnumerable<LineInfo> lines)
    {
        if (rayConnection == null || lines == null)
            return;

        var lineList = lines.ToList();
        var panelHeight = (lineList.Count * LineHeight) + (PanelPadding * 2);
        
        DrawPanelBackground(x, y, width, panelHeight);
        DrawPanelLines(rayConnection, x, y, width, lineList);
    }

    private void DrawPanelBackground(int x, int y, int width, int height)
    {
        Raylib.DrawRectangle(
            x - 2, 
            y - 2, 
            width + 4, 
            height + 4, 
            PanelBorderColor
        );
        
        Raylib.DrawRectangle(
            x, 
            y, 
            width, 
            height, 
            PanelBackgroundColor
        );
    }

    private void DrawPanelLines(IRayConnection rayConnection, int x, int y, int width, List<LineInfo> lines)
    {
        var currentY = y + PanelPadding;
        
        foreach (var line in lines)
        {
            _drawUtil.DrawText(rayConnection, line.Contents, x + PanelPadding, currentY, line.Color);
            currentY += LineHeight;
        }
    }
} 