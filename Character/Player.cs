
public static class IDGenerator {
    private static int idIndex = 0;

    public static int GenerateId() {
        return ++idIndex;
    }
}

public interface ICharacter {
    int ID { get; }
    string? Name { get; }
    int Health { get; }
    int Mana { get; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; }
    public int Height { get; }
    public Color Color => Color.Red;
    public Rectangle Bounds => new Rectangle(X, Y, Width, Height);
}


public class Player : ICharacter, IWorldChanger {

    public int X { get; set; } = GUI.GameForm.ScreenWidth / 2;
    public int Y { get; set; } = GUI.GameForm.ScreenHeight / 2;
    public int Width { get; set; } = 20;
    public int Height { get; set; } = 20;
    public Color Color => Color.Blue;
    public Rectangle Bounds => new Rectangle(X, Y, Width, Height);



    public string Name { get; set; }
    public World CurrentWorld { get; set; }

    public int ID { get; } = IDGenerator.GenerateId();
    

    public int Health { get; set; } = 100;
    public int Mana { get; set; } = 100;

    public ItemArmor? CurrentArmor { get; set; }
    public ItemWeapon CurrentWeapon { get; set; }

    public int? Money { get; set; } = 0;

    // Items in Backpack
    public List<ItemBase> InventoryItems { get; set; } = [];

    public IPlayerActions PlayerActions{ get; set; }

    public List<IQuest> QuestList { get; set; } = [];
    public IQuest? CurrentQuest { get; set; }

    //--- EVENTS
    public static event Action<Player, ICharacter?>? OnPlayerKilled;
    public static event Action<Player>? OnPlayerCreated;


    public Statistics PlayerStatistics { get; private set; } = new Statistics();
    public class Statistics {
        public int EnemiesKilled { get; set; } // TODO
        public int DeathsCount { get; set; } // TODO
        public double DamageDealt { get; set; } // TODO
        public double DamageTaken { get; set; } // TODO

        // TODO what else to add to statistics ???
    }


    public Player(World startWorld, string playerName = "Player") {
        
        CurrentWorld = startWorld;
        Name = playerName;
        PlayerActions = new PlayerActions(this);
        GameInstance.AddPlayerToInstance(this);

        CurrentWeapon = new MeleeWeapon.Fists();

        OnPlayerCreated?.Invoke(this);
    }

    
    public void KillPlayer(ICharacter? killer = null) {
        PlayerStatistics.DeathsCount++;

        OnPlayerKilled?.Invoke(this, killer);
        
        ResetPlayer();
    }

    private void ResetPlayer() {
        CurrentArmor = null;
        CurrentWeapon = new MeleeWeapon.Fists();
        InventoryItems.Clear();
        Health = 100;

        X = GUI.GameForm.ScreenWidth / 2;
        Y = GUI.GameForm.ScreenHeight / 2; 

        World homeWorld = GameInstance.Worlds.First(x => x.IsSafeWorld);
        ChangeWorld(homeWorld);
    }

    public void ChangeWorld(World newWorld) {
        if (newWorld == null || !GameInstance.Worlds.Contains(newWorld)) return;

        // Get the indices of the current and new worlds
        int currentIndex = GameInstance.Worlds.IndexOf(CurrentWorld);
        int newIndex = GameInstance.Worlds.IndexOf(newWorld);

        // Moving from left to right
        if (newIndex > currentIndex) {
            X = Width;
        } else {
            X = GUI.GameForm.ScreenWidth - (Width + 40);
        }
        

        if (currentIndex == -1 || newIndex == -1) return;
        
        // Update the player's current world
        GameInstance.ChangeWorld(this, newWorld);
    }

    public void AddItem(dynamic item) {
        if (CurrentArmor is null && item is ItemArmor) {
            CurrentArmor = item;
            return;
        }

        if (CurrentWeapon is null && item is ItemWeapon) {
            CurrentWeapon = item;
            return;
        }

        InventoryItems.Add(item);
    }

    public bool RemoveItem(dynamic item) {
        return InventoryItems.Remove(item);
    }

    public virtual void DebugDisplayInventory() {
        Console.WriteLine("-------------------");
        Console.WriteLine($"({InventoryItems.Count}) Items:");

        foreach (object item in InventoryItems) {
            var nameProperty = item.GetType().GetProperty("Name")?.GetValue(item);
            if (nameProperty != null) Console.Write($"    {nameProperty}");

            var damageProperty = item.GetType().GetProperty("Damage")?.GetValue(item);
            if (damageProperty != null) Console.Write($": Damage: {damageProperty}");

            var defenceProperty = item.GetType().GetProperty("Defence")?.GetValue(item);
            if (defenceProperty != null) Console.Write($", Defence: {defenceProperty}");

            var durabilityProperty = item.GetType().GetProperty("Durability")?.GetValue(item);
            if (durabilityProperty != null) Console.Write($", Durability: {durabilityProperty}");

            var priceProperty = item.GetType().GetProperty("Price")?.GetValue(item);
            if (priceProperty != null) Console.Write($", Price: {priceProperty}");
            Console.Write("\n");
        }
        Console.WriteLine("\n-------------------");
    }

    public void ChangeWeapon(ItemWeapon newWeapon) {
        if (CurrentWeapon == null) {
            CurrentWeapon = newWeapon;
            return;
        }
 
        // Move old weapon to inventory and select new weapon
        if (CurrentWeapon != null) InventoryItems.Add(CurrentWeapon);
        
        try {
            InventoryItems.Remove(newWeapon);
        } catch (Exception) {}
        
        CurrentWeapon = newWeapon;
    }

    public void ChangeArmor(ItemArmor newArmor) {
        if (CurrentArmor == null) {
            CurrentArmor = newArmor;
            return;
        }

        // Return equiped armor to inventory
        if (CurrentArmor != null) InventoryItems.Add(CurrentArmor);

        try {
            InventoryItems.Remove(newArmor);
        } catch (Exception) {}
        
        CurrentArmor = newArmor;
    }


    public void AddQuest(IQuest quest) {
        if (QuestList.Contains(quest)) throw new Exception("Quest already active!");

        if (CurrentQuest == null) CurrentQuest = quest;
        
        QuestList.Add(quest);
    }

    public bool RemoveQuest(IQuest quest) {
        bool removed = QuestList.Remove(quest);

        if (removed) CurrentQuest = null;

        return removed;
    }

    public List<T> GetInventoryItems<T>() where T : class {
        return InventoryItems
            .Where(item => item is T)
            .Cast<T>()
            .ToList();
    }

    public List<ItemPotion> GetInventoryPotions() {
        return GetInventoryItems<ItemPotion>();
    }

    public List<ItemWeapon> GetInventoryWeapons() {
        return GetInventoryItems<ItemWeapon>();
    }

    public List<ItemArmor> GetInventoryArmors() {
        return GetInventoryItems<ItemArmor>();
    }
}