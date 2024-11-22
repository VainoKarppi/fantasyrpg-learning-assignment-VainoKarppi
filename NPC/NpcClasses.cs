
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