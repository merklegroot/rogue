using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IInstructionsPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class InstructionsPresenter : IInstructionsPresenter
{
    private readonly IDrawUtil _drawUtil;

    public InstructionsPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Create a collection of instructions
        var instructions = new List<string>
        {
            "Use (W/A/S/D) to move",
            "(SPACE) to swing sword"
        };

        if (state.CrossbowState.HasCrossbow)
        {
            instructions.Add("(F) to fire crossbow");
        }

        instructions.Add("(ESC) to return to menu");
        instructions.Add("(G) for instant gold");
        instructions.Add("(B) to open the shop");
        instructions.Add("(C) to spawn charger");
        instructions.Add("(Z) to toggle enemy spawning");
        instructions.Add("(M) to toggle enemy movement");
        instructions.Add("(R) to round positions to nearest integer");

        // Join instructions with commas and "and" before the last one
        string instructionsText = string.Join(", ", instructions.Take(instructions.Count - 1)) + 
                                (instructions.Count > 1 ? ", and " : "") + 
                                instructions.Last();

        // Calculate text width
        int textWidth = Raylib.MeasureText(instructionsText, ScreenConstants.MenuFontSize);
        int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        
        // If text is too wide, split into two lines
        if (textWidth > screenWidth - 40) // 40 pixels margin
        {
            // Split instructions into two roughly equal groups
            int midPoint = instructions.Count / 2;
            string firstLine = string.Join(", ", instructions.Take(midPoint)) + ",";
            string secondLine = string.Join(", ", instructions.Skip(midPoint).Take(instructions.Count - midPoint - 1)) + 
                              (instructions.Skip(midPoint).Count() > 1 ? ", and " : "") + 
                              instructions.Last();

            // Draw first line
            _drawUtil.DrawText(rayConnection, firstLine, 20, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 80, Color.White);
            // Draw second line below
            _drawUtil.DrawText(rayConnection, secondLine, 20, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 60, Color.White);
        }
        else
        {
            // Draw single line
            _drawUtil.DrawText(rayConnection, instructionsText, 20, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 60, Color.White);
        }
    }
} 