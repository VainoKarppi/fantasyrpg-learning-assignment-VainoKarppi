
public class GameEventListener {
    public GameEventListener() {
        GameInstance.OnPlayerWorldChanged += HandleWorldChanged;
        GameInstance.OnWorldCreated += HandleWorldCreated;

        NpcCharacter.OnNpcCreated += HandleNpcCreated;
        NpcCharacter.OnNpcKilled += HandleNpcKilled;

        PlayerActions.OnPlayerAttack += HandlePlayerAttack;
        PlayerActions.OnPlayerAction += HandlePlayerAction;
        PlayerActions.OnPlayerPotionUse += HandlePotionUse;

        Player.OnPlayerKilled += HandlePlayerKilled;
        Player.OnPlayerCreated += HandlePlayerCreated;

        BaseNpcActions.OnNpcAction += HandleNpcAction;
        BaseNpcActions.OnNpcAttack += HandleNpcAttack;

        Quests.OnQuestCreated += HandleQuestCreated;
        Quests.OnQuestUpdated += HandleQuestUpdated;
        Quests.OnQuestEnded += HandleQuestEnded;
    }


    private void HandleQuestCreated(IQuest quest) {
        Console.WriteLine($"Quest received! ({quest.Name}) - {quest.Description}");
    }
    private void HandleQuestUpdated(IQuest quest) {
        Console.WriteLine($"Quest status updated! ({quest.StageDescription})");
    }
    private void HandleQuestEnded(IQuest quest, bool succes) {
        Console.WriteLine($"Quest {quest.Name} ended!");

        if (succes) {
            Console.WriteLine("Quest rewards added to your inventory!");
            foreach (KeyValuePair <ItemBase, int> reward in quest.RewardList) {
                Console.WriteLine($"    - {reward.Key.Name} - (x{reward.Value})");
            }
        }
    }

    private void HandleWorldChanged(World oldworld, World newWorld) {

    }
    private void HandleWorldCreated(World world) {

    }


    private void HandlePlayerKilled(Player player, ICharacter? killer) {
        if (killer != null) {
            Console.WriteLine($"You were killed by: {killer.Name}");
        } else {
            Console.WriteLine("You died!");
        }
    }
    private void HandlePlayerCreated(Player player) {
    }


    private void HandleNpcCreated(NpcCharacter npc) {

    }
    private void HandleNpcAttack(Player player, NpcCharacter npc, int damage) {
        Console.WriteLine($"{npc.Name} hit you! You took {damage} damage!");
    }
    private void HandleNpcAction(NpcCharacter npc) {
        Console.WriteLine("Its your turn!");
    }
    private void HandleNpcKilled(NpcCharacter npc, Player? player) {
        if (player == null) return; // Check if it was killed by a player

        Console.WriteLine($"Enemy Killed!");

        ItemDrop? itemDrop = DropManager.GetRandomDrop();
        if (itemDrop is null) {
            Console.WriteLine("No drop!");
        } else {
            Console.WriteLine($"You got: {itemDrop.Name}");
            player.AddItem(itemDrop);
        }
    }




    private void HandlePlayerAttack(Player player, NpcCharacter npc, int damage) {
        Console.WriteLine($"You've hit {npc.Name} with {player.CurrentWeapon?.Name}: Damage dealt: {damage}");
    }
    private void HandlePlayerAction(Player player) {
    }


    private void HandlePotionUse(Player player, ItemPotion potion) {

        // Health potion
        if (potion is ItemPotion.HealthPotion) {
            if (player.Health == 100) throw new Exception("Already full health!");

            player.Health = Math.Min(player.Health + 20, 100);
            Console.WriteLine($"Health potion used! Health: {player.Health}");
        }


        // Mana potion
        if (potion is ItemPotion.ManaPotion) {
            if (player.Mana == 100) throw new Exception("Already full mana!");
        
            player.Mana = Math.Min(player.Mana + 20, 100);
            Console.WriteLine($"Mana potion used! Mana: {player.Mana}");   
        }

        player.InventoryItems.Remove(potion);
    }
}