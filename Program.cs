using System.Runtime.InteropServices;


public class Program {

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    public static Config GameConfig { get; private set; }


    public static void Main(string[] args) {
        try {

            
            bool useDebug = args.ContainsIgnoreCase("debug");

            if (useDebug) AllocConsole();
            Console.WriteLine("Debug Console Started!");

            // Load configuration
            Config.LoadConfig();

            // Check if database exists
            Database.Initialize();

            // Create game instance
            new GameInstance();

            // Initialize Events
            new GameEventListener();

            // Add worlds from Config.json
            foreach (var world in Config.Instance.Worlds) {
                
                World createdWorld = GameInstance.CreateWorld(world.Name);

                // Mark as safe if no enemies
                if (world.Enemies.Count == 0) createdWorld.IsSafeWorld = true;

                // Add buildings
                foreach (var building in world.Buildings) {
                    createdWorld.Buildings.Add(new World.Building(
                        building.Name,
                        building.Position.X,
                        building.Position.Y,
                        building.Size.Width,
                        building.Size.Height,
                        Enum.Parse<World.BuildingType>(building.Type, true)
                    ));
                }
            }

            
            
            // Restore npc from database
            Database.RestoreNpcs();

            // Initialize NPC factory (AFTER restoring NPCs)
            NpcFactory.Start();

            // Initialize DropManager and it's loot
            new DropManager();

            // Initialize shop
            Shop shop = new Shop();

            


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); 
            Application.Run(new GUI.GameForm(shop));
        } catch (Exception ex) {
            Console.WriteLine(ex);
            Console.ReadLine();
        }
    }
}
