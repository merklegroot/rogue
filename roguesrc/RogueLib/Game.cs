namespace RogueLib;

public interface IGame
{
    void Run();
}

public class Game : IGame
{
    private readonly IScreenPresenter _presenter;
    private readonly GameState _state;
    private readonly IRayConnectionFactory _rayConnectionFactory;

    public Game(IScreenPresenter presenter, IRayConnectionFactory rayConnectionFactory)
    {
        _presenter = presenter;
        _rayConnectionFactory = rayConnectionFactory;
        _state = new GameState { PlayerX = 40, PlayerY = 10 };
    }

    public void Run()
    {
        var rayConnection = _rayConnectionFactory.Connect();
        _presenter.Initialize(rayConnection);

        try
        {
            while (!_presenter.WindowShouldClose())
            {
                _presenter.Update();
                _presenter.Draw(rayConnection, _state);
            }
        }
        finally
        {
            rayConnection.Dispose();
        }
    }
}