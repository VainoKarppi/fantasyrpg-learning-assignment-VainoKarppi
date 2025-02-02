
using System.Reflection;

public class ItemArmor : ItemBase {
    public double MeleeAttackMultiplier { get; set; } =  1;
    public double MeleeDefenseMultiplier { get; set; } =  1;

    public double MageAttackMultiplier { get; set; } =  1;
    public double MageDefenseMultiplier { get; set; } =  1;

    public double RangedAttackMultiplier { get; set; } =  1;
    public double RangedDefenseMultiplier { get; set; } =  1;

    public int Defense { get; set; }
    public int Durability { get; set; }

    public ItemArmor() {
        Type = ItemType.ArmorBase;
    }

    public static ItemArmor? GetArmorByName(string? name) {
        if (name == null) return null;
        
        var weaponTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ItemArmor)) && !t.IsAbstract);

        foreach (var type in weaponTypes) {
            if (Activator.CreateInstance(type) is ItemArmor armorInstance &&
                armorInstance.Name != null &&
                armorInstance.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                    return armorInstance;
            }
        }

        return null;
    }
}



public class MeleeArmor : ItemArmor {
    public MeleeArmor() {
        Type = ItemType.MeleeArmor;
    }
}
public class MetalArmor : MeleeArmor, IBuyable {
    public int? BuyPrice { get; set; }
    public MetalArmor() {
        Name = "Metal Armor";

        MageDefenseMultiplier = 2;
        RangedDefenseMultiplier = 2;

        MageAttackMultiplier = 0.8;
        RangedAttackMultiplier = 0.8;

        MeleeDefenseMultiplier = 2;

        Defense = 100;
        Durability = 80;

        BuyPrice = 700;
    }
}


public class RangedArmor : ItemArmor {
    public RangedArmor() {
        Type = ItemType.RangedArmor;
    }
}

public class LeatherArmor : RangedArmor {
    public LeatherArmor() {
        Name = "Leather Armor";

        MageDefenseMultiplier = 1.5;
        RangedDefenseMultiplier = 1.2;

        RangedAttackMultiplier = 2;

        MeleeDefenseMultiplier = 1.2;

        Defense = 30;
        Durability = 40;
    }
}



public class MageArmor : ItemArmor {
    public MageArmor() {
        Type = ItemType.MageArmor;
    }
}

public class MageRobe : MageArmor {
    public MageRobe() {
        Name = "Mage Robe";

        MageAttackMultiplier = 2;
        MageDefenseMultiplier = 1.5;

        MeleeDefenseMultiplier = 1.2;

        Defense = 15;
        Durability = 40;
    }
}
