using System.Text.Json;

public partial class Database {
    public static event Action<Player>? OnPlayerRestored;
    public static event Action<Player>? OnPlayerSaved;

    public static async Task SavePlayerAsync(Player player) {
        using var db = new MyDbContext();
        var playerEntity = db.Players.FirstOrDefault(p => p.ID == player.ID);
        if (playerEntity == null) {
            playerEntity = new PlayerEntity();
            db.Players.Add(playerEntity);
        }

        playerEntity.ID = player.ID;
        playerEntity.Health = player.Health;
        playerEntity.Mana = player.Mana;
        playerEntity.Name = player.Name!;
        playerEntity.Money = (int)player.Money!;
        playerEntity.X = player.X;
        playerEntity.Y = player.Y;
        playerEntity.Width = player.Width;
        playerEntity.Height = player.Height;

        playerEntity.CurrentWeapon = player.CurrentWeapon?.Name;
        playerEntity.CurrentArmor = player.CurrentArmor?.Name;
        playerEntity.CurrentWorld = player.CurrentWorld.Name;
        playerEntity.InventoryItems = player.InventoryItems.Select(i => i.Name).ToList()!;

        playerEntity.CurrentQuestName = player.CurrentQuest?.Name;
        playerEntity.QuestListJson = JsonSerializer.Serialize(player.QuestList.ToDictionary(q => q.Name, q => q.CurrentStageIndex));
        playerEntity.CompletedQuests = player.CompletedQuests;

        playerEntity.StatisticsJson = JsonSerializer.Serialize(player.PlayerStatistics);

        await db.SaveChangesAsync();

        OnPlayerSaved?.InvokeFireAndForget(player);
    }

    public static Player? RestorePlayer(string name) {
        using var db = new MyDbContext();

        // Restore player from DB using name
        PlayerEntity? playerEntity = db.Players.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
        if (playerEntity == null) return null;

        List<ItemBase> restoredItems = playerEntity.InventoryItems
            .Select(ItemBase.GetItemByName)
            .Where(invItem => invItem != null)
            .Cast<ItemBase>()
            .ToList();

        

        var player = new Player {
            ID = playerEntity.ID,

            Health = playerEntity.Health,
            Mana = playerEntity.Mana,
            Name = playerEntity.Name,
            Money = playerEntity.Money,
            
            X = playerEntity.X,
            Y = playerEntity.Y,
            Width = playerEntity.Width,
            Height = playerEntity.Height,

            CurrentWeapon = ItemWeapon.GetWeaponByName(playerEntity.CurrentWeapon),
            CurrentArmor = ItemArmor.GetArmorByName(playerEntity.CurrentArmor),
            InventoryItems = restoredItems,

            CurrentState = Character.State.Idle,

            CompletedQuests = playerEntity.CompletedQuests,

            CurrentWorld = GameInstance.GetWorld(playerEntity.CurrentWorld!),

            PlayerStatistics = JsonSerializer.Deserialize<Player.Statistics>(playerEntity.StatisticsJson)!
        };

        //--- Get quests
        Dictionary<string, int> questList = JsonSerializer.Deserialize<Dictionary<string, int>>(playerEntity.QuestListJson) ?? new Dictionary<string, int>();
        foreach (KeyValuePair<string, int> dbQuest in questList) {
            IQuest? quest = IQuest.GetQuestByName(dbQuest.Key, player);
            if (quest != null) {
                quest.CurrentStageIndex = dbQuest.Value;

                if (quest.Name == playerEntity.CurrentQuestName) player.CurrentQuest = quest;
                player.QuestList.Add(quest);
            }
        }
        
        //--- Initialize player
        player.Actions = new PlayerActions(player);
        GameInstance.AddPlayerToInstance(player);

        OnPlayerRestored?.InvokeFireAndForget(player);

        return player;
    }
}