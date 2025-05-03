using RogueLib.Models;
using RogueLib.Presenters;
using RogueLib.State;
using Raylib_cs;

namespace RogueLib;

public interface IGameApp
{
    void Run();
}

public class GameApp : IGameApp
{
    private readonly IScreenPresenter _presenter;
    private readonly GameState _state;
    private readonly IRayConnectionFactory _rayConnectionFactory;

    public GameApp(IScreenPresenter presenter, IRayConnectionFactory rayConnectionFactory)
    {
        _presenter = presenter;
        _rayConnectionFactory = rayConnectionFactory;
        _state = new GameState
        {
            PlayerPosition = new Coord2dFloat(40, 10)
        };
    }

    public void Run()
    {
        var rayConnection = _rayConnectionFactory.Connect();
        _presenter.Initialize(rayConnection, _state);

        try
        {
            while (!WindowShouldClose())
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

    private bool WindowShouldClose()
    {
        // Check if window close was requested (like clicking the X button)
        // but ignore if it was triggered by the Escape key
        bool closeRequested = Raylib.WindowShouldClose();
        
        // If close is requested and it's because of Escape key, ignore it
        if (closeRequested && Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            return false;
        }
        
        return closeRequested;
    }
}