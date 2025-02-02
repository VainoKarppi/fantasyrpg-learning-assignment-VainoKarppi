using GUI;



public class GameInstance {

    private static GameInstance? _instance;

    public static List<World> Worlds { get; private set; } = [];

    public static List<Player> Players { get; set; } = []; // In case of multiplayer support


    // Events
    public static event Action<IWorldChanger, World, World>? OnPlayerWorldChanged;
    public static event Action<World>? OnWorldCreated;

    public GameInstance() {
        if (_instance != null) throw new InvalidOperationException("Only one instance can be used!");

        _instance = this;

        PlayerActions.OnPlayerAttack += HandlePlayerAttack;
    }

    private void HandlePlayerAttack(Player player, Character npc, int damage) {

        // Dont trigger enemy attack if no enemies left
        if (player.CurrentWorld.NPCs.Count <= 0) return;

        
        // Attack player
        if (npc == null || npc is not NpcCharacter || npc.Health <= 0) return;

        Task.Run(async () => {
            await Task.Delay(((NpcCharacter)npc).AttackTime);
            if (npc.CanAttack(player)) npc?.Actions.Attack(player);
        });
    }


    
    public static void RemoveAllNpcs() {
        foreach (World world in Worlds) {
            world.NPCs.Clear();
        }
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

        bool removed = Players.Remove(player);
        return removed;
    }
    

    public static World CreateWorld(string name) {
        World world = new World(name);

        Worlds.Add(world);
        
        OnWorldCreated?.InvokeFireAndForget(world);

        return world;
    }

    public static World GetWorld(string name) {
        return Worlds.First(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public static void DeleteWorld(string name) {
        DeleteWorld(GetWorld(name));
    }

    public static void DeleteWorld(World world) {
        Worlds.Remove(world);
    }

    public static void ChangeWorld(Character unit, World newWorld) {
        World oldWorld = unit.CurrentWorld;
        unit.CurrentWorld = newWorld;

        OnPlayerWorldChanged?.InvokeFireAndForget(unit, oldWorld, newWorld);
    }

    public static int GetRandomID() {
        return new Random().Next(0, int.MaxValue);
    }
}