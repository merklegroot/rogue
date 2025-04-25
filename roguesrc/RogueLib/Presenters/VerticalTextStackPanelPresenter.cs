using Raylib_cs;
using RogueLib.Models;
using RogueLib.Constants;

namespace RogueLib.Presenters;

public interface IVerticalTextStackPanelPresenter
{
    void Draw(IRayConnection rayConnection, Coord2dInt position, IEnumerable<LineInfo> lines, string? title = null);
}

public class VerticalTextStackPanelPresenter : IVerticalTextStackPanelPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const int LineHeight = 30;
    private const int PanelPadding = 10;
    private const int HorizontalPadding = 20;
    private const int MinPanelWidth = 300;
    private int _maxWidthSeen = MinPanelWidth;

    private static readonly Color PanelBorderColor = new(220, 220, 220, 200);  // Semi-transparent white border
    private static readonly Color PanelBackgroundColor = new(100, 100, 100, 100);  // Semi-transparent gray background
    private static readonly Color TitleBackgroundColor = new(70, 70, 70, 100);  // Darker semi-transparent gray for title
    private static readonly Color TitleColor = new(255, 255, 255, 255);  // White title

    public VerticalTextStackPanelPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, Coord2dInt position, IEnumerable<LineInfo> lines, string? title = null)
    {
        if (rayConnection == null || lines == null)
            return;

        var lineList = lines.ToList();
        var titleHeight = title != null ? LineHeight + PanelPadding + (LineHeight / 2) : 0;
        var panelHeight = (lineList.Count * LineHeight) + (PanelPadding * 2) + titleHeight;
        var panelWidth = CalculatePanelWidth(rayConnection, lineList, title);
        
        DrawPanelBackground(position, new Coord2dInt(panelWidth, panelHeight));
        DrawPanelContent(rayConnection, position, lineList, title);
    }

    private int CalculatePanelWidth(IRayConnection rayConnection, List<LineInfo> lines, string? title)
    {
        if (!lines.Any() && title == null)
            return _maxWidthSeen;

        var maxLineWidth = lines.Max(line => 
            Raylib.MeasureTextEx(rayConnection.MenuFont, line.Contents, ScreenConstants.MenuFontSize, 1).X);
            
        var titleWidth = title != null ? 
            Raylib.MeasureTextEx(rayConnection.MenuFont, title, ScreenConstants.MenuFontSize, 1).X : 0;
            
        var newWidth = Math.Max((int)Math.Max(maxLineWidth, titleWidth) + (HorizontalPadding * 2), MinPanelWidth);
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

    private void DrawPanelContent(IRayConnection rayConnection, Coord2dInt position, List<LineInfo> lines, string? title)
    {
        var currentY = position.Y + PanelPadding;
        
        if (title != null)
        {
            // Draw title background
            Raylib.DrawRectangle(
                position.X,
                currentY - PanelPadding,
                _maxWidthSeen,
                LineHeight + PanelPadding + (LineHeight / 2),
                TitleBackgroundColor
            );
            
            _drawUtil.DrawText(rayConnection, title, position.X + HorizontalPadding, currentY, TitleColor);
            currentY += LineHeight + PanelPadding + (LineHeight / 2);
        }
        
        foreach (var line in lines)
        {
            _drawUtil.DrawText(rayConnection, line.Contents, position.X + HorizontalPadding, currentY, line.Color);
            currentY += LineHeight;
        }
    }
}