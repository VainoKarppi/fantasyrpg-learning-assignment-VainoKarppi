
using GUI;




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

    public static double CalculateDistance(Character start, Character end) {
        if (start == null || end is null) return -1;
        if (start.CurrentWorld != end.CurrentWorld) return -1;

        double deltaX = end.GetCenter().X - start.GetCenter().X;
        double deltaY = end.GetCenter().Y - start.GetCenter().Y;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
    public static double CalculateDistance(double x1, double y1, double x2, double y2) {
        double deltaX = x2 - x1;
        double deltaY = y2 - y1;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
    public static double CalculateDistance(int x1, int y1, int x2, int y2) {
        int deltaX = x2 - x1;
        int deltaY = y2 - y1;

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY); // Use double to return decimal results
    }

    

    // Method to add an NPC to the world
    public List<NpcCharacter> AddNPC(NpcCharacter npc) {
        NPCs.Add(npc);
        
        return NPCs;
    }

    public List<NpcCharacter> RemoveNPC(Character npc) {
        if (npc is not NpcCharacter) return NPCs;

        NPCs.Remove((NpcCharacter)npc);

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
