
public class GameInstance {
    public static string[,]? WorldMap { get; set; }

    private static GameInstance? _instance;

    public static List<World> Worlds { get; private set; } = [];

    public static List<Player> Players { get; set; } = []; // In case of multiplayer support


    // Events
    public static event Action<World, World>? OnPlayerWorldChanged;
    public static event Action<World>? OnWorldCreated;
    

    public GameInstance(int x, int y) {
        if (_instance != null) throw new InvalidOperationException("Only one instance can be used!");

        WorldMap = new string[x, y];
        _instance = this;

        PlayerActions.OnPlayerAction += HandlePlayerAction;

        BaseNpcActions.OnNpcAction += HandleNpcAction;
    }

    private void HandlePlayerAction(Player player) {
        player.CurrentWorld.Tick++;

        World fightWorld = player.CurrentWorld;

        // Dont trigger enemy attack if no enemies left
        if (fightWorld.NPCs.Count() <= 0) return;

        Random random = new Random();

        int randomIndex = random.Next(fightWorld.NPCs.Count);
        NpcCharacter npc = fightWorld.NPCs[randomIndex];

        if (random.Next(100) < 50) {
            npc.NpcActions?.Attack(player);
        } else {
            npc.NpcActions?.Attack(player);
        }
    }

    private void HandleNpcAction(NpcCharacter npc) {
        npc.CurrentWorld.Tick++;
    }

    

    

    public static GameInstance GetInstance() {
        if (_instance == null) throw new InvalidOperationException("Instance not found!");

        return _instance;
    }

    public static void AddPlayerToInstance(Player player) {
        if (_instance == null) throw new InvalidOperationException("Instance not found!");

        Players.Add(player);
    }

    public static bool RemovePlayerFromInstance(Player player) {
        if (_instance == null) throw new InvalidOperationException("Instance not found!");

        int removed = Players.RemoveAll(x => x.Name == player.Name);
        return removed > 0;
    }
    

    public static void DisplayWorlds() {
        Console.WriteLine($"------WORLDS------");
        foreach (var world in Worlds) {
            Console.WriteLine($"{world.Name} NPCs:{world.NPCs.Count} Tick:{world.Tick}");
        }
        Console.WriteLine($"------------------");
    }

    public static World CreateWorld(string name) {
        World world = new World(name);

        Worlds.Add(world);
        
        OnWorldCreated?.Invoke(world);

        return world;
    }

    public static World GetWorld(string name) {
        return Worlds.First(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public void DeleteWorld(string name) {
        DeleteWorld(GetWorld(name));
    }

    public static void DeleteWorld(World world) {
        Worlds.Remove(world);
    }

    public static void ChangeWorld(Player player, World newWorld) {
        player.CurrentWorld = newWorld;

        OnPlayerWorldChanged?.Invoke(player.CurrentWorld, newWorld);
    }
}




public class World {
    public string Name { get; set; }
    public List<NpcCharacter> NPCs { get; private set; } = [];

    public int Tick { get; set; }

    public bool IsSafeWorld = false;

    internal World(string worldName) {
        Name = worldName;
    }


    // Method to add an NPC to the world
    public List<NpcCharacter> AddNPC(NpcCharacter npc) {
        NPCs.Add(npc);
        
        return NPCs;
    }

    public List<NpcCharacter> RemoveNPC(NpcCharacter npc) {
        NPCs.Remove(npc);

        return NPCs;
    }

    // Method to display the game state
    public virtual void DisplayWorldState() {
        Console.WriteLine($"World name: {Name}");
        Console.WriteLine($"Number of NPCs: {NPCs.Count}");
    }

    public virtual void DisplayWorldEnemies() {
        if (NPCs.Count > 0) {
            Console.WriteLine("-----Enemies-----");
            int i = 0;
            foreach (NpcCharacter enemy in NPCs) {
                Console.WriteLine($"[{i}] {enemy.Name}: Health: {enemy.Health}, Mana: {enemy.Mana}");
                i++;
            }
            Console.WriteLine("-----------------");
        }
    }

    public bool IsPlayerTurn() {
        return Tick % 2 == 0;
    }
}
