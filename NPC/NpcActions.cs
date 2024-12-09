public interface INpcActions {
    void Attack(Player player);
    void UsePotion();
    int CalculateDamageToPlayer(Player player);
}


public abstract class BaseNpcActions(NpcCharacter npc) : INpcActions {
    public static event Action<NpcCharacter>? OnNpcAction;
    public static event Action<Player, NpcCharacter, int>? OnNpcAttack;
    protected void RaiseNpcActionEvent() {
        OnNpcAction?.InvokeFireAndForget(npc);
    }

    protected void RaiseNpcAttackEvent(Player player, int damage) {

        player.Health -= damage;

        OnNpcAttack?.InvokeFireAndForget(player, npc, damage);

        if (player.Health <= 0) {
            player.KillPlayer();
        }
    }


    public abstract void Attack(Player player);
    public abstract void UsePotion();


    public virtual int CalculateDamageToPlayer(Player player) {
        // TODO similiar to player attack, just dont use armor to calculate, instead use npc class
        return new Random().Next(5,20);
    }

    public void AttackFromNpcAsync(Player player) {
        GUI.GameForm.TriggerDamageEffect();
    }

}


public class WarriorActions(NpcCharacter npc) : BaseNpcActions(npc) {
    public override void Attack(Player player) {   
        int damage = CalculateDamageToPlayer(player);
        AttackFromNpcAsync(player);

        RaiseNpcAttackEvent(player, damage);
        RaiseNpcActionEvent();
    }

    public override void UsePotion() {
        // TODO
    }
}



public class MageActions(NpcCharacter npc) : BaseNpcActions(npc) {

    public override void Attack(Player player) {
        int damage = CalculateDamageToPlayer(player);
        AttackFromNpcAsync(player);

        RaiseNpcAttackEvent(player, damage);
        RaiseNpcActionEvent();
    }

    public override void UsePotion() {
        // TODO
    }

}


public class RangerActions(NpcCharacter npc) : BaseNpcActions(npc) {

    public override void Attack(Player player) {
        int damage = CalculateDamageToPlayer(player);
        AttackFromNpcAsync(player);

        RaiseNpcAttackEvent(player, damage);
        RaiseNpcActionEvent();
    }

    public override void UsePotion() {
        // TODO
    }
    
}