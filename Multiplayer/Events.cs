
public class NetworkEventListener {
    public NetworkEventListener() {

        PlayerActions.OnPlayerAttack += HandlePlayerAttack;

        Player.OnPlayerRespawn += HandlePlayerRespawn;

        BaseNpcActions.OnNpcAttack += HandleNpcAttack;
    }



    private void HandlePlayerRespawn(Player player) {
        // Send update location
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.X, player.Y, CurrentWorldName = player.CurrentWorld.Name, player.Health });
    }

    private void HandleNpcAttack(Player player, NpcCharacter npc, int damage) {
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.Health });
    }



    private void HandlePlayerAttack(Player player, NpcCharacter npc, int damage) {
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateDataNpc, npc.ID, npc.Health });
    }
}