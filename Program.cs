using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace GUI;

public partial class GameForm : Form {

    private Player player;

    private const int MoveStep = 5;

    public const int ScreenWidth = 1000;
    public const int ScreenHeight = 700 + StatsBarHeight;
    private const int StatsBarHeight = 200;

    public GameForm() {
        SuspendLayout();
        ClientSize = new Size(ScreenWidth, ScreenHeight);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Text = "Game Window";
        ResumeLayout(false);

        Width = ScreenWidth;
        Height = ScreenHeight;

        // Initialize the player
        player = new Player(GameInstance.GetWorld("Home"), "test name"); // TODO name
        player.Money = 800;
        player.AddItem(new MetalArmor());
        player.AddItem(new MetalArmor());

        player.AddQuest(new Quests.Massacare(player));


        Paint += GameForm_Paint;
        KeyDown += GameForm_KeyDown;
        KeyUp += GameForm_KeyUp;
        DoubleBuffered = true; // To avoid flickering
    }

    private HashSet<Keys> pressedKeys = new HashSet<Keys>();

    // Handle player movement based on keypress
    private void GameForm_KeyDown(object? sender, KeyEventArgs e) {
        pressedKeys.Add(e.KeyCode);


        ProcessMovement();

        // Check for world boundary collision
        CheckWorldBounds();

        // Check for collisions with NPCs
        CheckCollisions();

        // Redraw the screen
        Invalidate();
    }

    private void GameForm_KeyUp(object? sender, KeyEventArgs e) {
        // Remove the key from the pressedKeys set
        pressedKeys.Remove(e.KeyCode);
    }

    private void ProcessMovement() {
        if (pressedKeys.Contains(Keys.W)) {
            player.Y -= MoveStep;
        }
        if (pressedKeys.Contains(Keys.A)) {
            player.X -= MoveStep;
        }
        if (pressedKeys.Contains(Keys.S)) {
            player.Y += MoveStep;
        }
        if (pressedKeys.Contains(Keys.D)) {
            player.X += MoveStep;
        }
    }

    // Ensure the player does not move outside world boundaries
    private void CheckWorldBounds() {
        int worldWidth = ScreenWidth;
        int worldHeight = ScreenHeight - StatsBarHeight;

        // From Left to Right
        if (player.X + player.Width >= ScreenWidth - 20) {
            int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);

            if (currentWorldIndex + 1 < GameInstance.Worlds.Count) {
                player.ChangeWorld(GameInstance.Worlds[currentWorldIndex + 1]);
            }
        }

        // From Right to Left
        if (player.X + player.Width <= 20) {
            int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);

