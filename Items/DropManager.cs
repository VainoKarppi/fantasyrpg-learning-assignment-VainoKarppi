
public class DropManager {

    // Make the DropList contain as many sub arrays as there are rarity types
    private static List<List<ItemDrop>> DropList = 
        Enumerable.Range(0, Enum.GetNames(typeof(ItemRarity)).Length)
                  .Select(_ => new List<ItemDrop>())
                  .ToList();

    public static int DropChance = 70;

    public static Dictionary<ItemRarity, int> DropChances = new Dictionary<ItemRarity, int> {
        { ItemRarity.Common, 40 },
        { ItemRarity.Uncommon, 30 },
        { ItemRarity.Rare, 20 },
        { ItemRarity.Legendary, 10 }
    };

    public DropManager() {

        var itemTypes = typeof(ItemDrop).Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(ItemDrop)) && 
                    typeof(ItemDrop).IsAssignableFrom(type));

        
        // Populate DropList based on items that are in class "ItemDrop"
        foreach (var type in itemTypes) {
            ItemDrop? itemInstance = (ItemDrop?)Activator.CreateInstance(type);

            // Skip everything else other than buybles and those that have no buy price or name defined
            if (itemInstance is null || itemInstance.Name is null) continue;

            int rarity = (int)itemInstance.Rarity;
            DropList[rarity].Add(itemInstance);
        }
    }

    public static ItemDrop? GetRandomDrop() {
        Random random = new Random();

        // Check the initial drop chance
        if (random.Next(100) > DropChance) return null;


        // Determine the total chance sum
        int totalChance = DropChances.Values.Sum();
        int randomNumber = random.Next(1, totalChance + 1);

        int cumulativeChance = 0;

        // Iterate through each rarity to find where the random number falls
        foreach (var chance in DropChances) {
            cumulativeChance += chance.Value;

            if (randomNumber <= cumulativeChance) {

                // Pick a random item from the selected rarity list
                var itemsInRarityList = DropList[(int)chance.Key];

                // Select random item in that rarity class
                if (itemsInRarityList.Count > 0) return itemsInRarityList[random.Next(0, itemsInRarityList.Count)];
                

                // If no items of this rarity exist, continue to check other rarities
                break;
            }
        }

        // Empty array was returned
        return null;
    }
    
}