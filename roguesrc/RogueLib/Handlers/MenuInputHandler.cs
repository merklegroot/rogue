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
    public void Handle(GameState state)
    {
        while (state.KeyEvents.Count > 0)
        {
            HandleInput(state);
        }

        HandleInput(state);
    }

    private void HandleInput(GameState state)
    {
        bool isGamepadYPressed = Raylib.IsGamepadAvailable(0) && Raylib.IsGamepadButtonPressed(0, (GamepadButton)SteamDeckConstants.ButtonY);

        var key = state.KeyEvents.Any() ? state.KeyEvents.Dequeue() : (KeyboardKey?)null;


        if (key == KeyboardKey.C)
        {
            state.CurrentScreen = GameScreenEnum.CharacterSet;
            return;
        }
        if (key == KeyboardKey.A)
        {
            state.CurrentScreen = GameScreenEnum.Adventure;
            return;
        }
        if (key == KeyboardKey.B)
        {
            state.CurrentScreen = GameScreenEnum.Bestiary;
            return;
        }
        if (key == KeyboardKey.X || isGamepadYPressed)
        {
            Raylib.CloseWindow();
            return;
        }
        // Toggle CRT effect with T key
        if (key == KeyboardKey.T)
        {
            state.ShouldEnableCrtEffect = !state.ShouldEnableCrtEffect;
            return;
        }
    }
} 