            if (currentWorldIndex - 1 >= 0) {
                player.ChangeWorld(GameInstance.Worlds[currentWorldIndex - 1]);
            }
        }

        // Clamp player's position within world boundaries
        player.X = Math.Clamp(player.X, 0, worldWidth - player.Width * 2);
        player.Y = Math.Clamp(player.Y, 40, worldHeight - player.Height * 3); // Add some extra at the bottom, for stats display
    }

    // Check if player collides with any NPCs
    private void CheckCollisions() {
        foreach (NpcCharacter npc in player.CurrentWorld.NPCs) {
            if (player.Bounds.IntersectsWith(npc.Bounds)) {
                // Show dialog if collision detected
                pressedKeys.Clear();
                ShowNpcDialog();
                break;
            }
        }

        // Check collisions with Buildings
        foreach (World.Building building in player.CurrentWorld.Buildings) {
            if (player.Bounds.IntersectsWith(new Rectangle(building.X, building.Y, building.Width, building.Height))) {

                // Show dialog or take action if collision with a building is detected
                pressedKeys.Clear();

                if (building.BuildingType == World.BuildingType.Shop) OpenShopBuy();
                
                break;
            }
        }
    }

    



    // Show the dialog to ask whether to attack or flee
    private void ShowNpcDialog() {
        var result = MessageBox.Show("You have encountered an NPC! Do you want to attack or flee?", 
                                     "NPC Encounter", 
                                     MessageBoxButtons.YesNo);

        // Yes means attack, No means flee
        if (result == DialogResult.Yes) {
            Attack();
        } else {
            Flee();
        }
    }

    private void ShowShopDialog() {
        var result = MessageBox.Show("You have entered shop", 
                                     "NPC Encounter", 
                                     MessageBoxButtons.YesNo);

        // Yes means attack, No means flee
        if (result == DialogResult.Yes) {
            Attack();
        } else {
            Flee();
        }
    }

    // Handle the attack action
    private void Attack() {
        MessageBox.Show("You chose to attack the NPC!");
        // You can add attack logic here, like reducing NPC health, etc.
    }

    // Handle the flee action
    private void Flee() {
        MessageBox.Show("You chose to flee from the NPC!");
        // Flee logic: Move player away from the NPC
        player.X += 50; // Example flee action: Move the player to the right
        player.Y += 50; // Move player downward
    }


    

    private void GameForm_Paint(object? sender, PaintEventArgs e) {
        var g = e.Graphics;

        // Draw current world name at the center at the top
        string worldName = player.CurrentWorld.Name;
        using (Font font = new Font("Arial", 16, FontStyle.Bold)) // You can adjust font size and style
        using (Brush brush = new SolidBrush(Color.Black)) { // Grey color for the text
            // Calculate position to center the text at the top
            SizeF textSize = g.MeasureString(worldName, font);
            float textX = (Width - textSize.Width) / 2; // Center horizontally
            float textY = 10; // 10 pixels from the top

            g.DrawString(worldName, font, brush, textX, textY);
        }


        // Get Left and Right worlds (check if exists)
        int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);

        World? nextWorld = null;
        World? previousWorld = null;

        // Draw right side world enter text
        if (currentWorldIndex + 1 < GameInstance.Worlds.Count) {
            nextWorld = GameInstance.Worlds[currentWorldIndex + 1];
        }

        // Draw left side world enter text
        if (currentWorldIndex - 1 >= 0) {
            previousWorld = GameInstance.Worlds[currentWorldIndex - 1];
        }

        if (nextWorld != null) DrawRightArrow(g, $"Enter world\n    {nextWorld.Name}");
        if (previousWorld != null) DrawLeftArrow(g, $"Enter world\n    {previousWorld.Name}");


        // Draw player
        g.FillRectangle(new SolidBrush(player.Color), player.X, player.Y, player.Width, player.Height);

        DrawNpcs(g, player);

        DrawBuildings(g, player.CurrentWorld);
        DrawStatsBar(g);
        DrawQuest(g);
        DrawInventory(g);
    }

    public static Shop? Shop;    

    // Main method to run the game form
    public static void Main() {
        // Create game instance
        new GameInstance();

        // Create home world
        World homeWorld = GameInstance.CreateWorld("Home");
        homeWorld.IsSafeWorld = true;

        homeWorld.Buildings.Add(new World.Building("Shop", 300, 300, 100, 100, World.BuildingType.Shop));

        World forestWorld = GameInstance.CreateWorld("Forest");
        World castleWorld = GameInstance.CreateWorld("Castle");
        World caveWorld = GameInstance.CreateWorld("Cave");

        // Add some NPCs to the game worlds
        NpcCharacter.CreateNPC("warrior", forestWorld, 200, 150);
        NpcCharacter.CreateNPC("archer", forestWorld, 300, 250);
        NpcCharacter.CreateNPC("archer", forestWorld, 400, 50);
        NpcCharacter.CreateNPC("archer", forestWorld, 100, 300);


        NpcCharacter.CreateNPC("warrior", castleWorld, 200, 100);
        NpcCharacter.CreateNPC("warrior", castleWorld, 100, 50);
        NpcCharacter.CreateNPC("warrior", castleWorld, 300, 250);
        NpcCharacter.CreateNPC("archer", castleWorld, 400, 350);

        NpcCharacter.CreateNPC("warrior", caveWorld, 100, 300);
        NpcCharacter.CreateNPC("mage", caveWorld, 300, 150);
        NpcCharacter.CreateNPC("mage", caveWorld, 50, 400);
        NpcCharacter.CreateNPC("mage", caveWorld, 200, 150);

        // Initialize Shop
        Shop = new Shop();

        // Initialize DropManager
        new DropManager();


        // Initialize Events
        new GameEventListener();


        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new GameForm());
    }
}




