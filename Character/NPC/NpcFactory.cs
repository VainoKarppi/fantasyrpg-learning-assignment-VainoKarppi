using System;
using System.Threading;
using System.Threading.Tasks;
using GUI;

public class NpcFactory {
    public static event Action<NpcCharacter>? OnNpcMoved;
    public static class NpcCounts {
        public static int WarriorCount = 0;
        public static int MageCount = 0;
        public static int RangerCount = 0;
    }

    private static readonly object _lock = new object();
    private static bool _isRunning = false;

    public static void Start() {
        if (_isRunning) return;

        _isRunning = true;
        Task.Run(RunRespawnFactory);
        Task.Run(RunMovementFactory);
    }


    private static void RunRespawnFactory() {
        
        // Spawn initial NPCs and if some are missing
        foreach (var world in Config.Instance.Worlds) {
            World spawnWorld = GameInstance.GetWorld(world.Name);

            var groups = world.Enemies.GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());

            // Check if NPC is missing and spawn it
            foreach (var group in groups) {
                int currentCount = spawnWorld.NPCs.Count(npc => npc.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase));
                int requiredCount = group.Value;

                if (currentCount < requiredCount) {
                    int toSpawn = requiredCount - currentCount;
                    for (int i = 0; i < toSpawn; i++) {
                        var position = spawnWorld.FindSafeSpace();
                        NpcCharacter.CreateNPC(group.Key, spawnWorld, position);
                    }
                }
            }
        }
        
        NpcCharacter.OnNpcKilled += async (npc, killer) => {
            
            await Task.Delay(npc.RespawnTime * 1000); // Wait for RespawnTime before respawning new similiar NPC

            lock (_lock) {
                if (!_isRunning) return;

                // Create a new NPC and add it to the world
                World spawnWorld = npc.CurrentWorld;
                var position = spawnWorld.FindSafeSpace();
                NpcCharacter.CreateNPC(npc.Name, spawnWorld, position);

                GameForm.RefreshPage();
            }
        };
    }

    private static async Task RunMovementFactory() {
        Random random = new Random();
        Console.WriteLine($"NPC Movement Factory Started!");

        // Wait until player is loaded
        while (_isRunning && GameForm.Player == null) await Task.Delay(500);

        while (_isRunning) {
            foreach (var world in GameInstance.Worlds) {
                // Move only the npcs in players world
                if (GameForm.Player == null || GameForm.Player.CurrentWorld != world) continue;

                foreach (var npc in world.NPCs) {
                    if (npc.CurrentState != Character.State.Idle) continue; // Skip if NPC is already doing something else
                    if (random.Next(100) < 60) Task.Run(() => MoveNpc(npc)); // chance to move NPC
                }
            }

            await Task.Delay(400); // Move NPCs every 1 second
        }
    }

    private static async Task MoveNpc(NpcCharacter npc) {
        npc.CurrentState = Character.State.Moving;

        Random random = new Random();
        int direction = random.Next(8);

        int stepSize = 1;
        int[] possibleSteps = { 5, 10, 15, 20, 25, 30 };
        int totalSteps = possibleSteps[random.Next(possibleSteps.Length)];

        for (int i = 0; i < totalSteps; i++) {
            int newX = npc.X;
            int newY = npc.Y;

            switch (direction) {
                case 0: // Move up
                    newY -= stepSize;
                    break;
                case 1: // Move down
                    newY += stepSize;
                    break;
                case 2: // Move left
                    newX -= stepSize;
                    break;
                case 3: // Move right
                    newX += stepSize;
                    break;
                case 4: // Move up-left
                    newX -= stepSize;
                    newY -= stepSize;
                    break;
                case 5: // Move up-right
                    newX += stepSize;
                    newY -= stepSize;
                    break;
                case 6: // Move down-left
                    newX -= stepSize;
                    newY += stepSize;
                    break;
                case 7: // Move down-right
                    newX += stepSize;
                    newY += stepSize;
                    break;
            }

            if (!IsCollision(newX, newY, npc)) {
                npc.X = newX;
                npc.Y = newY;

                // Ensure NPC stays within world bounds with a 20px buffer
                npc.X = Math.Clamp(npc.X, 80, GameForm.WorldWidth - npc.Width - 80);
                npc.Y = Math.Clamp(npc.Y, GameForm.TopBarHeight + 20, GameForm.WorldHeight - npc.Height - 80);
                
                if (GameForm.Form != null) GameForm.RefreshPage();
            }

            await Task.Delay(50);
        }
        npc.CurrentState = Character.State.Idle;

        OnNpcMoved.InvokeFireAndForget(npc);
    }
    
    private static bool IsCollision(int x, int y, NpcCharacter npc) {
        // Check for collision with game borders
        if (x < 80 || x > GameForm.WorldWidth - npc.Width - 80 || y < GameForm.TopBarHeight + 20 || y > GameForm.WorldHeight - npc.Height - 80) {
            return true;
        }

        // Check for collision with other NPCs
        foreach (var otherNpc in npc.CurrentWorld.NPCs) {
            if (otherNpc != npc && IsNear(x, y, otherNpc.X, otherNpc.Y, npc.Width + 10, npc.Height + 10)) {
                return true;
            }
        }

        // Check for collision with players
        foreach (var player in GameInstance.Players) {
            if (IsNear(x, y, player.X, player.Y, npc.Width + 10, npc.Height + 10)) {
                return true;
            }
        }

        return false;
    }

    private static bool IsNear(int x1, int y1, int x2, int y2, int width, int height) {
        return Math.Abs(x1 - x2) < width && Math.Abs(y1 - y2) < height;
    }


    public static void Stop() {
        lock (_lock) {
            _isRunning = false;
        }
    }
}