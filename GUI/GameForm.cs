using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;


namespace GUI;
public partial class GameForm : Form {
    public static Player Player { get; internal set; }
    private static Shop? Shop;

    public static GameForm? Form { get; private set; }

    private const int MoveStep = 5;

    public const int ScreenWidth = 1000;
    public const int ScreenHeight = 700 + StatsBarHeight;

    public const int StatsBarHeight = 200;
    public const int TopBarHeight = 40;

    public const int WorldWidth = ScreenWidth;
    public const int WorldHeight = ScreenHeight - StatsBarHeight - TopBarHeight;
    
    public GameForm(Shop? shop = null) {
        // Initialize Player
        string name = PromptForUsername();

        // Save/Restore player to/From DB
        try {
            Player? restorePlayer = Database.RestorePlayer(name);
            
            if (restorePlayer == null) {
                World homeWorld = GameInstance.Worlds.First(x => x.IsSafeWorld);
                Player = new Player(homeWorld, "Player");
                Player.Name =  name;
                Player.Money = 1500;
                Player.AddItem(new ItemDrop.Gems.Sapphire());

                Database.SavePlayerAsync(Player).Wait();
            } else {
                Player = restorePlayer;
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
        
        // Initialize Shop
        Shop = shop ?? Shop;

        // Initialize GameForm
        Form = this;

        SuspendLayout();
        ClientSize = new Size(ScreenWidth, ScreenHeight);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        Text = "RPG Game";
        ResumeLayout(false);
        KeyPreview = false;
        DoubleBuffered = true; // To avoid flickering
        Width = ScreenWidth;
        Height = ScreenHeight;
        
        // Initialize Game buttons
        InitializeButtons();
        
        Paint += GameForm_Paint;
        KeyDown += GameForm_KeyDown;
        KeyUp += GameForm_KeyUp;

        FormClosing += GameForm_FormClosing;
    }


    private async void GameForm_FormClosing(object? sender, FormClosingEventArgs e) {
        try {
            Console.WriteLine("Saving player...");
            await Database.SavePlayerAsync(Player);

            Console.WriteLine("Saving NPCs...");
            await Database.SaveAllNpcs();
        } catch (Exception ex) {
            Console.WriteLine($"An error occurred while saving the player: {ex.Message}");
        }
    }

    

    

    

    private static World? nextWorld = null;
    private static World? previousWorld = null;
    private static int lastWorldIndex = -1;

    private void GameForm_Paint(object? sender, PaintEventArgs e) {
        var g = e.Graphics;
        
        // Draw all current effects on screen
        Effect.DrawEffects(g);
        

        // Check if the world has changed
        int currentWorldIndex = GameInstance.Worlds.IndexOf(Player.CurrentWorld!);
        if (currentWorldIndex != lastWorldIndex) {
            lastWorldIndex = currentWorldIndex;
            // Get Left and Right worlds (check if exists)
            nextWorld = (currentWorldIndex + 1 < GameInstance.Worlds.Count) ? GameInstance.Worlds[currentWorldIndex + 1] : null;
            previousWorld = (currentWorldIndex - 1 >= 0) ? GameInstance.Worlds[currentWorldIndex - 1] : null;
        }

        if (nextWorld != null) DrawRightArrow(g, $"Enter world\n    {nextWorld.Name}");
        if (previousWorld != null) DrawLeftArrow(g, $"Enter world\n    {previousWorld.Name}");

        // Draw player
        g.FillRectangle(new SolidBrush(Player.Color), Player.X, Player.Y, Player.Width, Player.Height);


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

    public static void RefreshPage() {
        if (Form == null) throw new InvalidOperationException("No active GameForm instance is available.");

        Form.Invalidate();
    }
}