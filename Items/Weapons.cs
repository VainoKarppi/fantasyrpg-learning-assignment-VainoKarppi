


public class ItemWeapon : ItemBase {
    public double Damage { get; set; }
    public int Durability { get; set; }
    public int Range { get; set; } = 40;
    public int ReloadTime { get; set; } = 1000;

    public ItemWeapon() {
        Type = ItemType.WeaponBase;
    }
}



public class MeleeWeapon : ItemWeapon {

    public MeleeWeapon() {
        Type = ItemType.MeleeWeapon;
    }
    public class Fists : MeleeWeapon {
        public Fists() {
            Name = "Fist";
            Damage = 10;

            Range = 35;
        }
    }
    public class BasicSword : MeleeWeapon, ISellable, IBuyable {
        public int? SellPrice { get; set; }
        public int? BuyPrice { get; set; }
        public BasicSword() {
            Name = "Basic Sword";
            Damage = 90;
            Durability = 80;

            SellPrice = 200;
            BuyPrice = 500;

            Range = 50;

            Rarity = ItemRarity.Common;
        }
    }

    public class LegendarySword : MeleeWeapon, ISellable {
        public int? SellPrice { get; set; }
        public LegendarySword() {
            Name = "Legendary Sword";
            Damage = 300;
            Durability = 200;
            Range = 55;

            SellPrice = 1000;
            
            Rarity = ItemRarity.Legendary;
        }
    }
}




public class RangedWeapon : ItemWeapon {
    

    public RangedWeapon() {
        Type = ItemType.RangedWeapon;
    }

    public class BasicBow : RangedWeapon, ISellable, IBuyable  {
        public int? SellPrice { get; set; }
        public int? BuyPrice { get; set; }
        public BasicBow() {
            Name = "Basic Bow";
            Damage = 30;
            Durability = 40;

            Range = 190;

            BuyPrice = 200;
            SellPrice = 100;
        }
    }

    public class LegendaryBow : RangedWeapon, ISellable {
        public int? SellPrice { get; set; }
        public LegendaryBow() {
            Name = "Legendary Bow";
            Damage = 300;
            Durability = 80;
            Range = 250;

            SellPrice = 1200;
        }
    }
}



public class MageWeapon : ItemWeapon {
    public MageWeapon() {
        Type = ItemType.MageWeapon;
    }
    
    public class BasicWand : MageWeapon, ISellable, IBuyable {
        public int? SellPrice { get; set; }
        public int? BuyPrice { get; set; }
        public BasicWand() {
            Name = "Basic Magic Wand";
            Damage = 15;
            Durability = 40;
            Range = 230;

            BuyPrice = 300;
            SellPrice = 80;
        }
    }

    public class LegendaryWand : MageWeapon, ISellable {
        public int? SellPrice { get; set; }
        public LegendaryWand() {
            Name = "Legendary Magic Wand";
            Damage = 350;
            Durability = 50;
            Range = 270;

            SellPrice = 1400;
        }
    }
}

