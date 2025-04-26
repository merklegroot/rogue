using Raylib_cs;
using RogueLib.Models;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IDebugPanelPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class DebugPanelPresenter : IDebugPanelPresenter
{    
    private static readonly Coord2dInt PanelPosition = new(10, 100);
    private readonly IVerticalTextStackPanelPresenter _panelPresenter;

    public DebugPanelPresenter(IDrawUtil drawUtil)
    {
        _panelPresenter = new VerticalTextStackPanelPresenter(drawUtil);
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        if (rayConnection == null || state == null)
            return;

        var debugLines = CollectDebugLines(state);
        _panelPresenter.Draw(rayConnection, PanelPosition, debugLines, "Debug");
    }

    private List<LineInfo> CollectDebugLines(GameState state)
    {
        var lines = new List<LineInfo>
        {
            new LineInfo
            {
                Contents = $"Enemy Spawning: {(state.IsEnemySpawnEnabled ? "Enabled" : "Disabled")}",
                Color = state.IsEnemySpawnEnabled ? Color.Green : Color.Red
            },
            new LineInfo
            {
                Contents = $"Enemy Movement: {(state.IsEnemyMovementEnabled ? "Enabled" : "Disabled")}",
                Color = state.IsEnemyMovementEnabled ? Color.Green : Color.Red
            },
            new LineInfo
            {
                Contents = $"Player at ({state.PlayerPosition.X:F2}, {state.PlayerPosition.Y:F2})",
                Color = Color.Yellow
            },
            new LineInfo
            {
                Contents = $"Action Direction: {state.ActionDirection}",
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
                Contents = $"Enemy at ({enemy.Position.X}, {enemy.Position.Y})",
                Color = Color.White
            });
        }

        return lines;
    }
} 
