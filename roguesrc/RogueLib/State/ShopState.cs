namespace RogueLib.State;

public class ShopState
{
    public int SelectedShopItem { get; set; } = 0;
    public List<ShopItem> ShopInventory { get; set; } = [];
}