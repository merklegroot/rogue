using Raylib_cs;
using RogueLib.Models;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using RogueLib.Constants;

namespace RogueLib.Presenters;

public interface IPanelPresenter
{
    void Draw(IRayConnection rayConnection, Coord2dInt position, IEnumerable<LineInfo> lines);
}

public class PanelPresenter : IPanelPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const int LineHeight = 30;
    private const int PanelPadding = 10;
    private const int MinPanelWidth = 300;
    private int _maxWidthSeen = MinPanelWidth;

    // Panel colors
    private static readonly Color PanelBorderColor = new(220, 220, 220, 200);  // Semi-transparent white border
    private static readonly Color PanelBackgroundColor = new(100, 100, 100, 100);  // Semi-transparent gray background

    public PanelPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, Coord2dInt position, IEnumerable<LineInfo> lines)
    {
        if (rayConnection == null || lines == null)
            return;

        var lineList = lines.ToList();
        var panelHeight = (lineList.Count * LineHeight) + (PanelPadding * 2);
        var panelWidth = CalculatePanelWidth(rayConnection, lineList);
        
        DrawPanelBackground(position, new Coord2dInt(panelWidth, panelHeight));
        DrawPanelLines(rayConnection, position, lineList);
    }

    private int CalculatePanelWidth(IRayConnection rayConnection, List<LineInfo> lines)
    {
        if (!lines.Any())
            return _maxWidthSeen;

        var maxLineWidth = lines.Max(line => 
            Raylib.MeasureTextEx(rayConnection.MenuFont, line.Contents, ScreenConstants.MenuFontSize, 1).X);
        var newWidth = Math.Max((int)maxLineWidth + (PanelPadding * 2), MinPanelWidth);
        _maxWidthSeen = Math.Max(_maxWidthSeen, newWidth);
        return _maxWidthSeen;
    }

    private void DrawPanelBackground(Coord2dInt position, Coord2dInt size)
    {
        Raylib.DrawRectangle(
            position.X - 2, 
            position.Y - 2, 
            size.X + 4, 
            size.Y + 4, 
            PanelBorderColor
        );
        
        Raylib.DrawRectangle(
            position.X, 
            position.Y, 
            size.X, 
            size.Y, 
            PanelBackgroundColor
        );
    }

    private void DrawPanelLines(IRayConnection rayConnection, Coord2dInt position, List<LineInfo> lines)
    {
        var currentY = position.Y + PanelPadding;
        
        foreach (var line in lines)
        {
            _drawUtil.DrawText(rayConnection, line.Contents, position.X + PanelPadding, currentY, line.Color);
            currentY += LineHeight;
        }
    }
}