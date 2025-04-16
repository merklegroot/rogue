using System;
using System.Threading;

public class Game
{
    public void Run()
    {
        Console.CursorVisible = false;
        int playerX = 40; // Starting position
        bool movingRight = true;
        char player = '☺'; // Smiley face character

        while (true)
        {
            Console.Clear();
            
            // Draw player
            Console.SetCursorPosition(playerX, 10);
            Console.Write(player);

            // Update position
            if (movingRight)
            {
                playerX++;
                if (playerX >= Console.WindowWidth - 1)
                    movingRight = false;
            }
            else
            {
                playerX--;
                if (playerX <= 0)
                    movingRight = true;
            }

            Thread.Sleep(50); // Control movement speed
        }
    }
} 