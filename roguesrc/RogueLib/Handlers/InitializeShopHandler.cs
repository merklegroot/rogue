using RogueLib.State;

namespace RogueLib.Handlers;

public class InitializeShopHandler : IInitializeShopHandler
{
    public void Handle(GameState state)
    {
        // Regular items section
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Id = "health_potion",
            Name = "Health Potion",
            Description = "Restores 25 health",
            Price = 10,
            OnPurchase = () => { state.CurrentHealth = Math.Min(10, state.CurrentHealth + 25); },
            Category = ShopCategory.Consumable,
            EquipmentEffect = new EquipmentEffect { HealthBonus = 25 }
        });
        
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Id = "faster_sword",
            Name = "Faster Sword",
            Description = "Reduces sword cooldown by 0.2s",
            Price = 25,
            OnPurchase = () => { 
                if (state.SwordState.SwordCooldown > 0.3f) // Don't let it go below 0.3s
                    state.SwordState.SwordCooldown -= 0.2f; 
            },
            Category = ShopCategory.Upgrade,
            EquipmentEffect = new EquipmentEffect { SwordCooldown = -0.2f }
        });
        
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Id = "longer_sword",
            Name = "Longer Sword",
            Description = "Increases sword reach",
            Price = 30,
            OnPurchase = () => { state.SwordState.SwordReach++; },
            Category = ShopCategory.Upgrade,
            EquipmentEffect = new EquipmentEffect { SwordReach = 1f }
        });
        
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Id = "temporary_invincibility",
            Name = "Invincibility",
            Description = "5 seconds of invincibility",
            Price = 50,
            OnPurchase = () => { 
                state.IsInvincible = true;
                state.InvincibilityTimer = 0f;
                state.InvincibilityTimer = 5f;
            },
            Category = ShopCategory.Consumable,
            EquipmentEffect = new EquipmentEffect()  // No permanent stat changes for temporary effects
        });
        
        // Weapons section header
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Id = "weapons_header",
            Name = "--- Weapons ---",
            Description = "Coming soon...",
            Price = 0,
            OnPurchase = () => { /* Do nothing, this is just a header */ },
            Category = ShopCategory.Header,
            EquipmentEffect = new EquipmentEffect()  // Headers have no effects
        });
        
        // Add crossbow to weapons section
        state.ShopState.ShopInventory.Add(new ShopItem
        {
            Id = "ranged_crossbow",
            Name = "Crossbow",
            Description = "Fires bolts at enemies from a distance",
            Price = 75,
            OnPurchase = () => { state.CrossbowState.HasCrossbow = true; },
            Category = ShopCategory.Weapon,
            EquipmentEffect = new EquipmentEffect()  // Crossbow is a new weapon type, not a stat modifier
        });
    }
} 