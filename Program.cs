﻿
GameInstance instance = new GameInstance(2,2);

// Initialize worlds
World homeWorld = GameInstance.CreateWorld("Home");
homeWorld.IsSafeWorld = true;

World forestWorld = GameInstance.CreateWorld("Forest");
World castleWorld = GameInstance.CreateWorld("Castle");
World caveWorld = GameInstance.CreateWorld("Cave");



// Add some NPCs to the game worlds
NpcCharacter.CreateNPC("warrior", forestWorld);
NpcCharacter.CreateNPC("archer", forestWorld);
NpcCharacter.CreateNPC("archer", forestWorld);
NpcCharacter.CreateNPC("archer", forestWorld);


NpcCharacter.CreateNPC("warrior", castleWorld);
NpcCharacter.CreateNPC("warrior", castleWorld);
NpcCharacter.CreateNPC("warrior", castleWorld);
NpcCharacter.CreateNPC("archer", castleWorld);

NpcCharacter.CreateNPC("warrior", caveWorld);
NpcCharacter.CreateNPC("mage", caveWorld);
NpcCharacter.CreateNPC("mage", caveWorld);
NpcCharacter.CreateNPC("mage", caveWorld);


// Initialize Shop
Shop shop = new Shop();

// Initialize DropManager
DropManager dropManager = new DropManager();


// Initialize Events
GameEventListener listener = new GameEventListener();

Console.WriteLine("Welcome to RPG game. Please choose a name:");
string playerName;

while (true) {
    playerName = Console.ReadLine()!;

    if (!string.IsNullOrEmpty(playerName)) break;
    Console.WriteLine("Invalid name. Try again!");
}


Player player = new Player(homeWorld, playerName);
player.Money = 600;


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


Task ReadKeyInput(CancellationToken cancellationToken) {
    while (!cancellationToken.IsCancellationRequested && !player.CurrentWorld.IsSafeWorld) {
        var keyInfo = Console.ReadKey(intercept: true);
        if (keyInfo.Key == ConsoleKey.A) {
            // Attack
        }

        if (keyInfo.Key == ConsoleKey.D) {
            // Defend
        }

        if (keyInfo.Key == ConsoleKey.UpArrow) {
            // Attack
        }
    }

    return Task.CompletedTask;
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

// test comment