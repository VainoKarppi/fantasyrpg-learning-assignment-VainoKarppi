using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GUI;

public enum NetworkMessageType {
    ServerShutdown,
    Connect,
    Disconnect,
    SendUpdateData,
    ReceiveUpdateData,
    InitialSync,
    ReceiveUpdateDataNpc,
    SendUpdateDataNpc,
    CreateNpc
    
}

public class NetworkMessage {
    public NetworkMessageType MessageType { get; set; }
    public dynamic? Data { get; set; }
}

public class NetworkUnit {
    public NetworkMessageType MessageType { get; set; }
    public int? ID { get; set; }
    public int? Health { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public string? Name { get; set; }
    public string? CurrentWorldName { get; set; }
}

static class MultiplayerClient {
    public static TcpClient? Client;
    private static NetworkStream? Stream;

    public static List<NetworkUnit> OtherPlayers = [];

    public static void Connect(string ipAddress, int port, Player player) {
        if (Client != null) throw new Exception("Already connected to server!");

        try {
            
            Client = new TcpClient();
            Client.Connect(ipAddress, port);
            Console.WriteLine("CLIENT: Connected to the server!");

            Stream = Client.GetStream();

            // Initialize Network Events handling
            new NetworkEventListener();

            // Start receiving data in a separate thread
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
            

            SendMessageAsync(new { MessageType = NetworkMessageType.Connect, player.Name, CurrentWorldName = player.CurrentWorld.Name, player.X, player.Y });
        } catch (Exception ex) {
            Console.WriteLine("Error connecting to server: " + ex.Message);
        }
    }

    public static void SendMessageAsync(object message) {
        // FIRE AND FORGET
        Task.Run(async () => {
            if (Client is null || Stream is null) return;

            try {
                var options = new JsonSerializerOptions {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                // Serialize the message to JSON
                string jsonMessage = JsonSerializer.Serialize(message, options);

                // Convert the JSON string to bytes
                byte[] data = Encoding.UTF8.GetBytes(jsonMessage);

                // Prefix the data with its length (4 bytes for an integer)
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                byte[] fullMessage = new byte[lengthPrefix.Length + data.Length];

                Buffer.BlockCopy(lengthPrefix, 0, fullMessage, 0, lengthPrefix.Length);
                Buffer.BlockCopy(data, 0, fullMessage, lengthPrefix.Length, data.Length);

                // Send the prefixed message
                await Stream.WriteAsync(fullMessage);
            } catch (Exception ex) {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        });
    }

    private static async void ReceiveMessages() {
        byte[] lengthBuffer = new byte[4];
        while (true) {
            try {
                if (Stream is null) return;

                // Read message lenght
                int bytesRead = await Stream.ReadAsync(lengthBuffer);
                if (bytesRead == 0) break;
                
                // Read JSON message from lenght
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] messageBuffer = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength) {
                    bytesRead = await Stream.ReadAsync(messageBuffer.AsMemory(totalBytesRead, messageLength - totalBytesRead));
                    if (bytesRead == 0) break;
                    
                    totalBytesRead += bytesRead;
                }

                string receivedMessage = Encoding.UTF8.GetString(messageBuffer);
                Console.WriteLine($"CLIENT: {receivedMessage}");

                NetworkUnit? unit = JsonSerializer.Deserialize<NetworkUnit>(receivedMessage);
                if (unit == null) continue;

                NetworkMessageType? method = unit.MessageType;
                if (method == null) continue;

                //--- ACCEPT SERVER CONNECTION ID
                if (method == NetworkMessageType.Connect) {
                    if (unit.ID == null) continue;
                    GameForm.player.ID = (int)unit.ID;
                    continue;
                }

                //--- SERVER SHUTDOWN
                if (method == NetworkMessageType.ServerShutdown) {
                    OtherPlayers.Clear();
                    GameForm.player.ID = -1;
                    return; // --> Disconnect
                }

                //--- UPDATE OTHER PLAYER
                if (method == NetworkMessageType.ReceiveUpdateData) {
                    if (unit.ID == null) continue;
                    
                    // Add to list if not found
                    if (OtherPlayers.FindIndex(p => p.ID == unit.ID) == -1) {
                        OtherPlayers.Add(unit);
                    };

                    NetworkUnit? playerToUpdate = OtherPlayers.FirstOrDefault(x => x.ID == unit.ID);
                    if (playerToUpdate == null) continue;

                    // TODO fix sync (optimize)
                    //if (!GameForm.player.CurrentWorld.Name.Equals(player.CurrentWorldName, StringComparison.CurrentCultureIgnoreCase)) continue;
                    
                    if (unit.Name != null) playerToUpdate.Name = unit.Name;
                    if (unit.CurrentWorldName != null) playerToUpdate.CurrentWorldName = unit.CurrentWorldName;
                    if (unit.X.HasValue) playerToUpdate.X = unit.X;
                    if (unit.Y.HasValue) playerToUpdate.Y = unit.Y;
                    if (unit.Health.HasValue) playerToUpdate.Health = unit.Health;

                    GameForm.RefreshPage();

                    continue;
                }

                //--- UPDATE NPCs
                if (method == NetworkMessageType.ReceiveUpdateDataNpc) {
                    foreach (World world in GameInstance.Worlds) {
                        // Find the NPC by ID within the world's NPCs
                        var npc = world.NPCs.FirstOrDefault(n => n.ID == unit.ID);
                        if (npc is null) continue;

                        if (unit.Health.HasValue) npc.Health = unit.Health.Value;
                        if (unit.X.HasValue) npc.X = unit.X.Value;
                        if (unit.Y.HasValue) npc.Y = unit.Y.Value;
                        if (unit.CurrentWorldName != null) npc.CurrentWorld = GameInstance.GetWorld(unit.CurrentWorldName);

                        // If player in same world, update screen
                        if (GameForm.player.CurrentWorld.Name.Equals(unit.CurrentWorldName, StringComparison.CurrentCultureIgnoreCase)) {
                            if (npc.Health < 0) npc.KillNPC();
                            GameForm.RefreshPage();
                        }

                        break;
                    }
                    continue;
                }


                //--- CREATE NPC
                if (method == NetworkMessageType.CreateNpc) {
                    if (unit.CurrentWorldName is null || unit.Name is null || unit.X is null || unit.Y is null || unit.ID is null) continue;

                    World spawnWorld = GameInstance.GetWorld(unit.CurrentWorldName);
                    NpcCharacter createdNpc = NpcCharacter.CreateNPC(unit.Name, spawnWorld, ((int)unit.X, (int)unit.Y), unit.Health);
                    createdNpc.ID = (int)unit.ID;

                    // If player in same world, update screen
                    if (GameForm.player.CurrentWorld.Name.Equals(unit.CurrentWorldName, StringComparison.CurrentCultureIgnoreCase)) {
                        GameForm.RefreshPage();
                    }
                }

            } catch (Exception ex) {
                Console.WriteLine("Error receiving data: " + ex.Message);
                break;
            }
        }

        Disconnect();
    }

    public static void Disconnect() {
        Stream?.Close();
        Client?.Close();
        Client = null;

        Console.WriteLine("Disconnected from the server.");
    }
}
