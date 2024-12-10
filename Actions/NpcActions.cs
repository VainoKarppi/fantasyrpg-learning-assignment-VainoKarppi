using GUI;

public abstract class BaseNpcActions : IActions {
    protected NpcCharacter npc;

    // Events to notify about NPC actions
    public static event Action<NpcCharacter>? OnNpcAction;
    public static event Action<NpcCharacter, Player, int>? OnNpcAttack;

    public BaseNpcActions(NpcCharacter npc) {
        this.npc = npc;
    }

    // Implement Attack method
    public virtual void Attack(Character? target) {
        
        if (npc.CurrentWeapon == null) return;
        Console.WriteLine(npc.CurrentWeapon.Type);

        if (npc.CurrentWeapon.Type == ItemType.MeleeWeapon) new Effect(npc, target, Effect.EffectType.Melee);
        if (npc.CurrentWeapon.Type == ItemType.MageWeapon) new Effect(npc, target, Effect.EffectType.Mage);
        if (npc.CurrentWeapon.Type == ItemType.RangedWeapon) new Effect(npc, target, Effect.EffectType.Ranged);

        if (target == null) return;
        int damage = npc.CalculateDamage(target);

        OnNpcAttack?.Invoke(npc, (target as Player)!, damage);
        OnNpcAction?.Invoke(npc);

        Effect.TriggerScreenShake();

        target.Health -= damage;
        if (target.Health <= 0) target.Kill();
    }

    // Implement UsePotion method
    public virtual void UsePotion(ItemPotion potion) {
        npc.Health += potion.Effect;

        OnNpcAction?.Invoke(npc);
    }
}

public class WarriorActions(NpcCharacter npc) : BaseNpcActions(npc) {
    public override void Attack(Character? target) {
        base.Attack(target);
    }

    public override void UsePotion(ItemPotion potion) {
        base.UsePotion(potion);
    }
}

public class MageActions(NpcCharacter npc) : BaseNpcActions(npc) {
    public override void Attack(Character? target) {
        base.Attack(target);
    }

    public override void UsePotion(ItemPotion potion) {
        base.UsePotion(potion);
    }
}

public class RangerActions(NpcCharacter npc) : BaseNpcActions(npc) {
    public override void Attack(Character? target) {
        base.Attack(target);
    }

    public override void UsePotion(ItemPotion potion) {
        base.UsePotion(potion);
    }
}