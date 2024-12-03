using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;


namespace GUI;



public partial class GameForm : Form {
    public const bool DEBUG = true;

    #if DEBUG
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
    #endif

    public static Player player; // Never null

    private static GameForm? Instance;
    private static Point _location;
    private static Control.ControlCollection? _controls;

    private const int MoveStep = 5;

    public const int ScreenWidth = 1000;
    public const int ScreenHeight = 700 + StatsBarHeight;

    public const int StatsBarHeight = 200;
    public const int TopBarHeight = 40;

    public const int WorldWidth = ScreenWidth;
    public const int WorldHeight = ScreenHeight - StatsBarHeight - TopBarHeight;

    public GameForm() {
        
        SuspendLayout();
        ClientSize = new Size(ScreenWidth, ScreenHeight);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Text = "Game Window";
        ResumeLayout(false);
        KeyPreview = true;
        DoubleBuffered = true; // To avoid flickering

        _location = Location;
        _controls = Controls;

        InitializeButtons();

        Width = ScreenWidth;
        Height = ScreenHeight;

        // Initialize the player
        player = new Player(GameInstance.GetWorld("Home"), "test name"); // TODO name
        player.Money = 800;
        player.AddItem(new ItemDrop.Gems.Sapphire());


        Paint += GameForm_Paint;
        KeyDown += GameForm_KeyDown;
        KeyUp += GameForm_KeyUp;


        // Create singelton static instance, so that we can access the GUI data from anyhwere
        Instance = this;
    }

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

        // Redraw the screen
        Invalidate();
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
        }

        // Redraw the screen
        Invalidate();
    }

    

    private static void CheckPlayerBuildingWorldBounds() {
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
    }

    // Ensure the player does not move outside world boundaries
    private static void CheckPlayerWorldBounds() {

        // Move player From Left to Right World
        if (player.X + player.Width >= ScreenWidth - 20) {
            int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);

            if (currentWorldIndex + 1 < GameInstance.Worlds.Count) {
                player.ChangeWorld(GameInstance.Worlds[currentWorldIndex + 1]);
            }
        }

        // Move player From Right to Left World
        if (player.X + player.Width <= 20) {
            int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);

            if (currentWorldIndex - 1 >= 0) {
                player.ChangeWorld(GameInstance.Worlds[currentWorldIndex - 1]);
                
            }
        }

        // Clamp player's position within world boundaries
        player.X = Math.Clamp(player.X, 0, WorldWidth - player.Width);
        player.Y = Math.Clamp(player.Y, TopBarHeight, WorldHeight - player.Height); // Add some extra at the bottom, for stats display

        // Send update location
        MultiplayerClient.SendMessageAsync(new { MessageType = NetworkMessageType.SendUpdateData, player.ID, player.X, player.Y, CurrentWorldName = player.CurrentWorld.Name });
    }

    // Check if player collides with any NPCs
    private void CheckCollisionsInteraction() {

        // Check NPC collisions
        foreach (NpcCharacter npc in player.CurrentWorld.NPCs) {
            if (player.Bounds.IntersectsWith(npc.Bounds)) {
                // Show dialog if collision detected
                pressedKeys.Clear();

                ShowNpcDialog(npc);

                break;
            }
        }

        // Check collisions with Buildings
        foreach (World.Building building in player.CurrentWorld.Buildings) {
            if (player.Bounds.IntersectsWith(new Rectangle(building.X - 5, building.Y - 5, building.Width + 10, building.Height + 10))) {

                pressedKeys.Clear();

                if (building.BuildingType == World.BuildingType.Shop) OpenShopMenu();
                if (building.Name == "Quest 1") MassacareQuest();
                
                break;
            }
        }
    }

    



    // Show the dialog to ask whether to attack or flee
    private void ShowNpcDialog(NpcCharacter npc) {
        var result = MessageBox.Show("You have encountered an NPC! Do you want to attack or flee?", 
                                     "NPC Encounter", 
                                     MessageBoxButtons.YesNo);

        // Yes means attack, No means flee
        if (result == DialogResult.Yes) {
            Attack(npc);
        } else {
            Flee();
        }
    }

    public static void RefreshPage() {
        if (Instance == null) throw new InvalidOperationException("No active GameForm instance is available.");

        Instance.Invalidate();
    }


    // Handle the attack action
    private void Attack(NpcCharacter npc) {
        
        player.PlayerActions.Attack(npc);

        TriggerBloodSplashEffect(npc);
    }

    // Handle the flee action
    private void Flee() {
        // Flee logic: Move player away from the NPC
        player.X += 50; // Example flee action: Move the player to the right
        player.Y += 50; // Move player downward
    }

    private static World? nextWorld = null;
    private static World? previousWorld = null;
    private static int lastWorldIndex = -1;

    private void GameForm_Paint(object? sender, PaintEventArgs e) {
        var g = e.Graphics;

        // Check if the world has changed
        int currentWorldIndex = GameInstance.Worlds.IndexOf(player.CurrentWorld);
        if (currentWorldIndex != lastWorldIndex) {
            lastWorldIndex = currentWorldIndex;
            // Get Left and Right worlds (check if exists)
            nextWorld = (currentWorldIndex + 1 < GameInstance.Worlds.Count) ? GameInstance.Worlds[currentWorldIndex + 1] : null;
            previousWorld = (currentWorldIndex - 1 >= 0) ? GameInstance.Worlds[currentWorldIndex - 1] : null;
        }

        if (nextWorld != null) DrawRightArrow(g, $"Enter world\n    {nextWorld.Name}");
        if (previousWorld != null) DrawLeftArrow(g, $"Enter world\n    {previousWorld.Name}");

        // Draw player
        g.FillRectangle(new SolidBrush(player.Color), player.X, player.Y, player.Width, player.Height);


        // TODO Optimize
        DrawOtherPlayers(g);
        DrawNpcs(g);
        DrawBuildings(g);
        DrawTopBar(g);
        DrawPlayerStatus(g);
        DrawQuestStatus(g);
        DrawInventory(g);
        DrawButtons(g);
    }


    private static Shop? Shop;    

    // Main method to run the game form
    public static void Main() {
        #if DEBUG
            if (DEBUG) AllocConsole();
            Console.WriteLine("Debug Console Started!");
        #endif

        // Create game instance
        new GameInstance();

        // Initialize Events
        new GameEventListener();

        // Create home world
        World homeWorld = GameInstance.CreateWorld("Home");
        homeWorld.IsSafeWorld = true;

        homeWorld.Buildings.Add(new World.Building("Shop", 300, 300, 100, 100, World.BuildingType.Shop));
        homeWorld.Buildings.Add(new World.Building("Quest 1", 40, 500, 80, 80, World.BuildingType.Quest));

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