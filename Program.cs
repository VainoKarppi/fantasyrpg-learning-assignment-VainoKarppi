using System.Runtime.InteropServices;


public class Program {

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    public static void Main(string[] args) {
        bool useDebug = args.ContainsIgnoreCase("debug");

        if (useDebug) AllocConsole();
        Console.WriteLine("Debug Console Started!");

        // Create game instance
        new GameInstance();

        // Initialize Events
        new GameEventListener();

        // Create home world
        World homeWorld = GameInstance.CreateWorld("Home");
        homeWorld.IsSafeWorld = true;

        // Add buildings
        homeWorld.Buildings.Add(new World.Building("Shop", 300, 300, 100, 100, World.BuildingType.Shop));
        homeWorld.Buildings.Add(new World.Building("Quest 1", 40, 500, 80, 80, World.BuildingType.Quest));

       
        World forestWorld = GameInstance.CreateWorld("Forest");
        World caveWorld = GameInstance.CreateWorld("Cave");
        World castleWorld = GameInstance.CreateWorld("Castle");
        

        
        // NPCs in the cave world
        NpcCharacter.CreateNPC("warrior", caveWorld, World.FindSafeSpaceFromWorld(caveWorld));
        NpcCharacter.CreateNPC("mage", caveWorld, World.FindSafeSpaceFromWorld(caveWorld));
        NpcCharacter.CreateNPC("mage", caveWorld, World.FindSafeSpaceFromWorld(caveWorld));
        NpcCharacter.CreateNPC("mage", caveWorld, World.FindSafeSpaceFromWorld(caveWorld));

        // NPCs in the forest world
        NpcCharacter.CreateNPC("warrior", forestWorld, World.FindSafeSpaceFromWorld(forestWorld));
        NpcCharacter.CreateNPC("archer", forestWorld, World.FindSafeSpaceFromWorld(forestWorld));
        NpcCharacter.CreateNPC("archer", forestWorld, World.FindSafeSpaceFromWorld(forestWorld));
        NpcCharacter.CreateNPC("archer", forestWorld, World.FindSafeSpaceFromWorld(forestWorld));

        // NPCs in the castle world
        NpcCharacter.CreateNPC("warrior", castleWorld, World.FindSafeSpaceFromWorld(castleWorld));
        NpcCharacter.CreateNPC("warrior", castleWorld, World.FindSafeSpaceFromWorld(castleWorld));
        NpcCharacter.CreateNPC("warrior", castleWorld, World.FindSafeSpaceFromWorld(castleWorld));
        NpcCharacter.CreateNPC("archer", castleWorld, World.FindSafeSpaceFromWorld(castleWorld));

        // Initialize DropManager and it's loot
        new DropManager();

        // Initialize shop
        Shop shop = new Shop();

        Player player = new Player(homeWorld, "Player");
        player.Money = 1500;
        player.AddItem(new ItemDrop.Gems.Sapphire());


        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false); 
        Application.Run(new GUI.GameForm(player, shop));
    }
}
