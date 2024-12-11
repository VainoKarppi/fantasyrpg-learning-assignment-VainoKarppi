using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GUI;

public enum NetworkMessageType {
    Connect,
    Disconnect,
    SendUpdateData,
    ReceiveUpdateData,
    InitialSync,
    ReceiveUpdateDataNpc,
    SendUpdateDataNpc,
    CreateNpc,
    CreateEffect
}



public class NetworkMessage {
    public NetworkMessageType MessageType { get; set; }
    public int? ID { get; set; }
    public int? Range { get; set;}
    public int? Health { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public string? Name { get; set; }
    public string? Version { get; set; }
    public string? CurrentWorldName { get; set; }
    public int? TargetID { get; set; }
}

static class MultiplayerClient {
    public static Character? GetCharacterByID(int? id) {
        if (id is null) return null;

        // TODO also return player
        NetworkMessage? foundCharcater = OtherPlayers.FirstOrDefault(p => p.ID == id);
        if (foundCharcater != null) {
            // Transform NetworkMessage to Player object
            Character character = new Player{ ID = (int)id!, X = (int)foundCharcater.X!, Y = (int)foundCharcater.Y! };
            return character;
        }


        foreach (World world in GameInstance.Worlds) {
            foreach (NpcCharacter npc in world.NPCs) {
                if (npc.ID == id) return npc;
            }
        }

        return null;
    }
    //--- EVENTS
    private static NetworkEventListener? EventListeners;
    public static event Action<TcpClient, Player>? OnConnectStart;
    public static event Action<TcpClient, Player, Exception?>? OnConnectEnd;
    public static event Action<bool>? OnDisconnect;
    

    public static TcpClient? Client;
    private static NetworkStream? Stream;

    public static List<NetworkMessage> OtherPlayers = [];

