
// Base Character Class
public partial class NpcCharacter : Character, IWorldChanger {

    public int AttackTime { get; set; } = 1500; // Milliseconds after player hit the NPC



    // 3D parameters

    public Color Color => Color.Red;
    public Rectangle Bounds => new Rectangle(X, Y, Width, Height);




    // Events
    public static event Action<NpcCharacter>? OnNpcCreated;
    public static event Action<NpcCharacter, Player?>? OnNpcKilled;


    public NpcCharacter(World world, int xPos, int yPos) {
        CurrentWorld = world ?? throw new ArgumentNullException(nameof(world), "CurrentWorld cannot be null.");
        
        X = xPos;
        Y = yPos;

        CurrentArmor = new ItemArmor();
    }
    public void TransferWorld(World newWorld) {
        if (newWorld == null) throw new ArgumentNullException(nameof(newWorld), "New world cannot be null.");
        

        // Remove NPC from current world and add to the new world
        CurrentWorld.RemoveNPC(this);
        newWorld.AddNPC(this);
        CurrentWorld = newWorld;
    }

    public override void Kill(Character? killer = null) {
        CurrentWorld.RemoveNPC(this);

        if (killer is Player) (killer as Player)!.PlayerStatistics.EnemiesKilled++;

        OnNpcKilled?.InvokeFireAndForget(this, killer);
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
        
        OnNpcCreated?.InvokeFireAndForget(npc);

        return npc;
    }
}

