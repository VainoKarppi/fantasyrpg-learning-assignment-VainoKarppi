
// Base Character Class
public abstract class NpcCharacter : ICharacter {
    public string Name { get; set; }
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




    public int ID { get; } = IDGenerator.GenerateId();

    public ItemArmor Armor { get; set; }

    public INpcActions? NpcActions { get; set; }

    public World CurrentWorld { get; private set; }

    // Events
    public static event Action<NpcCharacter>? OnNpcCreated;
    public static event Action<NpcCharacter, Player?>? OnNpcKilled;


    public NpcCharacter(World world, int xPos, int yPos) {
        CurrentWorld = world ?? throw new ArgumentNullException(nameof(world), "CurrentWorld cannot be null.");

        X = xPos;
        Y = yPos;

        Armor = new ItemArmor();
    }

    public abstract void DisplayCharacterStats();

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



    public static NpcCharacter CreateNPC(string npcType, World spawnWorld, int xPos, int yPos) {
        if (spawnWorld == null) throw new ArgumentNullException(nameof(spawnWorld), "World cannot be null when creating an NPC.");

        NpcCharacter npc = npcType.ToLower() switch {
            "warrior" => new NpcWarrior(spawnWorld, xPos, yPos),
            "mage" => new NpcMage(spawnWorld, xPos, yPos),
            "archer" => new NpcArcher(spawnWorld, xPos, yPos),
            _ => throw new ArgumentException("Invalid character type"),
        };

        spawnWorld.AddNPC(npc);
        
        OnNpcCreated?.Invoke(npc);

        return npc;
    }
}

