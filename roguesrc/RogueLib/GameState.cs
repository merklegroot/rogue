namespace RogueLib;

public class GameState
{
    public int PlayerX { get; set; }
    public int PlayerY { get; set; }
    
    // Add a constructor to initialize player position
    public GameState()
    {
        PlayerX = 10; // Default starting position
        PlayerY = 5;  // Default starting position
    }
}