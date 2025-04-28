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
        if (rayConnection == null || state == null || !state.ShouldShowDebugPanel)
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
                Contents = $"Action Direction: {state.ActionDirection}",
                Color = Color.Yellow
            }
        };

        if (state.Charger.IsAlive)
        {
            lines.Add(new LineInfo
            {
                Contents = $"Charger at ({state.Charger.X:F2}, {state.Charger.Y:F2})",
                Color = Color.Orange
            });
        }

        lines.Add(new LineInfo
            {
                Contents = $"Player at ({state.PlayerPosition.X:F2}, {state.PlayerPosition.Y:F2})",
                Color = Color.Yellow
            });

        foreach (var enemy in state.Enemies)
        {
            if (!enemy.IsAlive)
                continue;

            lines.Add(new LineInfo
            {
                Contents = $"Enemy at ({enemy.Position.X:F2}, {enemy.Position.Y:F2})",
                Color = Color.White
            });
        }

        // List currently pressed gamepad buttons
        if (Raylib.IsGamepadAvailable(0))
        {
            var pressedButtons = new List<string>();
            for (int i = 0; i < 16; i++) // Raylib supports up to 16 buttons
            {
                if (Raylib.IsGamepadButtonDown(0, (GamepadButton)i))
                {
                    pressedButtons.Add(i.ToString());
                }
            }
            string pressed = pressedButtons.Count > 0 ? string.Join(", ", pressedButtons) : "none";
            lines.Add(new LineInfo
            {
                Contents = $"Gamepad Buttons Down: {pressed}",
                Color = Color.SkyBlue
            });
        }

        return lines;
    }
} 
