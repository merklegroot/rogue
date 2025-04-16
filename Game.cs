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

        while (true)
        {
            _screen.Draw(_state);

            // Update position
            if (movingRight)
            {
                _state.PlayerX++;
                if (_state.PlayerX >= Console.WindowWidth - 1)
                    movingRight = false;
            }
            else
            {
                _state.PlayerX--;
                if (_state.PlayerX <= 0)
                    movingRight = true;
            }

            Thread.Sleep(50); // Control movement speed
        }
    }
} 