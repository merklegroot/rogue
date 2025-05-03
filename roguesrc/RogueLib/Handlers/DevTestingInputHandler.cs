using RogueLib.State;

namespace RogueLib.Handlers;

public interface IDevTestingInputHandler
{
    void Handle(GameState state);
}

public class DevTestingInputHandler : IDevTestingInputHandler
{
    public void Handle(GameState state)
    {
        if (state.KeyEvents.Count > 0)
        {
            state.CurrentScreen = GameScreenEnum.Menu;
        }
    }
} 