using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class MyDbContext : DbContext {
    public DbSet<PlayerEntity> Players { get; set; }
    public DbSet<NpcEntity> Npcs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseSqlite("Data Source=RpgGame.db");
    }
}

public class PlayerEntity {
    public int ID { get; set; }
    public int Health { get; set; }
    public int Mana { get; set; }
    public string Name { get; set; }
    public int Money { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public string? CurrentWeapon { get; set; }
    public string? CurrentArmor { get; set; }

    public string CurrentWorld { get; set; }


    public List<string> InventoryItems { get; set; }
    public string? CurrentQuestName { get; set; }
    public string QuestListJson { get; set; }
    public List<string> CompletedQuests { get; set; } = [];


    public string StatisticsJson { get; set; }
}


public class NpcEntity {
    public int ID { get; set; }
    public int Health { get; set; }
    public int Mana { get; set; }
    public string Name { get; set; }
    public int Money { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public string? CurrentWeapon { get; set; }
    public string? CurrentArmor { get; set; }

    public string CurrentWorld { get; set; }
}