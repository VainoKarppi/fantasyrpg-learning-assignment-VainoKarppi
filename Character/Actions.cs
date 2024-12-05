
public interface IPlayerActions {
    void Attack(NpcCharacter target);
    void UsePotion(ItemPotion potion);
    bool HasWeapon();
    bool HasArmor();
    int CalculateDamageToNPC(NpcCharacter target);
}

public abstract class BasePlayerActions(Player player) : IPlayerActions {

    public abstract void Attack(NpcCharacter target);
    public abstract void UsePotion(ItemPotion potion);

    public virtual bool HasWeapon() {
        return player.CurrentWeapon != null;
    }

    public virtual bool HasArmor() {
        return player.CurrentArmor != null;
    }


    public virtual int CalculateDamageToNPC(NpcCharacter npc) {
        if (!HasWeapon()) throw new Exception("No weapon in use!");
        
        ItemWeapon playerWeapon = player.CurrentWeapon!;
        ItemArmor? playerArmor = player.CurrentArmor;

        double baseDamage = playerWeapon.Damage;

        double armorDamageBoost = 1;
        if (playerArmor is not null) {
            armorDamageBoost = playerWeapon.Type switch {
                ItemType.MeleeWeapon => playerArmor.MeleeAttackMultiplier,
                ItemType.RangedWeapon => playerArmor.RangedAttackMultiplier,
                ItemType.MageWeapon => playerArmor.MageAttackMultiplier,
                _ => 1,
            };
        }
        
        double armorDefenseBoost = 1;
        if (npc.Armor != null) {
            armorDefenseBoost = npc.Armor.Type switch {
                ItemType.MeleeArmor => npc.Armor.MeleeDefenseMultiplier,
                ItemType.RangedArmor => npc.Armor.RangedDefenseMultiplier,
                ItemType.MageArmor => npc.Armor.MageDefenseMultiplier,
                _ => 1,
            };
        }

        double finalDamage = baseDamage * armorDamageBoost / armorDefenseBoost;

        return (int)Math.Round(finalDamage);
    }

}

public class PlayerActions(Player player) : BasePlayerActions(player) {

    // Events
    public static event Action<Player, NpcCharacter, int>? OnPlayerAttack;
    public static event Action<Player, ItemPotion>? OnPlayerPotionUse;
    public static event Action<Player>? OnPlayerAction;

    private readonly Player _player = player;

    public override void Attack(NpcCharacter target) {
        int damage = CalculateDamageToNPC(target);

        _player.PlayerStatistics.DamageDealt += damage;

        target.Health -= damage;


        OnPlayerAttack?.InvokeFireAndForget(_player, target, damage);

        // Kill enemy before triggering next action, to make sure the world NPC count gets to 0 before next attack if necessary
        if (target.Health <= 0) target.KillNPC(_player);

        OnPlayerAction?.InvokeFireAndForget(_player);
    }

    public override void UsePotion(ItemPotion potion) {
        OnPlayerPotionUse?.InvokeFireAndForget(_player, potion);
        OnPlayerAction?.InvokeFireAndForget(_player);
    }
}
