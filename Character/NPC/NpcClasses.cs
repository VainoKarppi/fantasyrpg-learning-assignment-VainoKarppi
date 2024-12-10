
// Specific Character Classes
public class NpcWarrior : NpcCharacter {
    public NpcWarrior(World spawnWorld, int xPos, int yPos) : base(spawnWorld, xPos, yPos) {
        Name = "Warrior";
        Health = 150;
        MaxHealth = 150;
        Mana = 20;

        AttackTime = 300;
        
        CurrentArmor = new MetalArmor();
        CurrentWeapon = new MeleeWeapon.BasicSword();
        Actions = new WarriorActions(this);
    }
}

public class NpcMage : NpcCharacter {
    public NpcMage(World spawnWorld, int xPos, int yPos) : base(spawnWorld, xPos, yPos) {
        Name = "Mage";
        Health = 70;
        MaxHealth = 70;
        Mana = 120;

        CurrentArmor = new MageRobe();
        CurrentWeapon = new MageWeapon.BasicWand();
        Actions = new MageActions(this);
    }
}

public class NpcArcher : NpcCharacter {
    public NpcArcher(World spawnWorld, int xPos, int yPos) : base(spawnWorld, xPos, yPos) {
        Name = "Archer";
        Health = 90;
        MaxHealth = 90;
        Mana = 50;

        CurrentArmor = new LeatherArmor();
        CurrentWeapon = new RangedWeapon.BasicBow();
        Actions = new RangerActions(this);
    }
}