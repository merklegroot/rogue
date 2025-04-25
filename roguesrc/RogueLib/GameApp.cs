using RogueLib.Presenters;
using RogueLib.State;

namespace RogueLib;

public interface IGameApp
{
    void Run();
}

public class GameAppApp : IGameApp
{
    private readonly IScreenPresenter _presenter;
    private readonly GameState _state;
    private readonly IRayConnectionFactory _rayConnectionFactory;

    public GameAppApp(IScreenPresenter presenter, IRayConnectionFactory rayConnectionFactory)
    {
        _presenter = presenter;
        _rayConnectionFactory = rayConnectionFactory;
        _state = new GameState { PlayerX = 40, PlayerY = 10 };
    }

    public void Run()
    {
        var rayConnection = _rayConnectionFactory.Connect();
        _presenter.Initialize(rayConnection, _state);

        try
        {
            while (!_presenter.WindowShouldClose())
            {
                _presenter.Update(_state);
                _presenter.Draw(rayConnection, _state);
            }
        }
        finally
        {
            rayConnection.Dispose();
        }
    }
}