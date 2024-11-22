using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private void DrawStatsBar(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;

        // Draw background for stats bar
        g.FillRectangle(Brushes.Black, 0, statsBarTop, ClientSize.Width, StatsBarHeight);

        // Draw stats text
        var font = new Font("Arial", 10, FontStyle.Bold);
        var brush = Brushes.White;
        int padding = 10;


        string healthText = $"Health: {player.Health}";
        string moneyText = $"Money: {player.Money}";
        string manaText = $"Mana: {player.Mana}";
        string weaponText = $"Weapon: {player.CurrentWeapon?.Name}";
        string armorText = $"Armor: {player.CurrentArmor?.Name}";

        g.DrawString(healthText, font, brush, padding, statsBarTop + padding);
        g.DrawString(moneyText, font, brush, padding, statsBarTop + padding + 20);
        g.DrawString(manaText, font, brush, padding, statsBarTop + padding + 40);
        g.DrawString(weaponText, font, brush, padding, statsBarTop + padding + 60);
        g.DrawString(armorText, font, brush, padding, statsBarTop + padding + 80);
    }
    private void DrawQuest(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;
        int questBoxLeft = ClientSize.Width / 4;
        int questBoxWidth = ClientSize.Width / 4;

        // Draw quest and inventory background
        g.FillRectangle(Brushes.DarkGray, questBoxLeft, statsBarTop, questBoxWidth, StatsBarHeight);

        var font = new Font("Arial", 10, FontStyle.Bold);
        var brush = Brushes.White;
        int padding = 10;

        // Display current quest
        string? questName = player.CurrentQuest?.Name;
        string? questDescription = player.CurrentQuest?.Description;

        if (questName is null) questName = "(No quest active)";
        g.DrawString($"Current Quest: {questName}", font, brush, questBoxLeft + padding, statsBarTop + padding);
        
        if (questDescription != null) {
            g.DrawString($"Description: {questDescription}", font, brush, questBoxLeft + padding, statsBarTop + padding + 20);
        }


    }
    private void DrawInventory(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;
        int questBoxLeft = ClientSize.Width / 2;
        int questBoxWidth = ClientSize.Width / 2;

        // Draw quest and inventory background
        g.FillRectangle(Brushes.Black, questBoxLeft, statsBarTop, questBoxWidth, StatsBarHeight);

        var font = new Font("Arial", 10, FontStyle.Bold);
        var brush = Brushes.White;
        int padding = 10;

        // Display inventory
        string inventoryTitle = "Inventory:";
        g.DrawString(inventoryTitle, font, brush, questBoxLeft + padding, statsBarTop + padding);

        // Draw inventory items as a scrollable list
        int inventoryY = statsBarTop + padding + 20;
        foreach (ItemBase item in player.InventoryItems) {
            g.DrawString($"\t{item.Name}", font, brush, questBoxLeft + padding, inventoryY);
            inventoryY += 20;
            if (inventoryY > statsBarTop + StatsBarHeight - 20) break; // Ensure items fit in the box
        }
    }
}