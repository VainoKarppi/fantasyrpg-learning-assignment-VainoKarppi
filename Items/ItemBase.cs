using System.Reflection;

public enum ItemType {
    WeaponBase,
    MeleeWeapon,
    RangedWeapon,
    MageWeapon,

    Potion,

    ArmorBase,
    MeleeArmor,
    RangedArmor,
    MageArmor,

    Sellable,
    Gem,
    Jewelry
}

public enum ItemRarity {
    Common,
    Uncommon,
    Rare,
    Legendary
}


public class ItemBase {
    public string? Name { get; set; }
    public ItemType Type { get; set; }
    public ItemRarity Rarity { get; set; }
    public double Weight { get; set;}

    public ItemBase() {
        Rarity = ItemRarity.Common;
        Weight = 1;
    }

    public static ItemBase? GetItemByName(string? name) {
        if (name == null) return null;
        
        var itemTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ItemBase)) && !t.IsAbstract);

        foreach (var type in itemTypes) {
            if (Activator.CreateInstance(type) is ItemBase itemInstance &&
                itemInstance.Name != null &&
                itemInstance.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                    return itemInstance;
            }
        }

        return null;
    }
}