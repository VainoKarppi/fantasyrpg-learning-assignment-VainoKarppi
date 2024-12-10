using GUI;

public abstract class BaseNpcActions(NpcCharacter npc) : IActions {
    // Events
    public static event Action<NpcCharacter>? OnNpcAction;
    public static event Action<NpcCharacter, ItemPotion>? OnNpcPotionUse;
    public static event Action<NpcCharacter, Player, int>? OnNpcAttack;


    // Implement Attack method
    public virtual void Attack(Character? target) {
        if (npc == null || npc.CurrentWeapon == null || target == null) return;

        int damage = npc.CalculateDamage(target!);

        target!.Health -= damage;
        Effect.TriggerScreenShake();

        OnNpcAttack?.Invoke(npc!, (target as Player)!, damage);
        OnNpcAction?.Invoke(npc!);

        if (target!.Health <= 0) target!.Kill();
    }

    // Implement UsePotion method
    public virtual void UsePotion(ItemPotion potion) {
        npc.Health += potion.Effect;

        OnNpcPotionUse?.InvokeFireAndForget(npc, potion);
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