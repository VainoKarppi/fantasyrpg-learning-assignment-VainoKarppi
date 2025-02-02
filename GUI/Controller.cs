namespace GUI;


public partial class GameForm : Form {

    private readonly HashSet<Keys> pressedKeys = new HashSet<Keys>();

    // Handle player movement based on keypress
    private void GameForm_KeyDown(object? sender, KeyEventArgs e) {
        pressedKeys.Add(e.KeyCode);

        ProcessCommand();

        // Handle movement
        ProcessMovement();

        // Check for the bounds of the buildings in the world
        CheckPlayerBuildingWorldBounds();

        // Check for world boundary collision
        CheckPlayerWorldBounds();

        // Check for collisions with NPCs
        CheckCollisionsInteraction();
    }

    private void GameForm_KeyUp(object? sender, KeyEventArgs e) {
        pressedKeys.Remove(e.KeyCode);
    }



    private void ProcessCommand() {
        if (pressedKeys.Contains(Keys.H)) {
            pressedKeys.Clear();
            ShowHelp();
        }
        if (pressedKeys.Contains(Keys.C)) {
            pressedKeys.Clear();
            ChangeWeapon();
        }
        if (pressedKeys.Contains(Keys.K)) {
            pressedKeys.Clear();
            ChangeArmor();
        }
        if (pressedKeys.Contains(Keys.P)) {
            pressedKeys.Clear();
            UsePotion();
        }
        if (pressedKeys.Contains(Keys.Q)) {
            pressedKeys.Clear();
            ChangeQuest();
        }
        if (pressedKeys.Contains(Keys.N)) {
            pressedKeys.Clear();
            ShowStats();
        }

        // Attack
        if (pressedKeys.Contains(Keys.Space)) {
            pressedKeys.Remove(Keys.Space);
            if (Player.CurrentWeapon == null || Player.CurrentState == Character.State.Attacking) return;

            NpcCharacter? npc = Player.GetNearestNpcTarget();

            // Create effect based on current weapon type
            var effectName = Player.CurrentWeapon.Type switch {
                ItemType.MeleeWeapon => "Melee",
                ItemType.MageWeapon => "Mage",
                ItemType.RangedWeapon => "Ranged",
                _ => throw new InvalidOperationException("Unknown weapon type")
            };

            // If using mage check if has enough mana
            if (!Player.HasEnoughMana()) return;

            var effectType = Player.CurrentWeapon.Type switch {
                ItemType.MeleeWeapon => Effect.EffectType.Melee,
                ItemType.MageWeapon => Effect.EffectType.Mage,
                ItemType.RangedWeapon => Effect.EffectType.Ranged,
                _ => throw new InvalidOperationException("Unknown weapon type")
            };

            MultiplayerClient.SendMessageAsync(new {
                MessageType = NetworkMessageType.CreateEffect,
                Player.ID,
                TargetID = npc?.ID,
                Name = effectName,
                Range = Player.CurrentWeapon?.Range ?? 40,
                CurrentWorldName = Player.CurrentWorld.Name
            });

            if (Player.CanAttack(npc)) {
                new Effect(Player, npc, effectType);
                Player.Actions.Attack(npc);
            } else {
                new Effect(Player, null, effectType);
            }
        }

        
    }

    private async void ProcessMovement() {
        const float stepSize = 1f;

        for (int i = 0; i < MoveStep; i++) {
            float deltaX = 0;
            float deltaY = 0;
        
            if (pressedKeys.Contains(Keys.W)) deltaY -= stepSize;
            if (pressedKeys.Contains(Keys.A)) deltaX -= stepSize;
            if (pressedKeys.Contains(Keys.S)) deltaY += stepSize;
            if (pressedKeys.Contains(Keys.D)) deltaX += stepSize;

            // Normalize diagonal movement to prevent speed boost
            if (deltaX != 0 && deltaY != 0) {
                float normalizationFactor = (float)(1 / Math.Sqrt(2));
                deltaX *= normalizationFactor;
                deltaY *= normalizationFactor;
            }

            Player.X += (int)Math.Round(deltaX);
            Player.Y += (int)Math.Round(deltaY);

            // Redraw the screen
            Invalidate();

            await Task.Delay(10);
        }
    }


    