    public static void Connect(string ipAddress, int port, Player player) {
        if (player is null) throw new Exception("No player object found!");
        if (Client != null) throw new Exception("Already connected to server!");

        try {
            Client = new TcpClient();
            Client.Connect(ipAddress, port);

            OnConnectStart?.InvokeFireAndForget(Client!, player);
            
            Stream = Client.GetStream();

            // Initialize event listeners
            if (EventListeners is null) EventListeners = new NetworkEventListener();
            
            // Start receiving data in a separate thread
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
            
            Assembly? assembly = Assembly.GetExecutingAssembly();
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;

            SendMessageAsync(new { MessageType = NetworkMessageType.Connect, Version = fileVersion, player.Name, player.Health, CurrentWorldName = player.CurrentWorld!.Name, player.X, player.Y });
        } catch (Exception ex) {

            // TODO not triggering for some reason?
            OnConnectEnd?.InvokeFireAndForget(Client!, player, ex);

            Disconnect();
            throw;
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
                Console.WriteLine($"CLIENT: Error sending message: {ex.Message}");
            }
        });
    }

    private static async void ReceiveMessages() {
        Exception? exception = null;
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

                NetworkMessage? message = JsonSerializer.Deserialize<NetworkMessage>(receivedMessage);
                if (message == null) continue;

                NetworkMessageType? method = message.MessageType;
                if (method == null) continue;

                //--- ACCEPT SERVER CONNECTION ID
                if (method == NetworkMessageType.Connect) {
                    if (message.ID == null) throw new Exception("Invalid version!");

                    GameForm.Player.ID = (int)message.ID;

                    OnConnectEnd?.InvokeFireAndForget(Client!, GameForm.Player, null);
                    continue;
                }

                //--- CLIENT DISCONNECT
                if (method == NetworkMessageType.Disconnect) {
                    if (message.ID == null) continue;

                    OtherPlayers.RemoveAll(p => p.ID == message.ID); // Remove other player from list

                    GameForm.RefreshPage();
                    continue;
                }


                //--- UPDATE OTHER PLAYER
                if (method == NetworkMessageType.ReceiveUpdateData) {
                    if (message.ID == null) continue;
                    
                    // Add to list if not found
                    if (OtherPlayers.FindIndex(p => p.ID == message.ID) == -1) OtherPlayers.Add(message);

                    NetworkMessage? playerToUpdate = OtherPlayers.FirstOrDefault(x => x.ID == message.ID);
                    if (playerToUpdate == null) continue;

                    if (message.Name != null) playerToUpdate.Name = message.Name;
                    if (message.CurrentWorldName != null) playerToUpdate.CurrentWorldName = message.CurrentWorldName;
                    if (message.X.HasValue) playerToUpdate.X = message.X;
                    if (message.Y.HasValue) playerToUpdate.Y = message.Y;
                    if (message.Health.HasValue) playerToUpdate.Health = message.Health;

                    GameForm.RefreshPage();
                    continue;
                }

                //--- UPDATE NPCs
                if (method == NetworkMessageType.ReceiveUpdateDataNpc) {
                    foreach (World world in GameInstance.Worlds) {
                        // Find the NPC by ID within the world's NPCs
                        var npc = world.NPCs.FirstOrDefault(n => n.ID == message.ID);
                        if (npc is null) continue;

                        if (message.Health.HasValue) npc.Health = message.Health.Value;
                        if (message.X.HasValue) npc.X = message.X.Value;
                        if (message.Y.HasValue) npc.Y = message.Y.Value;
                        if (message.CurrentWorldName != null) npc.CurrentWorld = GameInstance.GetWorld(message.CurrentWorldName);

                        // If player in same world, update screen
                        if (GameForm.Player.CurrentWorld!.Name.Equals(message.CurrentWorldName, StringComparison.CurrentCultureIgnoreCase)) {
                            if (npc.Health < 0) npc.Kill();
                            GameForm.RefreshPage();
                        }

                        break;
                    }
                    continue;
                }


                //--- CREATE NPC
                if (method == NetworkMessageType.CreateNpc) {
                    if (message.CurrentWorldName is null || message.Name is null || message.X is null || message.Y is null || message.ID is null) continue;

                    World spawnWorld = GameInstance.GetWorld(message.CurrentWorldName);
                    NpcCharacter createdNpc = NpcCharacter.CreateNPC(message.Name, spawnWorld, ((int)message.X, (int)message.Y), message.Health);
                    createdNpc.ID = (int)message.ID;

                    // If player in same world, update screen
                    if (GameForm.Player.CurrentWorld!.Name.Equals(message.CurrentWorldName, StringComparison.CurrentCultureIgnoreCase)) {
                        GameForm.RefreshPage();
                    }
                }

                //--- CREATE EFFECT
                if (method == NetworkMessageType.CreateEffect) {
                    if (message.CurrentWorldName is null || message.Name is null || message.ID is null) continue;

                    


                    // If player in same world, create effect
                    if (GameForm.Player.CurrentWorld!.Name.Equals(message.CurrentWorldName, StringComparison.CurrentCultureIgnoreCase)) {
                        Character? effectStartCharacter = GetCharacterByID(message.ID);
                        if (effectStartCharacter == null) return;

                        // Check if found
                        Character? endCharacter = endCharacter = GetCharacterByID(message.TargetID);

                        _ = message.Name.ToLower() switch {
                            "melee" => new Effect(effectStartCharacter, null, Effect.EffectType.Melee, message.Range),
                            "mage" => new Effect(effectStartCharacter, endCharacter, Effect.EffectType.Mage, message.Range),
                            "ranged" => new Effect(effectStartCharacter, endCharacter, Effect.EffectType.Ranged, message.Range),
                            "blood" => new Effect(effectStartCharacter, null, Effect.EffectType.Blood, message.Range),
                            "potion" => new Effect(effectStartCharacter, null, Effect.EffectType.Potion, message.Range),
                            _ => throw new ArgumentException("Invalid name type"),
                        };
                    }
                }

            } catch (Exception ex) {
                exception = ex;
                break;
            }
        }

        Disconnect(true, exception);
    }

    public static void Disconnect(bool serverShutdown = false, Exception? exception = null) {
        OtherPlayers.Clear();
        GameForm.Player.ID = -1;

        Stream?.Close();
        Client?.Close();
        Client = null;
        Stream = null;

        if (exception != null) {
            MessageBox.Show(exception.Message);
        } else {
            if (serverShutdown) MessageBox.Show("Server shutdown!");
            
        }

        Console.WriteLine(serverShutdown ? "CLIENT: Server shutdown!" : "CLIENT: Disconnected from the server.");

        OnDisconnect?.InvokeFireAndForget(serverShutdown);

        GameForm.RefreshPage();
    }
}
