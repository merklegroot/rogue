using System;
using System.Threading;

public class Game
{
    private readonly ScreenController _screen;
    private readonly GameState _state;

    public Game()
    {
        _screen = new ScreenController();
        _state = new GameState 
        { 
            PlayerX = 40,
            PlayerY = 10
        };
    }

    public void Run()
    {
        bool movingRight = true;

        while (!_screen.WindowShouldClose())
        {
            _screen.Update();
            _screen.Draw(_state);

            // Update position
            if (movingRight)
            {
                _state.PlayerX++;
                if (_state.PlayerX >= 79)  // Adjusted for 80-column display
                    movingRight = false;
            } 
            else
            {
                _state.PlayerX--;
                if (_state.PlayerX <= 0)
                    movingRight = true;
            }

            Thread.Sleep(200);
        }

        _screen.Cleanup();
    }
} 