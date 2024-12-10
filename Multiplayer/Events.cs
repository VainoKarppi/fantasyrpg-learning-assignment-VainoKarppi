
using System.Net.Sockets;

public class NetworkEventListener {
    public NetworkEventListener() {

        PlayerActions.OnPlayerAttack += HandlePlayerAttack;

        Player.OnPlayerRespawn += HandlePlayerRespawn;

        BaseNpcActions.OnNpcAttack += HandleNpcAttack;

        MultiplayerClient.OnConnectEnd += HandleMultiplayerClientConnectEnd;
    }


    private void HandleMultiplayerClientConnectEnd(TcpClient client, Player player, Exception? ex) {
        if (ex != null) {
            MessageBox.Show($"ERROR: {ex.Message}");
            return;
        }

        // Send current pos data
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.X, player.Y, CurrentWorldName = player.CurrentWorld.Name });
    }

    private void HandlePlayerRespawn(Player player) {
        // Send update location
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.X, player.Y, CurrentWorldName = player.CurrentWorld.Name, player.Health });
    }

    private void HandleNpcAttack(NpcCharacter npc, Player player, int damage) {
        // Create effects
        if (npc!.CurrentWeapon!.Type == ItemType.MeleeWeapon) {
            MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, npc.ID, npc.CurrentWeapon.Range, TargetID = player?.ID, Name = "Melee", CurrentWorldName = npc.CurrentWorld.Name });
        }

        if (npc!.CurrentWeapon!.Type == ItemType.MageWeapon) {
            MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, npc.ID, npc.CurrentWeapon.Range, TargetID = player?.ID, Name = "Mage", CurrentWorldName = npc.CurrentWorld.Name });
        }

        if (npc!.CurrentWeapon!.Type == ItemType.RangedWeapon) {
            MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, npc.ID, npc.CurrentWeapon.Range, TargetID = player?.ID, Name = "Ranged", CurrentWorldName = npc.CurrentWorld.Name });
        }

        // TODO fix blood effect (works in HandlePlayerAttack)
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, player?.ID, Name = "Blood", CurrentWorldName = player?.CurrentWorld.Name });
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player?.ID, player?.Health, CurrentWorldName = player?.CurrentWorld.Name });
    }



    private void HandlePlayerAttack(Player player, Character npc, int damage) {
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.CreateEffect, npc.ID, Name = "Blood", CurrentWorldName = npc.CurrentWorld.Name });
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateDataNpc, npc.ID, npc.Health, CurrentWorldName = npc.CurrentWorld.Name });
    }
}