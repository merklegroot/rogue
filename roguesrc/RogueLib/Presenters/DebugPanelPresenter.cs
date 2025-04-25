using Raylib_cs;
using RogueLib.Models;
using RogueLib.State;
using System.Collections.Generic;

namespace RogueLib.Presenters;

public interface IDebugPanelPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class DebugPanelPresenter : IDebugPanelPresenter
{
    private const int PanelWidth = 300;
    private const int PanelX = 10;
    private const int PanelY = 100;
    private readonly IPanelPresenter _panelPresenter;

    public DebugPanelPresenter(IDrawUtil drawUtil)
    {
        _panelPresenter = new PanelPresenter(drawUtil);
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        if (rayConnection == null || state == null)
            return;

        var debugLines = CollectDebugLines(state);
        _panelPresenter.Draw(rayConnection, PanelX, PanelY, PanelWidth, debugLines);
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
} 
