using System.Reflection.Metadata;

public class GameEventListener {
    public GameEventListener() {
        GameInstance.OnPlayerWorldChanged += HandleWorldChanged;
        GameInstance.OnWorldCreated += HandleWorldCreated;

        NpcCharacter.OnNpcCreated += HandleNpcCreated;
        NpcCharacter.OnNpcKilled += HandleNpcKilled;

        PlayerActions.OnPlayerAttack += HandlePlayerAttack;
        PlayerActions.OnPlayerAction += HandlePlayerAction;
        PlayerActions.OnPlayerPotionUse += HandlePotionUse;

        BaseNpcActions.OnNpcAction += HandleNpcAction;
        BaseNpcActions.OnNpcAttack += HandleNpcAttack;

    

        IQuest.OnQuestUpdated += HandleQuestUpdated;
        IQuest.OnQuestEnded += HandleQuestEnded;
    }


    private void HandleQuestUpdated(IQuest quest) {
        Console.WriteLine($"Current stage: {quest.StageDescription}");
    }
    private void HandleQuestEnded(IQuest quest, bool succes) {
        Console.WriteLine($"Current stage: {quest.StageDescription}");
    }

    private void HandleWorldChanged(World oldworld, World newWorld) {

    }
    private void HandleWorldCreated(World world) {

    }



    private void HandleNpcCreated(NpcCharacter npc) {

    }
    private void HandleNpcAttack(Player player, NpcCharacter npc, int damage) {
        Console.WriteLine($"{npc.Name} hit you! You took {damage} damage!");
        player.Health -= damage;

        if (player.Health <= 0) {
            Console.WriteLine("You died!");
            
            player.CurrentArmor = null;
            player.CurrentWeapon = new MeleeWeapon.Fists();
            player.InventoryItems.Clear();
            player.Health = 100;

            World homeWorld = GameInstance.Worlds.First(x => x.IsSafeWorld);
            player.ChangeWorld(homeWorld);
        }
    }
    private void HandleNpcAction(NpcCharacter npc) {
        Console.WriteLine("Its your turn!");
    }
    private void HandleNpcKilled(Player? player, NpcCharacter npc) {
        if (player == null) return;

        ItemDrop? itemDrop = DropManager.GetRandomDrop();
        if (itemDrop is null) {
            Console.WriteLine("No drop!");
        } else {
            Console.WriteLine($"You got: {itemDrop.Name}");
            player.AddItem(itemDrop);
        }
    }

    private void HandleNpcAttacked(Player player, NpcCharacter npc, int damage) {

    }



    private void HandlePlayerAttack(Player player, NpcCharacter npc, int damage) {
        npc.Health -= damage;
        player.PlayerStatistics.DamageDealt += damage;
        
        if (npc.Health <= 0) {
            Console.WriteLine($"Enemy Killed! Damage dealt: {damage}");
            npc.KillNPC(player);
        } else {
            Console.WriteLine($"You've hit {npc.Name} with {player.CurrentWeapon?.Name}: Damage dealt: {damage}");
        }
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