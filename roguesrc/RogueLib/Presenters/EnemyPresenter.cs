using Raylib_cs;
using RogueLib.State;
using RogueLib.Constants;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface IEnemyPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class EnemyPresenter : IEnemyPresenter
{
    private readonly IDrawUtil _drawUtil;

    public EnemyPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        foreach (var enemy in state.Enemies)
        {
            DrawCedilla(rayConnection, state, enemy);
        }
        
        foreach (var spinner in state.Spinners)
        {
            DrawSpinner(rayConnection, state, spinner);
        }
    }

    private void DrawCedilla(IRayConnection rayConnection, GameState state, Enemy enemy)
    {
        if (!enemy.IsAlive)
            return;

        int enemyScreenX = 100 + (int)((enemy.Position.X - state.CameraState.X) * 32) + 400;
        int enemyScreenY = 100 + (int)((enemy.Position.Y - state.CameraState.Y) * 40) + 200;
        
        if (!IsOnScreen(enemyScreenX, enemyScreenY))
            return;

        _drawUtil.DrawCharacter(rayConnection, ScreenConstants.CedillaChar, enemyScreenX, enemyScreenY, ScreenConstants.EnemyColor);
    }

    private void DrawSpinner(IRayConnection rayConnection, GameState state, SpinnerEnemyState spinner)
    {
        if (!spinner.IsAlive)
            return;

        int spinnerScreenX = 100 + (int)((spinner.X - state.CameraState.X) * 32) + 400;
        int spinnerScreenY = 100 + (int)((spinner.Y - state.CameraState.Y) * 40) + 200;

        if (!IsOnScreen(spinnerScreenX, spinnerScreenY))
            return;

        // Draw the center 'O'
        _drawUtil.DrawCharacter(rayConnection, 'O', spinnerScreenX, spinnerScreenY, Color.LightGray);

        // Blade length in pixels
        int bladeLen = 24;
        // Blade thickness
        int bladeThick = 2;
        // Number of blades (4: up, down, left, right)
        for (int i = 0; i < 4; i++)
        {
            float angle = spinner.SpinAngle + (float)(Math.PI / 2) * i;
            int dx = (int)(Math.Cos(angle) * bladeLen);
            int dy = (int)(Math.Sin(angle) * bladeLen);
            int bx = spinnerScreenX + dx;
            int by = spinnerScreenY + dy;
            Raylib.DrawLineEx(new System.Numerics.Vector2(spinnerScreenX, spinnerScreenY), new System.Numerics.Vector2(bx, by), bladeThick, Color.LightGray);
            // Draw '%' at blade tip
            _drawUtil.DrawCharacter(rayConnection, '%', bx, by, Color.LightGray);
        }
    }

    private bool IsOnScreen(int enemyScreenX, int enemyScreenY)
    {
        return enemyScreenX >= 0 && enemyScreenX < ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale &&
            enemyScreenY >= 0 && enemyScreenY < ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;
    }
} 