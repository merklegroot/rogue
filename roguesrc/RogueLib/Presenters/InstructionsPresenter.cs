using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface IInstructionsPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class InstructionsPresenter : IInstructionsPresenter
{
    private readonly IDrawUtil _drawUtil;
    private const int Margin = 20;
    private const int LineHeight = 20;

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

        // Calculate screen dimensions
        int screenWidth = ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale;
        int screenHeight = ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale;

        // Build lines gradually
        var lines = new List<string>();
        var currentLine = new List<string>();
        int currentWidth = 0;

        foreach (var instruction in instructions)
        {
            // Try adding the instruction to the current line
            string testLine = string.Join(", ", currentLine.Concat(new[] { instruction }));
            int testWidth = Raylib.MeasureText(testLine, ScreenConstants.MenuFontSize);

            if (testWidth > screenWidth - (Margin * 2))
            {
                // Current line is full, add it to lines and start a new one
                if (currentLine.Count > 0)
                {
                    lines.Add(string.Join(", ", currentLine));
                }
                currentLine = new List<string> { instruction };
                currentWidth = Raylib.MeasureText(instruction, ScreenConstants.MenuFontSize);
            }
            else
            {
                // Add to current line
                currentLine.Add(instruction);
                currentWidth = testWidth;
            }
        }

        // Add the last line if it has content
        if (currentLine.Count > 0)
        {
            lines.Add(string.Join(", ", currentLine));
        }

        // Draw lines from bottom up
        int totalHeight = lines.Count * LineHeight;
        int startY = screenHeight - totalHeight - 20;

        for (int i = 0; i < lines.Count; i++)
        {
            int y = startY + (i * LineHeight);
            _drawUtil.DrawText(rayConnection, lines[i], Margin, y, Color.White);
        }
    }
} 