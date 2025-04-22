namespace RogueLib.State;

// Define ShopItem class before it's used
public class ShopItem
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Price { get; set; }
    public required Action OnPurchase { get; set; }
    public ShopCategory Category { get; set; } = ShopCategory.Consumable;  // Default category
}