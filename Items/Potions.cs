
public class ItemPotion : ItemBase, ISellable, IBuyable {
    public int? BuyPrice { get; set; }
    public int? SellPrice { get; set; }
    public ItemPotion() {
        Type = ItemType.Potion;
    }

    public class HealthPotion : ItemPotion {
        public HealthPotion() {
            Name = "Health Potion";
            BuyPrice = 40;
            SellPrice = 20;
        }
    }

    public class ManaPotion : ItemPotion {
        public ManaPotion() {
            Name = "Mana Potion";
            BuyPrice = 40;
            SellPrice = 20;
        }
    }
}

