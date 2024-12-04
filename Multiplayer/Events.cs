
using System.Net.Sockets;

public class NetworkEventListener {
    public NetworkEventListener() {

        PlayerActions.OnPlayerAttack += HandlePlayerAttack;

        Player.OnPlayerRespawn += HandlePlayerRespawn;

        BaseNpcActions.OnNpcAttack += HandleNpcAttack;

        MultiplayerClient.OnConnectEnd += HandleMultiplayerClientConnectEnd;
    }


    private void HandleMultiplayerClientConnectEnd(TcpClient client, Player player, Exception? ex) {
        if (ex != null) return; // check connect success

        // Send current pos data
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.X, player.Y, CurrentWorldName = player.CurrentWorld.Name });
    }

    private void HandlePlayerRespawn(Player player) {
        // Send update location
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.X, player.Y, CurrentWorldName = player.CurrentWorld.Name, player.Health });
    }

    private void HandleNpcAttack(Player player, NpcCharacter npc, int damage) {
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.Health, CurrentWorldName = player.CurrentWorld.Name });
    }



    private void HandlePlayerAttack(Player player, NpcCharacter npc, int damage) {
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateDataNpc, npc.ID, npc.Health, CurrentWorldName = npc.CurrentWorld.Name });
    }
}