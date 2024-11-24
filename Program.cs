using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace GUI;



public partial class GameForm : Form {
    public const bool DEBUG = true;


    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    private readonly Player player;

    private const int MoveStep = 5;

    public const int ScreenWidth = 1000;
    public const int ScreenHeight = 700 + StatsBarHeight;
    public const int StatsBarHeight = 200;

    public GameForm() {
        
        SuspendLayout();
        ClientSize = new Size(ScreenWidth, ScreenHeight);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Text = "Game Window";
        ResumeLayout(false);
        KeyPreview = true;
        DoubleBuffered = true; // To avoid flickering

        InitializeButtons();

        Width = ScreenWidth;
        Height = ScreenHeight;

        // Initialize the player
        player = new Player(GameInstance.GetWorld("Home"), "test name"); // TODO name
        player.Money = 800;
        player.AddItem(new ItemDrop.Gems.Sapphire());


        player.Health = 50;


        Paint += GameForm_Paint;
        KeyDown += GameForm_KeyDown;
        KeyUp += GameForm_KeyUp;
        
    }

    private HashSet<Keys> pressedKeys = new HashSet<Keys>();

    // Handle player movement based on keypress
    private void GameForm_KeyDown(object? sender, KeyEventArgs e) {
        pressedKeys.Add(e.KeyCode);

        ProcessCommand();

        // Handle movement
        ProcessMovement();

        // Check for world boundary collision
        CheckWorldBounds();

        // Check for collisions with NPCs
        CheckCollisionsInteraction();

        // Redraw the screen
        Invalidate();
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
    }

    private void ProcessMovement() {
        if (pressedKeys.Contains(Keys.W)) {
            player.Y -= MoveStep;
        }
        if (pressedKeys.Contains(Keys.A)) {
            player.X -= MoveStep;
        }
        if (pressedKeys.Contains(Keys.S)) {
            player.Y += MoveStep;
        }
        if (pressedKeys.Contains(Keys.D)) {  
            player.X += MoveStep;
            TriggerDamageEffect();
        }
    }

    private async Task TriggerDamageEffect(bool shakeEffect = true) {

        Point playerLocation = new Point(player.X + player.Height / 2, player.Y + player.Width / 2);  // Placeholder for player position; replace with actual position

        // Trigger the blood splash effect at the player's location
        TriggerBloodSplashEffect(playerLocation);

        if (!shakeEffect) return;

        // Shake the form by briefly changing its location
        Point originalLocation = Location;

        Random rand = new Random();
        int shakeDuration = 10; // Duration in milliseconds
        int shakeAmount = 10; // The amount of shake (in pixels)

        // Shake the screen 5 times (for example)
        for (int i = 0; i < 5; i++) {
            int offsetX = rand.Next(-shakeAmount, shakeAmount + 1);
            int offsetY = rand.Next(-shakeAmount, shakeAmount + 1);

            Location = new Point(originalLocation.X + offsetX, originalLocation.Y + offsetY);
            await Task.Delay(shakeDuration);  // Brief delay to create the shake effect
        }

        // Return the form to its original position
        Location = originalLocation;
    }

    private async Task TriggerBloodSplashEffect(Point playerLocation) {
        // Set up parameters for the blood splash
        int splashSize = player.Width + player.Height + 5;  // Size of the blood splatter (in pixels)
        int splashAlpha = 220;  // Initial alpha (partially transparent)

        // Create a new PictureBox to draw the blood splash
        PictureBox bloodSplash = new PictureBox {
            Width = splashSize,
            Height = splashSize,
            Location = new Point(playerLocation.X - splashSize / 2, playerLocation.Y - splashSize / 2),  // Center it on the player location
            BackColor = Color.Transparent
        };

        // Create a Bitmap for the blood splash
        Bitmap splashBitmap = new Bitmap(splashSize, splashSize);
        bloodSplash.Image = splashBitmap;
        Controls.Add(bloodSplash);
        bloodSplash.BringToFront();

        // Fade out the blood splash
        for (int alpha = splashAlpha; alpha >= 0; alpha -= 10) {
            using (Graphics g = Graphics.FromImage(splashBitmap)) {
                g.Clear(Color.Transparent);  // Clear the previous drawing

                // Redraw the square with reduced alpha
                using Brush squareBrush = new SolidBrush(Color.FromArgb(alpha, Color.DarkRed));
                int squareSize = splashSize / 2;
                int squareOffset = (splashSize - squareSize) / 2;
                g.FillRectangle(squareBrush, squareOffset, squareOffset, squareSize, squareSize);
            }

            bloodSplash.Refresh();  // Refresh to show the updated image
            await Task.Delay(40);  // Smooth fade-out effect
        }

        // Remove the blood splash
        Controls.Remove(bloodSplash);
        bloodSplash.Dispose();
    }



