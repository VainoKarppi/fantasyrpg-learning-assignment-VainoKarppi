
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

    private void HandleNpcAttack(NpcCharacter npc, Player player, int damage) {
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, player.ID, Name = "Blood", CurrentWorldName = player.CurrentWorld.Name });
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.Health, CurrentWorldName = player.CurrentWorld.Name });
    }



    private void HandlePlayerAttack(Player player, Character npc, int damage) {
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, npc.ID, Name = "Blood", CurrentWorldName = npc.CurrentWorld.Name });
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateDataNpc, npc.ID, npc.Health, CurrentWorldName = npc.CurrentWorld.Name });
    }
}