using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;
using RogueLib.Utils;

namespace RogueLib.Presenters;

public interface IChunkPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class ChunkPresenter : IChunkPresenter
{
    private readonly IDrawUtil _drawUtil;

    public ChunkPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }

    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Calculate the chunk that the player is in
        int playerChunkX = (int)(state.PlayerPosition.X / GameConstants.ChunkSize);
        int playerChunkY = (int)(state.PlayerPosition.Y / GameConstants.ChunkSize);

        // Draw boundaries for the current chunk and adjacent chunks
        for (int y = playerChunkY - 1; y <= playerChunkY + 1; y++)
        {
            for (int x = playerChunkX - 1; x <= playerChunkX + 1; x++)
            {
                DrawChunkBoundary(rayConnection, x, y, state.CameraState.X, state.CameraState.Y);
            }
        }
    }

    private void DrawChunkBoundary(IRayConnection rayConnection, int chunkX, int chunkY, float cameraX, float cameraY)
    {
        // Calculate chunk boundaries in world coordinates
        int left = chunkX * GameConstants.ChunkSize;
        int right = (chunkX + 1) * GameConstants.ChunkSize;
        int top = chunkY * GameConstants.ChunkSize;
        int bottom = (chunkY + 1) * GameConstants.ChunkSize;

        // Convert to screen coordinates using camera position
        int screenLeft = 100 + (int)((left - cameraX) * 32) + 400;
        int screenRight = 100 + (int)((right - cameraX) * 32) + 400;
        int screenTop = 100 + (int)((top - cameraY) * 40) + 200;
        int screenBottom = 100 + (int)((bottom - cameraY) * 40) + 200;

        // Draw chunk boundary lines
        Color boundaryColor = new Color(255, 255, 255, 50); // Semi-transparent white

        // Draw horizontal lines
        Raylib.DrawLine(screenLeft, screenTop, screenRight, screenTop, boundaryColor);
        Raylib.DrawLine(screenLeft, screenBottom, screenRight, screenBottom, boundaryColor);

        // Draw vertical lines
        Raylib.DrawLine(screenLeft, screenTop, screenLeft, screenBottom, boundaryColor);
        Raylib.DrawLine(screenRight, screenTop, screenRight, screenBottom, boundaryColor);

        // Draw chunk coordinates
        string chunkLabel = $"({chunkX},{chunkY})";
        _drawUtil.DrawText(rayConnection, chunkLabel, screenLeft + 5, screenTop + 5, boundaryColor);
    }
} 