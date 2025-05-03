using RogueLib.State;

namespace RogueLib.Handlers;

public interface IBestiaryInputHandler
{
    void Handle(GameState state);
}

public class BestiaryInputHandler : IBestiaryInputHandler
{
    public void Handle(GameState state)
    {
        if (state.KeyEvents.Count > 0)
        {
            state.CurrentScreen = GameScreenEnum.Menu;
        }
    }
} 