    private static void CheckPlayerBuildingWorldBounds() {
        // Prevent player from moving into buildings
        foreach (World.Building building in Player.CurrentWorld!.Buildings) {
            if (Player.Bounds.IntersectsWith(new Rectangle(building.X, building.Y, building.Width, building.Height))) {
                // Prevent player from moving further right if colliding with the building
                if (Player.X + Player.Width > building.X && Player.X < building.X) {
                    Player.X = building.X - Player.Width; // Stop player from going past the building's left side
                }
                // Prevent player from moving further left if colliding with the building
                if (Player.X < building.X + building.Width && Player.X + Player.Width > building.X + building.Width) {
                    Player.X = building.X + building.Width; // Stop player from going past the building's right side
                }

                // Prevent player from moving further down if colliding with the building
                if (Player.Y + Player.Height > building.Y && Player.Y < building.Y) {
                    Player.Y = building.Y - Player.Height; // Stop player from going past the building's top side
                }

                // Prevent player from moving further up if colliding with the building
                if (Player.Y < building.Y + building.Height && Player.Y + Player.Height > building.Y + building.Height) {
                    Player.Y = building.Y + building.Height; // Stop player from going past the building's bottom side
                }

                // Prevent the player from moving closer to the building
                break; // Exit after the first building collision to prevent multiple checks for the same frame
            }
        }
    }

    // Ensure the player does not move outside world boundaries
    private static int lastX = 0;
    private static int lastY = 0;
    private static void CheckPlayerWorldBounds() {

        // Move player From Left to Right World
        if (Player.X + Player.Width >= ScreenWidth - 20) {
            int currentWorldIndex = GameInstance.Worlds.IndexOf(Player.CurrentWorld!);

            if (currentWorldIndex + 1 < GameInstance.Worlds.Count) {
                Player.ChangeWorld(GameInstance.Worlds[currentWorldIndex + 1]);
            }
        }

        // Move player From Right to Left World
        if (Player.X + Player.Width <= 20) {
            int currentWorldIndex = GameInstance.Worlds.IndexOf(Player.CurrentWorld!);

            if (currentWorldIndex - 1 >= 0) {
                Player.ChangeWorld(GameInstance.Worlds[currentWorldIndex - 1]);
                
            }
        }

        // Clamp player's position within world boundaries
        Player.X = Math.Clamp(Player.X, 0, WorldWidth - Player.Width);
        Player.Y = Math.Clamp(Player.Y, TopBarHeight, WorldHeight - Player.Height); // Add some extra at the bottom, for stats display

        // Send update location
        if (Player.X != lastX || Player.Y != lastY) {
            MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, Player.ID, Player.X, Player.Y, CurrentWorldName = Player.CurrentWorld!.Name });
            lastX = Player.X;
            lastY = Player.Y;
        }
        
    }

    // Check if player collides with any NPCs
    private void CheckCollisionsInteraction() {

        // Check NPC collisions
        foreach (NpcCharacter npc in Player.CurrentWorld!.NPCs) {
            if (Player.Bounds.IntersectsWith(npc.Bounds)) {
                // Show dialog if collision detected
                pressedKeys.Clear();

                break;
            }
        }

        // Check collisions with Buildings
        foreach (World.Building building in Player.CurrentWorld!.Buildings) {
            if (Player.Bounds.IntersectsWith(new Rectangle(building.X - 5, building.Y - 5, building.Width + 10, building.Height + 10))) {

                pressedKeys.Clear();

                if (building.BuildingType == World.BuildingType.Shop) OpenShopMenu();
                if (building.Name == "Quest 1") MassacareQuest();
                
                break;
            }
        }
    }
}