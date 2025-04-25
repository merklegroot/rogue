using Raylib_cs;
using RogueLib.State;
using RogueLib.Constants;

namespace RogueLib.Handlers;

public interface IMenuInputHandler
{
    void Handle(GameState state);
}

public class MenuInputHandler : IMenuInputHandler
{
    private bool _shouldEnableCrtEffect = true;

    public void Handle(GameState state)
    {
        while (state.KeyEvents.Count > 0)
        {
            var key = state.KeyEvents.Dequeue();
            if (key == KeyboardKey.C)
            {
                state.CurrentScreen = GameScreenEnum.CharacterSet;
                break;
            }
            if (key == KeyboardKey.A)
            {
                state.CurrentScreen = GameScreenEnum.Adventure;
                break;
            }
            if (key == KeyboardKey.X)
            {
                Raylib.CloseWindow();
                break;
            }
            // Toggle CRT effect with T key
            if (key == KeyboardKey.T)
            {
                _shouldEnableCrtEffect = !_shouldEnableCrtEffect;
                break;
            }
        }
    }
} 