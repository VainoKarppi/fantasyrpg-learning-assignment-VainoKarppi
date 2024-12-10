
using GUI;

public interface ICharacter {
    int ID { get; }
    string? Name { get; }
    int Health { get; }
    int Mana { get; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; }
    public int Height { get; }
    public Color Color => Color.Red;
    public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

    public ItemWeapon? CurrentWeapon { get; set; }
    public ItemArmor? CurrentArmor { get; set; } 
}




public abstract class Character : ICharacter {
    public World CurrentWorld { get; set; }

    public int ID { get; set; } = -1;

    public string? Name { get; set; }

    public int Health { get; set; } = 100;
    public int MaxHealth { get; set; } = 100;
    public int Mana { get; set; } = 100;

    public int X { get; set; } = GameForm.ScreenWidth / 2;
    public int Y { get; set; } = (GameForm.ScreenHeight - GUI.GameForm.StatsBarHeight) / 2;
    public int Width { get; set; } = 20;
    public int Height { get; set; } = 20;

    public State CurrentState { get; set; }
    public IActions Actions { get; set; }

    public ItemWeapon? CurrentWeapon { get; set; }
    public ItemArmor? CurrentArmor { get; set; }


    
    public enum State {
        Idle,
        Attacking,
        Moving,
        Waiting
    }

    public virtual Point GetCenter() {
        return new Point(X + Width/2, Y + Height/2);
    }

    public virtual bool CanAttack(Character target) {
        // Check weapon range (assuming CalculateDistance is a method of World)
        double distance = World.CalculateDistance(this, target);
        if (CurrentWeapon == null) return false;

        int weaponRange = CurrentWeapon.Range;
        if (weaponRange < distance) return false;

        return true;
    }

    public virtual bool HasArmor() {
        return CurrentArmor != null;
    }
    public virtual bool HasWeapon() {
        return CurrentWeapon != null;
    }

    public abstract void Kill(Character? killer = null);

    public virtual int CalculateDamage(Character target) {
        if (CurrentWeapon is null) return 0;


        double armorDamageBoost = 1;
        if (CurrentArmor is not null) {
            armorDamageBoost = CurrentWeapon.Type switch {
                ItemType.MeleeWeapon => CurrentArmor.MeleeAttackMultiplier,
                ItemType.RangedWeapon => CurrentArmor.RangedAttackMultiplier,
                ItemType.MageWeapon => CurrentArmor.MageAttackMultiplier,
                _ => 1,
            };
        }
        
        double armorDefenseBoost = 1;
        if (target.CurrentArmor != null) {
            armorDefenseBoost = target.CurrentArmor.Type switch {
                ItemType.MeleeArmor => target.CurrentArmor.MeleeDefenseMultiplier,
                ItemType.RangedArmor => target.CurrentArmor.RangedDefenseMultiplier,
                ItemType.MageArmor => target.CurrentArmor.MageDefenseMultiplier,
                _ => 1,
            };
        }

        double finalDamage = CurrentWeapon.Damage * armorDamageBoost / armorDefenseBoost;

        return (int)Math.Round(finalDamage);
    }
    
}

