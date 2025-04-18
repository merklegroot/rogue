namespace Rogue;

public interface IGame
{
    void Run();
}

public class Game : IGame
{
    private readonly IScreenPresenter _presenter;
    private readonly GameState _state;

    public Game(IScreenPresenter presenter)
    {
        _presenter = presenter;
        _state = new GameState { PlayerX = 40, PlayerY = 10 };
    }

    public void Run()
    {
        try
        {
            while (!_presenter.WindowShouldClose())
            {
                _presenter.Update();
                _presenter.Draw(_state);
            }
        }
        finally
        {
            _presenter.Cleanup();
        }
    }
}