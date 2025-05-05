using Raylib_cs;
using RogueLib;
using RogueLib.Constants;
using RogueLib.Models;
using RogueLib.Utils;
using System.Numerics;
using System.Text;
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
        
        // TODO: Add direction parameter to determine fafcing. For now, draw right-facing
        _drawUtil.DrawCharacter(rayConnection, '[', (int)(screenPosition.X - _screenUtil.ScreenDelX), screenPosition.Y, kestrelColor); // body
        _drawUtil.DrawCharacter(rayConnection, AsciiConstants.Ring, screenPosition.X, 
            (int)(screenPosition.Y - _screenUtil.ScreenDelY / 2.0f), kestrelColor); // eye
        _drawUtil.DrawCharacter(rayConnection, '>', (int)(screenPosition.X + _screenUtil.ScreenDelX), screenPosition.Y, kestrelColor); // beak
    }
    
    private void DrawMinotaur(IRayConnection rayConnection, Coord2dInt screenPosition)
    {
        // The first frame is good enough for the loiks of you.
        // DrawSimpleAsciiFrame(rayConnection, screenPosition, rayConnection.MinotaurFrames.First());
        // DrawSimpleFontFrame(rayConnection, screenPosition, rayConnection.MinotaurFrames.First());
        DrawTranslatedAsciiFrame(rayConnection, screenPosition, rayConnection.MinotaurFrames.First());
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

    private void DrawSimpleAsciiFrame(IRayConnection rayConnection, Coord2dInt screenPosition, List<string> frame)
    {
        var color = Color.White;
        for (int dy = 0; dy < frame.Count; dy++)
        {
            var y = (float)dy - (float)frame.Count / 2.0f + 0.5f;

            var line = frame[dy];
            for (int dx = 0; dx < line.Length; dx++)
            {
                char ch = line[dx];
                if (ch == ' ')
                    continue;

                var x = (float)dx - (float)line.Length / 2.0f;

                _drawUtil.DrawCharacter(rayConnection, ch, (int)(screenPosition.X + x * _screenUtil.ScreenDelX), (int)(screenPosition.Y + y * _screenUtil.ScreenDelY), color);
            }
        }
    }

    private void DrawSimpleFontFrame(IRayConnection rayConnection, Coord2dInt screenPosition, List<string> frame)
    {
        var color = Color.White;
        for (int dy = 0; dy < frame.Count; dy++)
        {
            var y = (float)dy - (float)frame.Count / 2.0f + 0.5f;

            var line = frame[dy];
            for (int dx = 0; dx < line.Length; dx++)
            {
                char ch = line[dx];
                if (ch == ' ')
                    continue;

                var x = (float)dx - (float)line.Length / 2.0f;

                // Instead of DrawCharacter which draws a glyph, we want to write an individual letter from the default font.
                // We can use DrawTextEx to draw a single character.
                var font = rayConnection.MenuFont;
                var fontTexture = font.Texture;

                // Draw the character.
                Raylib.DrawTextEx(font, ch.ToString(), 
                new Vector2(screenPosition.X + x * _screenUtil.ScreenDelX, 
                screenPosition.Y + y * _screenUtil.ScreenDelY), 22, 1, color);
            }
        }

    }
    private void DrawTranslatedAsciiFrame(IRayConnection rayConnection, Coord2dInt screenPosition, List<string> frame)
    {
        var translatedFrame = TranslateAsciiFrame(frame);
        DrawSimpleAsciiFrame(rayConnection, screenPosition, translatedFrame);
    }

    private List<string> TranslateAsciiFrame(List<string> frame)
    {
        /*
        We have characters that almost match what's in the ascii chart, but are using
        character codes from the modern font system.

        For known matches, translate them.
        When a match is not know, just pass the value through.
        */

        var translatedFrame = new List<string>();
        
        foreach (var line in frame)
        {
            var translatedLine = new StringBuilder();
            
            foreach (var ch in line)
            {
                char translatedChar = ch switch
                {
                    '☺' => (char)1,
                    '☻' => (char)2,
                    '♥' => (char)3,
                    '♦' => (char)4,
                    '♣' => (char)5,
                    '♠' => (char)6,
                    
                    // follow the pattern above for the pipe-like characters
                    '│' => (char)7,
                    // '┃' => (char)8,
                    '┃' => (char)222,
                    
                    'Ö' => (char)148,
                    
                    _ => ch // Pass through characters that don't need translation
                };
                
                translatedLine.Append(translatedChar);
            }
            
            translatedFrame.Add(translatedLine.ToString());
        }
        
        return translatedFrame;

    }
}