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
    Charger = 3,
    Kestrel = 4,
    Minotaur = 5
}

public interface IDrawEnemyUtil
{
    void Draw(IRayConnection rayConnection, EnemyEnum enemyEnum, Coord2dInt screenPosition);
    void Draw(IRayConnection rayConnection, EnemyEnum enemyEnum, Coord2dFloat screenPosition);
    void DrawSpinner(IRayConnection rayConnection, Coord2dFloat screenPosition, float spinAngle);
}

public class DrawEnemyUtil : IDrawEnemyUtil
{
    private readonly IDrawUtil _drawUtil;
    private readonly IScreenUtil _screenUtil;
    public DrawEnemyUtil(IDrawUtil drawUtil, IScreenUtil screenUtil)
    {
        _drawUtil = drawUtil;
        _screenUtil = screenUtil;
    }

    public void Draw(IRayConnection rayConnection, EnemyEnum enemyEnum, Coord2dFloat screenPosition)
    {
        Draw(rayConnection, enemyEnum, new Coord2dInt((int)screenPosition.X, (int)screenPosition.Y));
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

        if (enemyEnum == EnemyEnum.Kestrel)
        {
            DrawKestrel(rayConnection, screenPosition);
            return;
        }

        if (enemyEnum == EnemyEnum.Minotaur)
        {
            DrawMinotaur(rayConnection, screenPosition);
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

    private void DrawKestrel(IRayConnection rayConnection, Coord2dInt screenPosition)
    {
        // Draw the kestrel bird character by character: [°]>
        var kestrelColor = new Color(135, 206, 235, 255); // Sky blue color for the kestrel
        
        // TODO: Add direction parameter to determine facing. For now, draw right-facing
        _drawUtil.DrawCharacter(rayConnection, '[', (int)(screenPosition.X - _screenUtil.ScreenDelX), screenPosition.Y, kestrelColor); // body
        _drawUtil.DrawCharacter(rayConnection, AsciiConstants.Ring, screenPosition.X, 
            (int)(screenPosition.Y - _screenUtil.ScreenDelY / 2.0f), kestrelColor); // eye
        _drawUtil.DrawCharacter(rayConnection, '>', (int)(screenPosition.X + _screenUtil.ScreenDelX), screenPosition.Y, kestrelColor); // beak
    }
    
    private void DrawMinotaur(IRayConnection rayConnection, Coord2dInt screenPosition)
    {
        // Use the first frame from MinotaurFrames
        var color = Color.Brown;
        if (rayConnection.MinotaurFrames == null || rayConnection.MinotaurFrames.Count == 0)
            return;
        var frame = rayConnection.MinotaurFrames[0];
        for (int dy = 0; dy < frame.Count; dy++)
        {
            var line = frame[dy];
            for (int dx = 0; dx < line.Length; dx++)
            {
                char ch = line[dx];
                if (ch == ' ')
                    continue;

                _drawUtil.DrawCharacter(rayConnection, ch, (int)(screenPosition.X + dx * _screenUtil.ScreenDelX), (int)(screenPosition.Y + dy * _screenUtil.ScreenDelY), color);
            }
        }
    }
    
    public void DrawSpinner(IRayConnection rayConnection, Coord2dFloat screenPosition, float spinAngle)
    {
        // Draw the center 'O'
        _drawUtil.DrawCharacter(rayConnection, 'O', (int)screenPosition.X, (int)screenPosition.Y, Color.LightGray);

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
            _drawUtil.DrawCharacter(rayConnection, '%', (int)bx, (int)by, Color.LightGray);
        }
    }
}