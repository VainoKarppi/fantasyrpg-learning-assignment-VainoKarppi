
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
        Console.WriteLine($"{unit} changed World. To {newWorld.Name} from {oldworld.Name}");
    }
    private void HandleWorldCreated(World world) {
        Console.WriteLine($"Created world: {world.Name}");
    }


    private void HandlePlayerKilled(Player player, Character? killer) {
        if (killer != null) {
            MessageBox.Show($"You were killed by: {killer.Name}", "You Died", MessageBoxButtons.OK, MessageBoxIcon.Information);
        } else {
            Effect.DeathEffect();
        }

        GameForm.RefreshPage();
    }
    private void HandlePlayerCreated(Player player) {
        Console.WriteLine($"Created Player: {player.Name}");
    }


    private void HandleNpcCreated(NpcCharacter npc) {
        Console.WriteLine($"Created NPC: {npc.Name} to World: {npc.CurrentWorld.Name}");
    }
    private void HandleNpcAttack(NpcCharacter npc, Character target, int damage) {
        Console.WriteLine($"{npc.Name} hit you! You took {damage} damage!");

        new Effect(target, npc, Effect.EffectType.Blood);
        GameForm.RefreshPage();
    }
    private void HandleNpcAction(NpcCharacter npc) {
        
    }
    private void HandleNpcKilled(NpcCharacter npc, Player? player) {
        if (player == null) return; // Check if it was killed by a player

        Console.WriteLine($"Enemy Killed!");

        ItemDrop? itemDrop = DropManager.GetRandomDrop();
        if (itemDrop != null) player.AddItem(itemDrop);
    }




    private void HandlePlayerAttack(Player player, Character npc, int damage) {
        Console.WriteLine($"You've hit {npc.Name} with {player.CurrentWeapon?.Name}: Damage dealt: {damage}");

        new Effect(npc, player, Effect.EffectType.Blood);
    }
    private void HandlePlayerAction(Player player) {
    }


    private void HandlePotionUse(Player player, ItemPotion potion) {

        
    }
}