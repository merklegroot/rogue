using Raylib_cs;
using RogueLib.Constants;

namespace RogueLib.Presenters;

public interface ICharacterSetPresenter
{
    void Draw(IRayConnection rayConnection);
}

public class CharacterSetPresenter : ICharacterSetPresenter
{
    private readonly IDrawUtil _drawUtil;

    public CharacterSetPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection)
    {
        // Draw all characters in a grid
        for (var charNum = 0; charNum < 256; charNum++)
        {
            var row = charNum / 32;
            var col = charNum % 32;

            _drawUtil.DrawCharacter(rayConnection, 
                charNum,
                20 + (col * 40),
                20 + (row * 60),
                ScreenConstants.SampleColors[charNum % ScreenConstants.SampleColors.Length]
            );
        }

        _drawUtil.DrawText(rayConnection, "Press any key to return", 20, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 40, Color.White);
    }
} 