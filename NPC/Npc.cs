
// Base Character Class
public abstract class NpcCharacter {
    public string? Name { get; protected set; }
    public int Health { get; set; }
    public int Mana { get; set; }
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int AttackTime { get; set; } = 5;



    public ItemArmor Armor { get; set; }

    public INpcActions? NpcActions { get; set; }

    public World CurrentWorld { get; private set; }

    // Events
    public static event Action<NpcCharacter>? OnNpcCreated;
    public static event Action<Player?, NpcCharacter>? OnNpcKilled;


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

        OnNpcKilled?.Invoke(killer, this);
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

// Specific Character Classes
public class NpcWarrior : NpcCharacter {
    public NpcWarrior(World spawnWorld) : base(spawnWorld) {
        Name = "Warrior";
        Health = 150;
        Mana = 20;
        Strength = 80;
        Agility = 30;
        Armor = new MetalArmor();
        NpcActions = new WarriorActions(this);
    }

    public override void DisplayCharacterStats() {
        Console.WriteLine($"Warrior Stats - Health: {Health}, Mana: {Mana}, Strength: {Strength}, Agility: {Agility}");
    }
}

public class NpcMage : NpcCharacter {
    public NpcMage(World spawnWorld) : base(spawnWorld) {
        Name = "Mage";
        Health = 70;
        Mana = 120;
        Strength = 20;
        Agility = 40;
        Armor = new MageRobe();
        NpcActions = new MageActions(this);
    }

    public override void DisplayCharacterStats() {
        Console.WriteLine($"Mage Stats - Health: {Health}, Mana: {Mana}, Strength: {Strength}, Agility: {Agility}");
    }
}

public class NpcArcher : NpcCharacter {
    public NpcArcher(World spawnWorld) : base(spawnWorld) {
        Name = "Archer";
        Health = 90;
        Mana = 50;
        Strength = 40;
        Agility = 80;
        Armor = new LeatherArmor();
        NpcActions = new RangerActions(this);
    }

    public override void DisplayCharacterStats() {
        Console.WriteLine($"Archer Stats - Health: {Health}, Mana: {Mana}, Strength: {Strength}, Agility: {Agility}");
    }
}
