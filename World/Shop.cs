

public interface ISellable {
    int? SellPrice { get; protected set; }
    string? Name { get; protected set; }
}

public interface IBuyable {
    int? BuyPrice { get; protected set; }
    string? Name { get; protected set; }
}



// Shop class to manage items available for sale
public class Shop {
    public List<IBuyable> ItemsForSale { get; private set; } = [];

    public Shop() {
        ItemsForSale = GetBuyableItems();
    }


    public static void BuyItem(Player player, IBuyable itemTobuy) {

        player.AddItem(itemTobuy);

        player.Money -= itemTobuy.BuyPrice;
    }

    public static void SellItem(Player player, ISellable item) {
        player.RemoveItem(item);

        player.Money += item.SellPrice;
    }

    public static Dictionary<ISellable,int> GetSellableItems(Player player) {
        Dictionary<ISellable,int> sellableItems = [];

        foreach (object item in player.InventoryItems) {
            ISellable? sellableItem = item as ISellable;

            // Skip everything else other than sellables and those that have no sell price or name defined
            if (sellableItem is null || sellableItem.Name is null || sellableItem.SellPrice is null) continue;

            sellableItems[sellableItem] = sellableItems.ContainsKey(sellableItem) ? sellableItems[sellableItem] + 1 : 1;
        }

        return sellableItems;
    }

    public static List<IBuyable> GetBuyableItems() {
        List<IBuyable> buyableItems = [];

        // Get all the items that have IBuyable interface applied to them
        var itemTypes = typeof(ItemBase).Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(ItemBase)) && 
                    typeof(IBuyable).IsAssignableFrom(type));

        foreach (var item in itemTypes) {
            IBuyable? itemInstance = (IBuyable?)Activator.CreateInstance(item);

            // Skip everything else other than buybles and those that have no buy price or name defined
            if (itemInstance is null || itemInstance.Name is null || itemInstance.BuyPrice == null) continue;

            buyableItems.Add(itemInstance);  
        }
        
        return buyableItems;
    }
}