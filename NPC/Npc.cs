
// Base Character Class
public abstract class NpcCharacter : ICharacter {
    public string Name { get; set; }
    public int Health { get; set; }
    public int Mana { get; set; }
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int AttackTime { get; set; } = 5;

    public int ID { get; } = IDGenerator.GenerateId();


    public ItemArmor Armor { get; set; }

    public INpcActions? NpcActions { get; set; }

    public World CurrentWorld { get; private set; }

    // Events
    public static event Action<NpcCharacter>? OnNpcCreated;
    public static event Action<NpcCharacter, Player?>? OnNpcKilled;


    public NpcCharacter(World world) {
        CurrentWorld = world ?? throw new ArgumentNullException(nameof(world), "CurrentWorld cannot be null.");

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



    public static NpcCharacter CreateNPC(string npcType, World world) {
        if (world == null) throw new ArgumentNullException(nameof(world), "World cannot be null when creating an NPC.");

        NpcCharacter npc = npcType.ToLower() switch {
            "warrior" => new NpcWarrior(world),
            "mage" => new NpcMage(world),
            "archer" => new NpcArcher(world),
            _ => throw new ArgumentException("Invalid character type"),
        };

        world.AddNPC(npc);
        
        OnNpcCreated?.Invoke(npc);

        return npc;
    }
}

