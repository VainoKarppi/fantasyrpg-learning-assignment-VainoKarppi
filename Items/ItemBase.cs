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
}