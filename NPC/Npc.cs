
// Base Character Class
public partial class NpcCharacter : ICharacter, IWorldChanger {
    public string? Name { get; set; }
    public int MaxHealth { get; set; } = 100;
    public int Health { get; set; }
    public int Mana { get; set; }
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int AttackTime { get; set; } = 5;


    // 3D parameters
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 20;
    public int Height { get; set; } = 20;
    public Color Color => Color.Red;
    public Rectangle Bounds => new Rectangle(X, Y, Width, Height);




    public int ID { get; set; } = -1;

    public ItemArmor Armor { get; set; }

    public INpcActions? NpcActions { get; set; }

    public World CurrentWorld { get; set; }

    // Events
    public static event Action<NpcCharacter>? OnNpcCreated;
    public static event Action<NpcCharacter, Player?>? OnNpcKilled;


    public NpcCharacter(World world, int xPos, int yPos) {
        CurrentWorld = world ?? throw new ArgumentNullException(nameof(world), "CurrentWorld cannot be null.");
        
        X = xPos;
        Y = yPos;

        Armor = new ItemArmor();
    }
    public void TransferWorld(World newWorld) {
        if (newWorld == null) {
            throw new ArgumentNullException(nameof(newWorld), "New world cannot be null.");
        }

        // Remove NPC from current world and add to the new world
        CurrentWorld.RemoveNPC(this);
        newWorld.AddNPC(this);
        CurrentWorld = newWorld;
    }

    public void KillNPC(Player? killer = null) {
        CurrentWorld.RemoveNPC(this);

        if (killer != null) killer.PlayerStatistics.EnemiesKilled++;

        OnNpcKilled?.Invoke(this, killer);
    }



    public static NpcCharacter CreateNPC(string npcType, World spawnWorld, (int xPos, int yPos) position, int? health = null) {
        if (spawnWorld == null) throw new ArgumentNullException(nameof(spawnWorld), "World cannot be null when creating an NPC.");

        NpcCharacter npc = npcType.ToLower() switch {
            "warrior" => new NpcWarrior(spawnWorld, position.xPos, position.yPos),
            "mage" => new NpcMage(spawnWorld, position.xPos, position.yPos),
            "archer" => new NpcArcher(spawnWorld, position.xPos, position.yPos),
            _ => throw new ArgumentException("Invalid character type"),
        };

        if (health != null) npc.Health = (int)health;

        spawnWorld.AddNPC(npc);
        
        OnNpcCreated?.Invoke(npc);

        return npc;
    }
}

