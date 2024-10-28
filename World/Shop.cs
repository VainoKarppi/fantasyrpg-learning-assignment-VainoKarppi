

public interface ISellable {
    int? SellPrice { get; protected set; }
    string? Name { get; protected set; }
}

public interface IBuyable {
    int? BuyPrice { get; protected set; }
    string? Name { get; protected set; }
}


public class ShopItem {
    public object Item { get; }
    public int Price { get; }

    public ShopItem(object item, int price) {
        Item = item;
        Price = price;
    }
}



// Shop class to manage items available for sale
public class Shop {
    public List<IBuyable> ItemsForSale { get; private set; } = [];

    public Shop() {
        ItemsForSale = GetBuyableItems();
    }


    public void BuyItem(Player player, IBuyable itemTobuy) {

        player.AddItem(itemTobuy);

        player.Money -= itemTobuy.BuyPrice;
    }

    public void SellItem(Player player, ISellable item) {
        if (!player.RemoveItem(item)) throw new Exception("This item cannot be sold!");

        player.Money += item.SellPrice;
    }

    public Dictionary<ISellable,int> GetSellableItems(Player player) {
        Dictionary<ISellable,int> sellableItems = [];

        foreach (object item in player.InventoryItems) {
            ISellable? sellableItem = item as ISellable;

            // Skip everything else other than sellables and those that have no sell price or name defined
            if (sellableItem is null || sellableItem.Name is null || sellableItem.SellPrice is null) continue;

            sellableItems[sellableItem] = sellableItems.ContainsKey(sellableItem) ? sellableItems[sellableItem] + 1 : 1;
        }

        return sellableItems;
    }

    public List<IBuyable> GetBuyableItems() {
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


    public virtual void DisplaySellableItems(Dictionary<ISellable, int> sellableItems) {
        Console.WriteLine("-------------SELLABLES-------------");
        int i = 0;
        foreach (KeyValuePair<ISellable,int> item in sellableItems) {
            Console.WriteLine($"    [{i}] {item.Key.Name}, Price: {item.Key.SellPrice}");
            i++;
        }
        Console.WriteLine("-----------------------------------");
    }


    // Method to display the items available in the shop
    public virtual void DisplayShopInventory() {
        Console.WriteLine("-----------Shop Inventory-----------");
        int i = 0;
        foreach (IBuyable shopItem in ItemsForSale) {
            Console.WriteLine($"[{i}] Item: {shopItem.Name}, Price: {shopItem.BuyPrice}");
            i++;
        }
        Console.WriteLine("------------------------------------");
    }
}