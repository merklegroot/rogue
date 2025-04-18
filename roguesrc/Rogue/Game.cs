using System;
using System.Threading;

public class Game
{
    private readonly ScreenPresenter _screen;
    private readonly GameState _state;

    public Game()
    {
        _screen = new ScreenPresenter();
        _state = new GameState 
        { 
            PlayerX = 40,
            PlayerY = 10
        };
    }

    public void Run()
    {
        while (!_screen.WindowShouldClose())
        {
            _screen.Update();
            _screen.Draw(_state);
        }

        _screen.Cleanup();
    }
} 