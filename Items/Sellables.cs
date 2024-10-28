




public class ItemDrop : ItemBase, ISellable {
    public int? SellPrice { get; set; }

    public class Gems : ItemDrop {
        public class Quartz : Gems {
            public Quartz() {
                Name = "Quartz Gem";
                SellPrice = 10;
                Rarity = ItemRarity.Common;
            }
        }
        public class Ruby : Gems {
            public Ruby() {
                Name = "Ruby Gem";
                SellPrice = 50;
                Rarity = ItemRarity.Uncommon;
            }
        }

        public class Sapphire : Gems {
            public Sapphire() : base() {
                Name = "Sapphire Gem";
                SellPrice = 150;
                Rarity = ItemRarity.Rare;
            }
        }

        public class Diamond : Gems {
            public Diamond() : base() {
                Name = "Diamond Gem";
                SellPrice = 300;
                Rarity = ItemRarity.Legendary;
            }
        }
    }
}