/*
Console.WriteLine("Welcome to RPG game. Please choose a name:");
string playerName;

while (true) {
    playerName = Console.ReadLine()!;

    if (!string.IsNullOrEmpty(playerName)) break;
    Console.WriteLine("Invalid name. Try again!");
}





// Add quest to test
player.AddQuest(new Quests.Massacare(player));


Console.WriteLine($"Welcome {player}. Type 'Help' for commands, and for intructions for how to play.");
Console.WriteLine("You can access SHOP by typing: 'shop'. This can be only accessed in Home town.");

List<string> Commands = ["Quit","Help","Shop","Inventory","Stats","World","Worlds","ChangeWorld","Potion", "Attack", "ChangeWeapon","Quests","Quest","ChangeQuest"];

while (true) {
    try {
        Console.WriteLine();
        Console.Write($"{player.CurrentWorld.Name} > ");
        string? command = Console.ReadLine()?.ToLower();

        // Make sure command exists
        if (string.IsNullOrEmpty(command) || !Commands.Any(c => c.Equals(command, StringComparison.CurrentCultureIgnoreCase))) {
            Console.WriteLine("Invalid command. Try again!");
            continue;
        }

        if (command == "quit") return 0;

        if (command == "help") Showhelp();
        
        if (command == "shop") OpenShop();

        if (command == "stats") player.DisplayStats();
        if (command == "inventory") player.DisplayInventory();

        if (command == "world") player.CurrentWorld.DisplayWorldState();
        if (command == "worlds") GameInstance.DisplayWorlds();
        if (command == "changeworld") ChangeWorld();

        if (command == "potion") UsePotion();

        if (command == "changeweapon") ChangeWeapon();

        if (command == "attack") Attack();

        if (command == "quest") DiplayQuestInfo();
        if (command == "changequest") ChangeActiveQuest();
        if (command == "quests") DiplayQuests();

    } catch (Exception ex) {
        Console.WriteLine(ex);
        Console.WriteLine(ex.Message);
    }
}





void Showhelp() {
    Console.WriteLine("---------------------HELP---------------------");
    Console.WriteLine("Instructions:");
    Console.WriteLine("Your job is to Clean all worlds from enemies. Make sure to gear up well before fighting. Good Luck!");
    Console.WriteLine("\nCommands:");
    string commandsText = "    ";
    foreach (var command in Commands) {
        commandsText += command + ", ";
    }
    commandsText = commandsText.Substring(0, commandsText.Length - 2);
    Console.WriteLine($"{commandsText}"); 
    Console.WriteLine("----------------------------------------------");
}


void ChangeActiveQuest() {
    if (player.QuestList.Count() == 0) throw new Exception("No quests found!");

    if (player.QuestList.Count() == 1 && player.CurrentQuest == player.QuestList[0])
        throw new Exception("You dont have any more quests other than the one that is active!");


    IQuest? selectedQuest;

    // Get selected quest from list
    DiplayQuests();
    while (true) {
        Console.WriteLine("\nSelect Active quest or 'exit'");
        Console.Write($"{player.CurrentWorld.Name} (Quests) > ");
        string mode = Console.ReadLine()!;
        if (mode.ToLower() == "exit") return;
        mode = mode.ToLower();

        try {
            selectedQuest = player.QuestList[int.Parse(mode)];
            if (selectedQuest != null) break;
        } catch (Exception) {}

        Console.WriteLine("Invalid command. Try again!");
    }

    // Update Active quest
    player.CurrentQuest = selectedQuest;
}

void DiplayQuests() {
    Console.WriteLine("---------------------QUESTS---------------------");
    int i = 0;
    foreach (IQuest quest in player.QuestList) {
        Console.WriteLine($"[{i}]    {quest.Name} - {quest.Description}");
        i++;
    }
    Console.WriteLine("------------------------------------------------");
}

void DiplayQuestInfo() {
    if (player.CurrentQuest is null) throw new Exception("No quest is active!");

    Console.WriteLine("-----------------CURRENT QUESTS-----------------");
    Console.WriteLine($"Name: {player.CurrentQuest.Name} - {player.CurrentQuest.Description}");
    if (player.CurrentQuest.StageDescription != null) {
        Console.WriteLine($"    {player.CurrentQuest.StageDescription}");
    }
    Console.WriteLine("------------------------------------------------");
}

void OpenShop() {
    if (!player.CurrentWorld.IsSafeWorld) throw new Exception("You must be in home world to perfor mthis operation!");

    while (true) {
        // Select Shop mode
        string? mode;
        while (true) {
            Console.WriteLine("\nSelect 'Buy', 'Sell' or 'exit'");
            Console.Write($"{player.CurrentWorld.Name} (Shop) > ");
            mode = Console.ReadLine();
            if (string.IsNullOrEmpty(mode) || mode.ToLower() == "exit") return;
            mode = mode.ToLower();

            if (mode == "buy" || mode == "sell") break;
            Console.WriteLine("Invalid command. Try again!");
        }


        // SELL
        if (mode == "sell") {
            
            Dictionary<ISellable, int> sellableItems = Shop.GetSellableItems(player);

            if (sellableItems.Count == 0) {
                Console.WriteLine("No items to sell!");
                continue;
            }

            shop.DisplaySellableItems(sellableItems);

            Console.WriteLine("To sell an item, type the item number. To exit Shop type 'Exit'");
            while (true) {
                try {
                    Console.Write($"{player.CurrentWorld} (Shop : Sell) > ");
                    string indexToSell = Console.ReadLine()!;
                    if (indexToSell.ToLower() == "exit") break;

                    int itemIndex = int.Parse(indexToSell);

                    
                    ISellable itemToSell = sellableItems.Keys.ElementAt(itemIndex);

                    Shop.SellItem(player, itemToSell);
                    Console.WriteLine($"Sell succesfull! You gained: {itemToSell.SellPrice}. Money: {player.Money}");

                    break;
                } catch (Exception) {
                    Console.WriteLine("Invalid item. Try again!");
                }
            }
            continue;
        }

        // BUY
        shop.DisplayShopInventory();

        Console.WriteLine("\nTo buy item, type the item number. To exit Shop type 'Exit'");
        Console.WriteLine($"Money: {player.Money}");
        while (true) {
            try {
                Console.Write($"{player.CurrentWorld} (Shop : Buy) > ");
                string numberToBuy = Console.ReadLine()!;
                if (numberToBuy.ToLower() == "exit") break;

                int itemIndex = int.Parse(numberToBuy);
                IBuyable itemToBuy = shop.ItemsForSale[itemIndex];

                if (player.Money < itemToBuy.BuyPrice) throw new NotEnoughMoneyException("Not enough money!");
                Shop.BuyItem(player, itemToBuy);

                Console.WriteLine($"You bought: {itemToBuy.Name} for {itemToBuy.BuyPrice} Coins. Money: {player.Money}");
                break;
            } catch (Exception ex) {
                if (ex is NotEnoughMoneyException) {
                    Console.WriteLine(ex.Message);
                    continue;
                }
                Console.WriteLine("Invalid item. Try again!"); 
            }
        }
    }
}

void ChangeWorld() {
    Console.WriteLine("Enter new world name:");
    string newWorldName = Console.ReadLine()!;

    try {  
        World newWorld = GameInstance.GetWorld(newWorldName);
        GameInstance.ChangeWorld(player, newWorld);


        Console.WriteLine($"Change world to: {newWorld.Name}");
        newWorld.DisplayWorldEnemies();
    } catch (Exception) {
        Console.WriteLine("Unable to change world");
    }
}


void UsePotion() {
    
    Dictionary<ItemPotion, int> potionCounts = [];

    foreach (object item in player.InventoryItems) {
        if (item is not ItemPotion) continue; // Skip everything else other than potions
        
        potionCounts[(ItemPotion)item] = potionCounts.ContainsKey((ItemPotion)item) ? potionCounts[(ItemPotion)item] + 1 : 1;
    }

    if (potionCounts.Count > 0) {
        Console.WriteLine("-----Potions-----");

        int i = 0;
        foreach (KeyValuePair<ItemPotion, int> item in potionCounts) {
            Console.WriteLine($"[{i}] {item.Key.Name} ({item.Value})");
            i++;
        }
        Console.WriteLine("-----------------");

        Console.WriteLine("Select potion to use. Or use 'Exit'");
        string? indexText = Console.ReadLine();
        if (string.IsNullOrEmpty(indexText) || indexText.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) return;
        
        int index = int.Parse(indexText);
        ItemPotion selectedPotion = potionCounts.Keys.ElementAt(index);

        player.PlayerActions.UsePotion(selectedPotion);
    } else {
        Console.WriteLine("No potions in inventory!");
    }
}


void Attack() {
    if (player.CurrentWorld.IsSafeWorld) throw new Exception("You cannot be in home world while performing this operation!");
    if (!player.CurrentWorld.IsPlayerTurn()) throw new Exception("It's not your turn yet!");
    if (player.CurrentWorld.NPCs.Count <= 0) throw new Exception("No enemies left!");


    Console.WriteLine("Select target to attack, or use 'exit'");

    string? input = Console.ReadLine();
    if (string.IsNullOrEmpty(input)) return;
    input = input.Trim().ToLower();

    if (input == "exit") {
        Console.WriteLine("You have left the fight, back to Home.");
        player.CurrentWorld = homeWorld;
        return;
    }

    int index = int.Parse(input);
    NpcCharacter target = player.CurrentWorld.NPCs[index];

    player.PlayerActions.Attack(target);

    player.CurrentWorld.DisplayWorldEnemies();
}


void ChangeWeapon() {
    Dictionary<ItemWeapon, int> Weapons = [];

    foreach (object item in player.InventoryItems) {
        if (item is not ItemWeapon) continue; // Skip everything else other than Weapons
        
        Weapons[(ItemWeapon)item] = Weapons.ContainsKey((ItemWeapon)item) ? Weapons[(ItemWeapon)item] + 1 : 1;
    }

    if (Weapons.Count > 0) {
        Console.WriteLine("Select new weapon to use or type 'Exit'");
        Console.WriteLine("-----Weapons-----");

        int i = 0;
        foreach (KeyValuePair<ItemWeapon, int> item in Weapons) {
            Console.WriteLine($"[{i}] {item.Key.Name} ({item.Value})");
            i++;
        }
        Console.WriteLine("-----------------");

        Console.WriteLine("Select weapon to use. Or use 'Exit'");
        string? indexText = Console.ReadLine();
        if (string.IsNullOrEmpty(indexText) || indexText.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) return;
        
        int index = int.Parse(indexText);
        ItemWeapon selectedPotion = Weapons.Keys.ElementAt(index);

        player.ChangeWeapon(selectedPotion);

        Console.WriteLine($"Current Weapon changed to: {selectedPotion.Name}");
    } else {
        Console.WriteLine("No Weapons in invetory!");
    }

    
}
*/
// test comment

