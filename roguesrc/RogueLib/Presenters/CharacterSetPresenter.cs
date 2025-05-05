using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface ICharacterSetPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class CharacterSetPresenter : ICharacterSetPresenter
{
    private readonly IDrawUtil _drawUtil;

    public CharacterSetPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        const int offsetX = 32;
        const int offsetY = 32;
        const int totalChars = 256;

        // Draw all characters in a grid
        for (var charNum = 0; charNum < totalChars; charNum++)
        {
            var row = charNum / 32;
            var col = charNum % 32;

            _drawUtil.DrawCharacter(rayConnection, 
                charNum,
                offsetX + (col * 35),
                offsetY + (row * 60),
                charNum == state.SelectedCharIndex ? Color.Gold : ScreenConstants.SampleColors[charNum % ScreenConstants.SampleColors.Length]
            );
        }

        // integer, rouund up
        var totalRows = (int)Math.Ceiling((double)totalChars / 32);
        var infoY = offsetY + (totalRows * 60) + 20;

        // Show selected character and its index below the grid
        var selectedChar = (char)state.SelectedCharIndex;
        var info = $"Selected: '{selectedChar}' (Index: {state.SelectedCharIndex})";
        // _drawUtil.DrawText(rayConnection, info, offsetX, offsetY + 32 * 60 + 20, Color.White);
        _drawUtil.DrawText(rayConnection, info, offsetX, infoY, Color.White);

        _drawUtil.DrawText(rayConnection, "Press any key to return", 20, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 40, Color.White);
    }
} 