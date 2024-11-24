
public class ItemPotion : ItemBase, ISellable, IBuyable {
    public int? BuyPrice { get; set; }
    public int? SellPrice { get; set; }
    public required int Effect { get; set; }
    public ItemPotion() {
        Type = ItemType.Potion;
    }

    public class HealthPotion : ItemPotion {
        public HealthPotion() {
            Name = "Health Potion";
            Effect = 20;
            BuyPrice = 40;
            SellPrice = 20;
        }
    }

    public class ManaPotion : ItemPotion {
        public ManaPotion() {
            Name = "Mana Potion";
            Effect = 20;
            BuyPrice = 40;
            SellPrice = 20;
        }
    }
}

