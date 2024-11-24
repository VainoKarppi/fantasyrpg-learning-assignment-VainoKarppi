
// Specific Character Classes
public class NpcWarrior : NpcCharacter {
    public NpcWarrior(World spawnWorld, int xPos, int yPos) : base(spawnWorld, xPos, yPos) {
        Name = "Warrior";
        Health = 150;
        Mana = 20;
        Strength = 80;
        Agility = 30;
        Armor = new MetalArmor();
        NpcActions = new WarriorActions(this);
    }
}

public class NpcMage : NpcCharacter {
    public NpcMage(World spawnWorld, int xPos, int yPos) : base(spawnWorld, xPos, yPos) {
        Name = "Mage";
        Health = 70;
        Mana = 120;
        Strength = 20;
        Agility = 40;
        Armor = new MageRobe();
        NpcActions = new MageActions(this);
    }
}

public class NpcArcher : NpcCharacter {
    public NpcArcher(World spawnWorld, int xPos, int yPos) : base(spawnWorld, xPos, yPos) {
        Name = "Archer";
        Health = 90;
        Mana = 50;
        Strength = 40;
        Agility = 80;
        Armor = new LeatherArmor();
        NpcActions = new RangerActions(this);
    }
}