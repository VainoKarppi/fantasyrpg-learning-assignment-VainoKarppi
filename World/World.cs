
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

        PlayerActions.OnPlayerAction += HandlePlayerAction;
    }

    private void HandlePlayerAction(Player player) {

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
        
        OnWorldCreated?.Invoke(world);

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

    public static void ChangeWorld(IWorldChanger unit, World newWorld) {
        unit.CurrentWorld = newWorld;

        OnPlayerWorldChanged?.Invoke(unit, unit.CurrentWorld, newWorld);
    }
}


public interface IWorldChanger {
    World CurrentWorld { get; set; }
}

public class World {
    public enum BuildingType {
        Block, // No use (just a building block)
        Shop,
        Storage,
        Quest
    }
    public class Building(string name, int xPos, int yPos, int width = 100, int height = 100, BuildingType type = BuildingType.Block) {
        public string Name { get; set; } = name;
        public int X { get; set; } = xPos;
        public int Y { get; set; } = yPos;
        public int Width { get; set;} = width;
        public int Height { get; set; } = height;
        public BuildingType BuildingType { get; set; } = type;
    }

    public string Name { get; set; }
    public List<NpcCharacter> NPCs { get; private set; } = [];

    public List<Building> Buildings { get; private set; } = [];

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



    public static (int, int) FindSafeSpaceFromWorld(World worldToSpawn, int minDistance = 100) {

        Random random = new Random();

        int tries = 0;
        while (true) {
            if (tries > 500) throw new InvalidOperationException("Unable to find safe location!");
            tries++;

            // Don't allow NPCs to spawn right next to borders
            int xSpawn = random.Next(100, GUI.GameForm.ScreenWidth - 100);
            int ySpawn = random.Next(100, GUI.GameForm.ScreenHeight - GUI.GameForm.StatsBarHeight - 100);

            if (worldToSpawn.NPCs.Count == 0) return (xSpawn, ySpawn);

            bool isSafe = true; // Assume the spot is safe initially

            // Check collision with existing NPCs
            foreach (NpcCharacter existingNpc in worldToSpawn.NPCs) {
                double distance = Math.Sqrt(Math.Pow(existingNpc.X - xSpawn, 2) + Math.Pow(existingNpc.Y - ySpawn, 2)); // Calculate distance to each other NPC

                // Check if distance is less than minimum distance, if so, it's not safe
                if (distance < minDistance) {
                    isSafe = false;
                    break; // No need to check further, break out of the loop
                }
            }

            // If no collision was found and it's safe, return the position
            if (isSafe) {
                return (xSpawn, ySpawn);
            }
        }
    }

}
