public interface INpcActions {
    void Attack(Player player);
    void UsePotion();
    int CalculateDamageToPlayer(Player player);
}


public abstract class BaseNpcActions : INpcActions {
    public static event Action<NpcCharacter>? OnNpcAction;
    public static event Action<Player, NpcCharacter, int>? OnNpcAttack;
    protected void RaiseNpcActionEvent() {
        OnNpcAction?.Invoke(_npc);
    }

    protected void RaiseNpcAttackEvent(Player player, int damage) {
        OnNpcAttack?.Invoke(player, _npc, damage);
    }


    protected NpcCharacter _npc;
    protected BaseNpcActions(NpcCharacter npc) {
        _npc = npc;
    }

    public abstract void Attack(Player player);
    public abstract void UsePotion();


    public virtual int CalculateDamageToPlayer(Player player) {
        
        return 20;
    }

    public async Task<bool> AttackFromNpcAsync(Player player) {
        using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = cancellationTokenSource.Token;

        World fightWorld = player.CurrentWorld;

        Console.WriteLine($"{_npc.Name} is about to hit you!");

        // Start the countdown on a separate task
        Task countdownTask = Task.Run(() => {
            for (int i = _npc.AttackTime; i > 0; i--) {
                // Check if cancellation is requested or fight is canceled
                if (cancellationToken.IsCancellationRequested || fightWorld != player.CurrentWorld) return;

                Console.Write($"                   Attack incoming in: {i} seconds...   \r");
                Thread.Sleep(1000);
            }
        }, cancellationToken);

        // Monitor user input while the countdown is running
        while (!countdownTask.IsCompleted) {
            if (Console.KeyAvailable) {
                string? userInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(userInput)) {
                    Console.WriteLine($"\n\nYou typed: {userInput}");
                }
            }

            await Task.Delay(100); // Small delay to prevent tight loop
        }

        if (cancellationToken.IsCancellationRequested || fightWorld != player.CurrentWorld)
            return false;

        return true;
    }

}


public class WarriorActions(NpcCharacter npc) : BaseNpcActions(npc) {
    public override async void Attack(Player player) {
        bool attackDone = await AttackFromNpcAsync(player);
        if (!attackDone) return;

        int damage = CalculateDamageToPlayer(player);

        Console.WriteLine("The Warrior hit you!");

        RaiseNpcAttackEvent(player, damage);
        RaiseNpcActionEvent();
    }

    public override void UsePotion() {

    }
}



public class MageActions(NpcCharacter npc) : BaseNpcActions(npc) {

    public override async void Attack(Player player) {

        bool attackDone = await AttackFromNpcAsync(player);
        if (!attackDone) return;
        
        int damage = CalculateDamageToPlayer(player);

        Console.WriteLine("The Mage casts a powerful spell!");

        RaiseNpcAttackEvent(player, damage);
        RaiseNpcActionEvent();
    }

    public override void UsePotion() {
        Console.WriteLine("The Mage creates a magical barrier to absorb damage.");
    }

}


public class RangerActions(NpcCharacter npc) : BaseNpcActions(npc) {

    public override async void Attack(Player player) {
        Console.WriteLine("The Ranger shot you!");

        bool attackDone = await AttackFromNpcAsync(player);
        if (!attackDone) return;
        
        int damage = CalculateDamageToPlayer(player);

        RaiseNpcAttackEvent(player, damage);
        RaiseNpcActionEvent();
    }

    public override void UsePotion() {
        Console.WriteLine("The Mage creates a magical barrier to absorb damage.");
    }
    
}