using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Text.Json.Serialization;

class MultiplayerServer {
    public static TcpListener? Server;
    private static bool ServerRunning;

    private static TcpClient? HostClient;

    public static void Start(string ipAddress, int port) {
        if (ServerRunning) throw new Exception("Server already running!");
        if (Server == null) Server = new TcpListener(IPAddress.Parse(ipAddress), port);

        Server.Start();
        Console.WriteLine("Server started. Waiting for connections...");

        ServerRunning = true;

        while (ServerRunning) {
            try {
                TcpClient client = Server.AcceptTcpClient(); // Accept an incoming connection
                Console.WriteLine("SERVER: Client connected!");

                if (HostClient is null) HostClient = client;

                // Handle the client in a separate thread
                Thread clientThread = new Thread(() => HandleClientAsync(client));
                clientThread.Start();
            } catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    private static readonly List<NetworkUnit> Players = [];
    private static readonly List<NetworkStream> Streams = [];

    private static void SendMessageAsync(NetworkStream stream, object message) {
        Task.Run(async () => {
            if (stream is null) return;

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
                await stream.WriteAsync(fullMessage);
            } catch (Exception ex) {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        });
    }

    private static int GetID() {
        return new Random().Next(999999);
        // TODO make sure its unique
    }


    private static async Task HandleClientAsync(TcpClient client) {
        NetworkStream stream = client.GetStream();
        Streams.Add(stream);

        byte[] lengthBuffer = new byte[4];
        while (true) {
            try {
                // Read message lenght
                int bytesRead = await stream.ReadAsync(lengthBuffer);
                if (bytesRead == 0) {
                    Streams.Remove(stream);
                    // TODO REMOVE FROM LIST and notify other clients
                    break;
                }

                // Read JSON message from lenght
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] messageBuffer = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength) {
                    bytesRead = await stream.ReadAsync(messageBuffer.AsMemory(totalBytesRead, messageLength - totalBytesRead));
                    if (bytesRead == 0) {
                        Streams.Remove(stream);
                        // TODO REMOVE FROM LIST and notify other clients
                        break;
                    }
                    totalBytesRead += bytesRead;
                }

                string receivedMessage = Encoding.UTF8.GetString(messageBuffer);
                
                NetworkUnit? unit = JsonSerializer.Deserialize<NetworkUnit>(receivedMessage);
                if (unit == null) continue;

                NetworkMessageType? method = unit.MessageType;
                if (method == null) continue;

                //--- ACCEPT CONNECTION TO SERVER
                if (method == NetworkMessageType.Connect) {
                    int newId = GetID();
                    
                    Console.WriteLine($"PLAYER: {unit.Name} Connected to server! ID: {newId}");

                    // Send client id back to connected client
                    SendMessageAsync(stream, new { MessageType = NetworkMessageType.Connect, ID = newId });
                    
                    // Send sync data of other players
                    foreach (NetworkUnit otherPlayer in Players) {
                        SendMessageAsync(stream, new { MessageType = NetworkMessageType.ReceiveUpdateData, otherPlayer.Name, otherPlayer.ID, otherPlayer.X, otherPlayer.Y, otherPlayer.CurrentWorldName });
                    }
                
                    unit.ID = newId;
                    Players.Add(unit);

                    // Send other players data
                    unit.MessageType = NetworkMessageType.ReceiveUpdateData;
                    foreach (NetworkStream otherPlayerStream in Streams) {
                        if (otherPlayerStream == stream) continue; // Dont send sync data back to connected client

                        SendMessageAsync(otherPlayerStream, unit);
                    }

                    // Send NPC sync data
                    if (HostClient != client) {
                        foreach (World world in GameInstance.Worlds) {
                            foreach (NpcCharacter npc in world.NPCs) {
                                npc.ID = GetID();
                                SendMessageAsync(stream, new { MessageType = NetworkMessageType.CreateNpc, npc.ID, npc.Health, npc.X, npc.Y, npc.Name, CurrentWorldName = npc.CurrentWorld.Name });
                            }
                        }
                    }
                }

                //--- RECEIVE PLAYER DATA UPDATED
                if (method == NetworkMessageType.SendUpdateData) {
                    NetworkUnit? player = Players.SingleOrDefault(x => x.ID == unit.ID);
                    if (player is null) continue;

                    unit.MessageType = NetworkMessageType.ReceiveUpdateData;

                    if (unit.Name != null) player.Name = unit.Name;
                    if (unit.X.HasValue) player.X = unit.X;
                    if (unit.Y.HasValue) player.Y = unit.Y;
                    if (unit.CurrentWorldName != null) player.CurrentWorldName = unit.CurrentWorldName;

                    foreach (NetworkStream otherPlayerStream in Streams) {
                        // Dont send data back to sender
                        if (otherPlayerStream == stream) continue;

                        SendMessageAsync(otherPlayerStream, unit);
                    }
                }

                //--- RECEIVE NPC DATA UPDATED -> Forward to other clients
                if (method == NetworkMessageType.SendUpdateDataNpc) {
                    NpcCharacter? npc = null;
                    foreach (World world in GameInstance.Worlds) {
                        npc = world.NPCs.SingleOrDefault(n => n.ID == unit.ID);
                        if (npc != null) break;
                    }
                    if (npc is null) continue;

                    unit.MessageType = NetworkMessageType.ReceiveUpdateDataNpc;
                    if (unit.X.HasValue) npc.X = (int)unit.X;
                    if (unit.Y.HasValue) npc.X = (int)unit.Y;
                    if (unit.Health.HasValue) npc.Health = (int)unit.Health;
                    // TODO update current world

                    foreach (NetworkStream otherPlayerStream in Streams) {
                        // Dont send data back to sender
                        if (otherPlayerStream == stream) continue;

                        SendMessageAsync(otherPlayerStream, unit);
                    }
                }

    

                // TODO FORWARD to other clients
                
            } catch (Exception ex) {
                Console.WriteLine("Error handling client: " + ex.Message);
                break;
            }
        }

        client.Close();
    }

    public static void Stop() {
        ServerRunning = false;
        Server = null;
        Console.WriteLine("Server stopped.");
    }
}
