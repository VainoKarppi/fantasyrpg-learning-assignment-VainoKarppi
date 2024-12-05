
using GUI;

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
        Console.WriteLine($"Quest received! |{quest.Name}| - ({quest.Description}) for player: {quest.QuestOwner.Name}");
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

    private void HandleWorldChanged(IWorldChanger unit, World oldworld, World newWorld) {
        Console.WriteLine($"{unit} changed World: {newWorld.Name} from {oldworld.Name}");
    }
    private void HandleWorldCreated(World world) {
        Console.WriteLine($"Created world: {world.Name}");
    }


    private void HandlePlayerKilled(Player player, ICharacter? killer) {
        if (killer != null) {
            MessageBox.Show($"You were killed by: {killer.Name}.\n\nRespawn?", "You Died", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } else {
            MessageBox.Show($"You Died!\n\nRespawn?");
        }

        GUI.GameForm.RefreshPage();
    }
    private void HandlePlayerCreated(Player player) {
        Console.WriteLine($"Created Player: {player.Name}");
    }


    private void HandleNpcCreated(NpcCharacter npc) {
        Console.WriteLine($"Created NPC: {npc.Name} to World: {npc.CurrentWorld.Name}");
    }
    private void HandleNpcAttack(Player player, NpcCharacter npc, int damage) {
        Console.WriteLine($"{npc.Name} hit you! You took {damage} damage!");
        GameForm.RefreshPage(); // update player health
    }
    private void HandleNpcAction(NpcCharacter npc) {
        
    }
    private void HandleNpcKilled(NpcCharacter npc, Player? player) {
        if (player == null) return; // Check if it was killed by a player

        Console.WriteLine($"Enemy Killed!");


        // TODO move elsewhere?
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
            if (player.Health == 100) {
                MessageBox.Show("Already full health!");
                return;
            }

            player.Health = Math.Min(player.Health + potion.Effect, 100);
            Console.WriteLine($"Health potion used! Health: {player.Health}");
        }


        // Mana potion
        if (potion is ItemPotion.ManaPotion) {
            if (player.Mana == 100) {
                MessageBox.Show("Already full mana!");
                return;
            }
        
            player.Mana = Math.Min(player.Mana + potion.Effect, 100);
            Console.WriteLine($"Mana potion used! Mana: {player.Mana}");   
        }

        player.InventoryItems.Remove(potion);
    }
}