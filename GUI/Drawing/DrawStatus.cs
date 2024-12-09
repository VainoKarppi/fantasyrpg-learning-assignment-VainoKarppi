using System.Drawing;

namespace GUI;

public partial class GameForm : Form {
    private static readonly Font StatFont = new Font("Arial", 10, FontStyle.Bold);
    private static readonly Brush StatBrush = Brushes.White;
    private static readonly Font TopBarFont = new Font("Arial", 16, FontStyle.Bold);
    private static readonly Brush TopBarBrush = Brushes.Black;

    private void DrawTopBar(Graphics g) {
        using (Brush backgroundBrush = new SolidBrush(Color.LightGray)) {
            g.FillRectangle(backgroundBrush, 0, 0, Width, TopBarHeight); // Full width, 40px height
        }

        string worldName = Player.CurrentWorld!.Name;
        SizeF textSize = g.MeasureString(worldName, TopBarFont);
        float textX = (Width - textSize.Width) / 2;
        float textY = (TopBarHeight - textSize.Height) / 2;

        g.DrawString(worldName, TopBarFont, TopBarBrush, textX, textY);
    }

    private void DrawPlayerStatus(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;

        g.FillRectangle(Brushes.Black, 0, statsBarTop, ClientSize.Width, StatsBarHeight);

        int padding = 10;
        string[] statsTexts = {
            $"Health: {Player.Health}",
            $"Money: {Player.Money}",
            $"Mana: {Player.Mana}",
            $"Weapon: {Player.CurrentWeapon?.Name}",
            $"Armor: {Player.CurrentArmor?.Name}"
        };

        for (int i = 0; i < statsTexts.Length; i++) {
            g.DrawString(statsTexts[i], StatFont, StatBrush, padding, statsBarTop + padding + (i * 20));
        }
    }

    private void DrawQuestStatus(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;
        int questBoxLeft = ClientSize.Width / 4;
        int questBoxWidth = ClientSize.Width / 4;

        g.FillRectangle(Brushes.DarkGray, questBoxLeft, statsBarTop, questBoxWidth, StatsBarHeight);

        int padding = 10;
        string questName = Player.CurrentQuest?.Name ?? "No Quest Active!";
        string? questDescription = Player.CurrentQuest?.Description;
        string? questStageDescription = Player.CurrentQuest?.StageDescription;

        string text = questDescription != null ? $"{questName} - ({questDescription})" : questName;
        g.DrawString($"Current Quest:\n{text}", StatFont, StatBrush, questBoxLeft + padding, statsBarTop + padding);

        if (questStageDescription != null) {
            g.DrawString($"\nStatus: {questStageDescription}", StatFont, StatBrush, questBoxLeft + padding, statsBarTop + padding + 20);
        }
    }

    private void DrawInventory(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;
        int inventoryBoxLeft = ClientSize.Width / 2;
        int inventoryBoxWidth = ClientSize.Width / 4;

        g.FillRectangle(Brushes.Black, inventoryBoxLeft, statsBarTop, inventoryBoxWidth, StatsBarHeight);

        int padding = 10;
        g.DrawString("Inventory:", StatFont, StatBrush, inventoryBoxLeft + padding, statsBarTop + padding);

        var groupedItems = Player.InventoryItems
            .GroupBy(item => item.Name)
            .Select(group => new { Name = group.Key, Count = group.Count() });

        int inventoryY = statsBarTop + padding + 20;
        foreach (var groupedItem in groupedItems) {
            string? itemDisplay = groupedItem.Count > 1 
                ? $"{groupedItem.Name} (x{groupedItem.Count})" 
                : groupedItem.Name;

            g.DrawString(itemDisplay, StatFont, StatBrush, inventoryBoxLeft + padding, inventoryY);
            inventoryY += 20;

            if (inventoryY > statsBarTop + StatsBarHeight - 20) break;
        }
    }
}
