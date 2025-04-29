using Raylib_cs;
using RogueLib;
using RogueLib.Constants;
using RogueLib.Models;
using RogueLib.Utils;

public enum EnemyEnum
{
    Invalid = 0,
    Cedilla = 1,
    Spinner = 2,
    Charger = 3
}

public interface IDrawEnemyUtil
{
    void Draw(IRayConnection rayConnection, EnemyEnum enemyEnum, Coord2dInt screenPosition);
    void DrawSpinner(IRayConnection rayConnection, Coord2dInt screenPosition, float spinAngle);
}

public class DrawEnemyUtil : IDrawEnemyUtil
{
    private readonly IDrawUtil _drawUtil;

    public DrawEnemyUtil(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }
    
    public void Draw(IRayConnection rayConnection, EnemyEnum enemyEnum, Coord2dInt screenPosition)
    {
        if (enemyEnum == EnemyEnum.Cedilla)
        {
            DrawCedilla(rayConnection, screenPosition);
            return;
        }

        if (enemyEnum == EnemyEnum.Charger)
        {
            DrawCharger(rayConnection, screenPosition);
            return;
        }
    }

    private void DrawCedilla(IRayConnection rayConnection, Coord2dInt screenPosition)
    {
        _drawUtil.DrawCharacter(rayConnection, ScreenConstants.CedillaChar, screenPosition.X, screenPosition.Y, ScreenConstants.EnemyColor);
    }

    private void DrawCharger(IRayConnection rayConnection, Coord2dInt screenPosition)
    {
        _drawUtil.DrawCharacter(rayConnection, ScreenConstants.ChargerCharacter, screenPosition.X, screenPosition.Y, ScreenConstants.ChargerColor);
    }
    
    public void DrawSpinner(IRayConnection rayConnection, Coord2dInt screenPosition, float spinAngle)
    {
        // Draw the center 'O'
        _drawUtil.DrawCharacter(rayConnection, 'O', screenPosition.X, screenPosition.Y, Color.LightGray);

        // Blade length in pixels
        const int bladeLen = 24;
        // Blade thickness
        const int bladeThick = 2;
        // Number of blades (4: up, down, left, right)
        for (var i = 0; i < 4; i++)
        {
            var angle = spinAngle + (float)(Math.PI / 2) * i;
            var dx = (int)(Math.Cos(angle) * bladeLen);
            var dy = (int)(Math.Sin(angle) * bladeLen);
            var bx = screenPosition.X + dx;
            var by = screenPosition.Y + dy;
            Raylib.DrawLineEx(new System.Numerics.Vector2(screenPosition.X, screenPosition.Y), new System.Numerics.Vector2(bx, by), bladeThick, Color.LightGray);
            
            // Draw '%' at blade tip
            _drawUtil.DrawCharacter(rayConnection, '%', bx, by, Color.LightGray);
        }
    }
}