    // Ensure the player does not move outside world boundaries
    private void CheckWorldBounds() {
        int worldWidth = ScreenWidth;
        int worldHeight = ScreenHeight - StatsBarHeight;

        // From Left to Right
        if (player.X + player.Width >= ScreenWidth - 20) {
            int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);

            if (currentWorldIndex + 1 < GameInstance.Worlds.Count) {
                player.ChangeWorld(GameInstance.Worlds[currentWorldIndex + 1]);
            }
        }

        // From Right to Left
        if (player.X + player.Width <= 20) {
            int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);

            if (currentWorldIndex - 1 >= 0) {
                player.ChangeWorld(GameInstance.Worlds[currentWorldIndex - 1]);
            }
        }

        // Prevent player from moving into buildings
        foreach (World.Building building in player.CurrentWorld.Buildings) {
            if (player.Bounds.IntersectsWith(new Rectangle(building.X, building.Y, building.Width, building.Height))) {
                // Prevent player from moving further right if colliding with the building
                if (player.X + player.Width > building.X && player.X < building.X) {
                    player.X = building.X - player.Width; // Stop player from going past the building's left side
                }
                // Prevent player from moving further left if colliding with the building
                if (player.X < building.X + building.Width && player.X + player.Width > building.X + building.Width) {
                    player.X = building.X + building.Width; // Stop player from going past the building's right side
                }

                // Prevent player from moving further down if colliding with the building
                if (player.Y + player.Height > building.Y && player.Y < building.Y) {
                    player.Y = building.Y - player.Height; // Stop player from going past the building's top side
                }

                // Prevent player from moving further up if colliding with the building
                if (player.Y < building.Y + building.Height && player.Y + player.Height > building.Y + building.Height) {
                    player.Y = building.Y + building.Height; // Stop player from going past the building's bottom side
                }

                // Prevent the player from moving closer to the building
                break; // Exit after the first building collision to prevent multiple checks for the same frame
            }
        }

        // Clamp player's position within world boundaries
        player.X = Math.Clamp(player.X, 0, worldWidth - player.Width * 2);
        player.Y = Math.Clamp(player.Y, 40, worldHeight - player.Height * 3); // Add some extra at the bottom, for stats display
    }

    // Check if player collides with any NPCs
    private void CheckCollisionsInteraction() {
        foreach (NpcCharacter npc in player.CurrentWorld.NPCs) {
            if (player.Bounds.IntersectsWith(npc.Bounds)) {
                // Show dialog if collision detected
                pressedKeys.Clear();
                ShowNpcDialog();
                break;
            }
        }

        // Check collisions with Buildings
        foreach (World.Building building in player.CurrentWorld.Buildings) {
            if (player.Bounds.IntersectsWith(new Rectangle(building.X - 5, building.Y - 5, building.Width + 10, building.Height + 10))) {

                // Show dialog or take action if collision with a building is detected
                pressedKeys.Clear();

                if (building.BuildingType == World.BuildingType.Shop) OpenShopMenu();
                if (building.Name == "Butcher") StartQuest();
                
                break;
            }
        }
    }

    



    // Show the dialog to ask whether to attack or flee
    private void ShowNpcDialog() {
        var result = MessageBox.Show("You have encountered an NPC! Do you want to attack or flee?", 
                                     "NPC Encounter", 
                                     MessageBoxButtons.YesNo);

        // Yes means attack, No means flee
        if (result == DialogResult.Yes) {
            Attack();
        } else {
            Flee();
        }
    }



    // Handle the attack action
    private void Attack() {
        TriggerDamageEffect(true);
        MessageBox.Show("You chose to attack the NPC!");
        // You can add attack logic here, like reducing NPC health, etc.
    }

    // Handle the flee action
    private void Flee() {
        MessageBox.Show("You chose to flee from the NPC!");
        // Flee logic: Move player away from the NPC
        player.X += 50; // Example flee action: Move the player to the right
        player.Y += 50; // Move player downward
    }


    

    private void GameForm_Paint(object? sender, PaintEventArgs e) {
        var g = e.Graphics;

        // Draw a grey background for the whole screen along the x-axis, and 40 pixels in height on the y-axis
        using (Brush backgroundBrush = new SolidBrush(Color.LightGray)) {
            // Fill a rectangle across the top of the screen
            g.FillRectangle(backgroundBrush, 0, 0, Width, 40); // Full width, 40px height
        }

        // Draw current world name at the center of the grey background
        string worldName = player.CurrentWorld.Name;
        using (Font font = new Font("Arial", 16, FontStyle.Bold)) // Adjust font size and style
        using (Brush brush = new SolidBrush(Color.Black)) {
            // Calculate position to center the text horizontally
            SizeF textSize = g.MeasureString(worldName, font);
            float textX = (Width - textSize.Width) / 2; // Center horizontally
            float textY = (40 - textSize.Height) / 2; // Center vertically within the 40px height

            // Draw the text on top of the grey background
            g.DrawString(worldName, font, brush, textX, textY);
        }


        // Get Left and Right worlds (check if exists)
        int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);

        World? nextWorld = null;
        World? previousWorld = null;

        // Draw right side world enter text
        if (currentWorldIndex + 1 < GameInstance.Worlds.Count) {
            nextWorld = GameInstance.Worlds[currentWorldIndex + 1];
        }

        // Draw left side world enter text
        if (currentWorldIndex - 1 >= 0) {
            previousWorld = GameInstance.Worlds[currentWorldIndex - 1];
        }

        if (nextWorld != null) DrawRightArrow(g, $"Enter world\n    {nextWorld.Name}");
        if (previousWorld != null) DrawLeftArrow(g, $"Enter world\n    {previousWorld.Name}");


        // Draw player
        g.FillRectangle(new SolidBrush(player.Color), player.X, player.Y, player.Width, player.Height);

        DrawNpcs(g, player);

        DrawBuildings(g, player.CurrentWorld);
        DrawStatsBar(g);
        DrawQuest(g);
        DrawInventory(g);
        DrawButtons(g);
    }

    private static Shop? Shop;    

    // Main method to run the game form
    public static void Main() {
        if (DEBUG) AllocConsole();
        Console.WriteLine("Debug Console Started!");


        // Create game instance
        new GameInstance();

        // Initialize Events
        new GameEventListener();

        // Create home world
        World homeWorld = GameInstance.CreateWorld("Home");
        homeWorld.IsSafeWorld = true;

        homeWorld.Buildings.Add(new World.Building("Shop", 300, 300, 100, 100, World.BuildingType.Shop));
        homeWorld.Buildings.Add(new World.Building("Butcher", 40, 500, 80, 80, World.BuildingType.Quest));

        World caveWorld = GameInstance.CreateWorld("Cave");
        World forestWorld = GameInstance.CreateWorld("Forest");
        World castleWorld = GameInstance.CreateWorld("Castle");
        

        // Initialize Shop
        Shop = new Shop();

        
        // NPCs in the cave world
        NpcCharacter.CreateNPC("warrior", caveWorld, World.FindSafeSpaceFromWorld(caveWorld));
        NpcCharacter.CreateNPC("mage", caveWorld, World.FindSafeSpaceFromWorld(caveWorld));
        NpcCharacter.CreateNPC("mage", caveWorld, World.FindSafeSpaceFromWorld(caveWorld));
        NpcCharacter.CreateNPC("mage", caveWorld, World.FindSafeSpaceFromWorld(caveWorld));

        // NPCs in the forest world
        NpcCharacter.CreateNPC("warrior", forestWorld, World.FindSafeSpaceFromWorld(forestWorld));
        NpcCharacter.CreateNPC("archer", forestWorld, World.FindSafeSpaceFromWorld(forestWorld));
        NpcCharacter.CreateNPC("archer", forestWorld, World.FindSafeSpaceFromWorld(forestWorld));
        NpcCharacter.CreateNPC("archer", forestWorld, World.FindSafeSpaceFromWorld(forestWorld));

        // NPCs in the castle world
        NpcCharacter.CreateNPC("warrior", castleWorld, World.FindSafeSpaceFromWorld(castleWorld));
        NpcCharacter.CreateNPC("warrior", castleWorld, World.FindSafeSpaceFromWorld(castleWorld));
        NpcCharacter.CreateNPC("warrior", castleWorld, World.FindSafeSpaceFromWorld(castleWorld));
        NpcCharacter.CreateNPC("archer", castleWorld, World.FindSafeSpaceFromWorld(castleWorld));

        // Initialize DropManager and it's loot
        new DropManager();


        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new GameForm());
    }
}