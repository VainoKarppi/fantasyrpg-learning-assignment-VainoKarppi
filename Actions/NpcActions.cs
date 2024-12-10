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
        if (npc == null || npc.CurrentWeapon == null || target == null) return;

        if (npc!.CurrentWeapon.Type == ItemType.MeleeWeapon) {
            new Effect(npc, target, Effect.EffectType.Melee);
            MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, npc.ID, Name = "Melee", CurrentWorldName = npc.CurrentWorld.Name });
        }


        if (npc!.CurrentWeapon.Type == ItemType.MageWeapon) {
            new Effect(npc, target, Effect.EffectType.Mage);
            MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, npc.ID, TargetID = target?.ID, Name = "Mage", CurrentWorldName = npc.CurrentWorld.Name });
        }
        if (npc!.CurrentWeapon.Type == ItemType.RangedWeapon) {
            new Effect(npc, target, Effect.EffectType.Ranged);
            MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, npc.ID, TargetID = target?.ID, Name = "Range", CurrentWorldName = npc.CurrentWorld.Name });
        }

        
        int damage = npc.CalculateDamage(target!);

        OnNpcAttack?.Invoke(npc!, (target as Player)!, damage);
        OnNpcAction?.Invoke(npc!);

        Effect.TriggerScreenShake();

        target!.Health -= damage;
        if (target!.Health <= 0) target!.Kill();
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