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
    
    public GameForm(Player player, Shop? shop = null) {

        // Initialize the player
        Player = player ?? throw new ArgumentNullException(nameof(player));
        string username = PromptForUsername();
        Player.Name = username;
        Player.Money = 800;
        Player.AddItem(new ItemDrop.Gems.Sapphire());

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