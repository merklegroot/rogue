using System.Numerics;
using Raylib_cs;
using RogueLib.Constants;
using RogueLib.State;

namespace RogueLib.Presenters;

public interface IShopPresenter
{
    void Draw(IRayConnection rayConnection, GameState state);
}

public class ShopPresenter : IShopPresenter
{
    private readonly IDrawUtil _drawUtil;
    
    public ShopPresenter(IDrawUtil drawUtil)
    {
        _drawUtil = drawUtil;
    }
    
    public void Draw(IRayConnection rayConnection, GameState state)
    {
        // Draw shop background
        Raylib.DrawRectangle(50, 50, ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale - 100, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 100, new Color(30, 30, 30, 230));
        Raylib.DrawRectangleLines(50, 50, ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale - 100, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 100, Color.Gold);
        
        // Draw shop title
        string shopTitle = "ADVENTURER'S SHOP";
        Vector2 titleSize = Raylib.MeasureTextEx(rayConnection.MenuFont, shopTitle, 32, 1);
        Raylib.DrawTextEx(rayConnection.MenuFont, shopTitle, new Vector2((ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale) / 2 - titleSize.X / 2, 70), 32, 1, Color.Gold);
        
        // Draw player's gold
        string goldText = $"Your Gold: {state.PlayerGold}";
        _drawUtil.DrawText(rayConnection, goldText, 70, 120, ScreenConstants.GoldColor);
        
        // Draw shop items
        int itemY = 170;
        int itemSpacing = 50;
        
        for (int i = 0; i < state.ShopState.ShopInventory.Count; i++)
        {
            var item = state.ShopState.ShopInventory[i];
            Color itemColor = i == state.ShopState.SelectedShopItem ? Color.White : new Color(200, 200, 200, 200);
            
            // Draw selection indicator
            if (i == state.ShopState.SelectedShopItem)
            {
                Raylib.DrawRectangle(60, itemY - 5, ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale - 120, 40, new Color(60, 60, 60, 150));
            }
            
            // Draw item name and price
            _drawUtil.DrawText(rayConnection, item.Name, 70, itemY, itemColor);
            string priceText = $"{item.Price} gold";
            _drawUtil.DrawText(rayConnection, priceText, ScreenConstants.Width * ScreenConstants.CharWidth * ScreenConstants.DisplayScale - 200, itemY, itemColor);
            
            // Draw item description
            _drawUtil.DrawText(rayConnection, item.Description, 70, itemY + 20, new Color(150, 150, 150, 200));
            
            itemY += itemSpacing;
        }
        
        // Draw instructions
        _drawUtil.DrawText(rayConnection, "Use UP/DOWN to select, ENTER to buy, ESC to exit shop", 70, ScreenConstants.Height * ScreenConstants.CharHeight * ScreenConstants.DisplayScale - 100, Color.White);
    }
}