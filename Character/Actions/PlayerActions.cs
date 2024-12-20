


using GUI;

public class PlayerActions(Player player) : IActions {
    // Events
    public static event Action<Player, Character, int>? OnPlayerAttack;
    public static event Action<Player, ItemPotion>? OnPlayerPotionUse;
    public static event Action<Player>? OnPlayerAction;



    public virtual void Attack(Character? target) {
        if (player.CurrentWeapon == null) return;
       

        if (target == null || !player.CanAttack(target)) return;

        player.Mana -= player.CurrentWeapon.ManaRequired;

        int damage = player.CalculateDamage(target);

        player.PlayerStatistics.DamageDealt += damage;

        target.Health -= damage;

        OnPlayerAttack?.InvokeFireAndForget(player, target, damage);
        OnPlayerAction?.InvokeFireAndForget(player);

        if (target.Health <= 0) target.Kill(player);
    }

    public virtual void UsePotion(ItemPotion potion) {
        // Health potion
        if (potion is ItemPotion.HealthPotion) {
            if (player.Health == 100) {
                MessageBox.Show("Already full health!");
                return;
            }

            player.Health = Math.Min(player.Health + potion.Effect, 100);
            Console.WriteLine($"Health potion used! Health: {player.Health}");
        }


        // Mana potion
        if (potion is ItemPotion.ManaPotion) {
            if (player.Mana == 100) {
                MessageBox.Show("Already full mana!");
                return;
            }
        
            player.Mana = Math.Min(player.Mana + potion.Effect, 100);
            Console.WriteLine($"Mana potion used! Mana: {player.Mana}");   
        }

        bool potionUsed = player.InventoryItems.Remove(potion);
        if (potionUsed) OnPlayerPotionUse?.InvokeFireAndForget(player, potion);

        OnPlayerAction?.InvokeFireAndForget(player);
    }


    

}