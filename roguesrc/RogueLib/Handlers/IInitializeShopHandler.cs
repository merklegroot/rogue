using RogueLib.State;

namespace RogueLib.Handlers;

public interface IInitializeShopHandler
{
    void Handle(GameState state);
} 