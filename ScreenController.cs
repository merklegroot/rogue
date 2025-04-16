using System;

public class ScreenController
{
    private readonly char _player = '☺';
    private readonly char _ground = '.';
    private readonly int _width;
    private readonly int _height;

    public ScreenController()
    {
        _width = Console.WindowWidth;
        _height = 16;
        Console.CursorVisible = false;
    }

    public void Draw(GameState state)
    {
        Console.Clear();
        
        // Draw ground
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                Console.SetCursorPosition(x, y);
                if (x != state.PlayerX || y != state.PlayerY)
                {
                    Console.Write(_ground);
                }
            }
        }

        // Draw player
        Console.SetCursorPosition(state.PlayerX, state.PlayerY);
        Console.Write(_player);
    }
} 