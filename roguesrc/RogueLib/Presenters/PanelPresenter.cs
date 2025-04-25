using Raylib_cs;
using RogueLib.Models;

namespace RogueLib.Presenters;

public interface IPanelPresenter
{
    void Draw(IRayConnection rayConnection, Coord2dInt position, IEnumerable<LineInfo> lines);
}

public class PanelPresenter : IPanelPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const int LineHeight = 20;
    private const int PanelPadding = 10;
    private const int PanelWidth = 300;

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
        
        DrawPanelBackground(position, new Coord2dInt(PanelWidth, panelHeight));
        DrawPanelLines(rayConnection, position, lineList);
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