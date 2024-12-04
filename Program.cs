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