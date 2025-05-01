using Raylib_cs;
using RogueLib.State;

namespace RogueLib.Handlers;

public interface IShopInputHandler
{
    void Handle(GameState state);
}

public class ShopInputHandler : IShopInputHandler
{
    public void Handle(GameState state)
    {
        while (state.KeyEvents.Count > 0)
        {
            var key = state.KeyEvents.Dequeue();
            
            if (key == KeyboardKey.Escape)
            {
                state.CurrentScreen = GameScreenEnum.Adventure;
            }
            else if (key == KeyboardKey.Up)
            {
                // Move selection up, skipping headers
                int newSelection = state.ShopState.SelectedShopItem - 1;
                while (newSelection >= 0 && state.ShopState.ShopInventory[newSelection].Category == ShopCategory.Header)
                {
                    newSelection--;
                }
                state.ShopState.SelectedShopItem = Math.Max(0, newSelection);
            }
            else if (key == KeyboardKey.Down)
            {
                // Move selection down, skipping headers
                int newSelection = state.ShopState.SelectedShopItem + 1;
                while (newSelection < state.ShopState.ShopInventory.Count && state.ShopState.ShopInventory[newSelection].Category == ShopCategory.Header)
                {
                    newSelection++;
                }
                state.ShopState.SelectedShopItem = Math.Min(state.ShopState.ShopInventory.Count - 1, newSelection);
            }
            else if (key == KeyboardKey.Enter)
            {
                // Try to purchase the selected item
                if (state.ShopState.SelectedShopItem >= 0 && state.ShopState.SelectedShopItem < state.ShopState.ShopInventory.Count)
                {
                    var item = state.ShopState.ShopInventory[state.ShopState.SelectedShopItem];
                    // Don't allow purchasing headers
                    if (item.Category != ShopCategory.Header && state.PlayerGold >= item.Price)
                    {
                        // Deduct gold
                        state.PlayerGold -= item.Price;
                        
                        // Apply the purchase effect
                        item.OnPurchase?.Invoke();
                    }
                }
            }
        }
    }
} 