





public class Player : Character, IWorldChanger {

    public static Color Color => Color.Blue;
    public Rectangle Bounds => new Rectangle(X, Y, Width, Height);




    public int? Money { get; set; } = 0;

    // Items in Backpack
    public List<ItemBase> InventoryItems { get; set; } = [];


    public List<IQuest> QuestList { get; set; } = [];
    public IQuest? CurrentQuest { get; set; }
    public List<string> CompletedQuests { get; set; } = [];

    //--- EVENTS
    public static event Action<Player, Character?>? OnPlayerKilled;
    public static event Action<Player>? OnPlayerCreated;
    public static event Action<Player>? OnPlayerRespawn;


    public Statistics PlayerStatistics { get; private set; } = new Statistics();
    public class Statistics {
        public int EnemiesKilled { get; set; }
        public int DeathsCount { get; set; }
        public double DamageDealt { get; set; }
        public double DamageTaken { get; set; } // TODO

        // TODO what else to add to statistics ???
    }
    

    public Player(World startWorld, string playerName = "Player") {
        
        CurrentWorld = startWorld;
        Name = playerName;
        Actions = new PlayerActions(this);
        GameInstance.AddPlayerToInstance(this);

        CurrentWeapon = new MeleeWeapon.Fists();

        OnPlayerCreated?.InvokeFireAndForget(this);
    }

    public override void Kill(Character? killer = null) {
        PlayerStatistics.DeathsCount++;

        OnPlayerKilled?.InvokeFireAndForget(this, killer);
        
        ResetPlayer();
    }

    private void ResetPlayer() {
        CurrentArmor = null;
        CurrentWeapon = new MeleeWeapon.Fists();
        InventoryItems.Clear();
        Health = 100;

        World homeWorld = GameInstance.Worlds.First(x => x.IsSafeWorld);
        ChangeWorld(homeWorld);

        X = GUI.GameForm.ScreenWidth / 2;
        Y = GUI.GameForm.ScreenHeight / 2;
        GUI.Effect.Effects.Clear();

        OnPlayerRespawn?.InvokeFireAndForget(this); 
    }

    public void ChangeWorld(string worldName) {
        World world = GameInstance.GetWorld(worldName);
        ChangeWorld(world);
    }

    public void ChangeWorld(World newWorld) {
        if (newWorld == null || CurrentWorld == null || !GameInstance.Worlds.Contains(newWorld)) return;

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