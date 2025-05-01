namespace RogueLib.State;

public class ShopItem
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Price { get; set; }
    public Action? OnPurchase { get; set; }
    public ShopCategory Category { get; set; } = ShopCategory.Consumable;

    public required EquipmentEffect EquipmentEffect { get; set; }
}

public class EquipmentEffect
{
    public int? HealthBonus { get; set; }
    public float? SwordCooldown { get; set; }
    public float? SwordReach { get; set; }
    public float? SwordDamage { get; set; }
    public float? SwordSpeed { get; set; }
    public float? SwordStaminaCost { get; set; }
    
}