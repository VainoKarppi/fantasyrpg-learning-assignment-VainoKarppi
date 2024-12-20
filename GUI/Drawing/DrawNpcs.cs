using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private static readonly Font NameFont = new Font("Arial", 9, FontStyle.Bold);
    private static readonly Font HealthFont = new Font("Arial", 7, FontStyle.Bold);
    private static readonly Brush NameBrush = new SolidBrush(Color.Gray);
    private static readonly Brush HealthBrush = new SolidBrush(Color.White);
    private static readonly Brush HealthBarBrush = new SolidBrush(Color.Green);
    private static readonly Brush HealthBackgroundBrush = new SolidBrush(Color.Gray);

    
    private static void DrawNpcs(Graphics g) {

        // Draw NPCs
        foreach (NpcCharacter npc in Player.CurrentWorld!.NPCs) {
            // Draw NPC body
            g.FillRectangle(new SolidBrush(npc.Color), npc.X, npc.Y, npc.Width, npc.Height);

            // Draw NPC name above the NPC
            string npcName = npc.Name ?? "Enemy";
            float textX = npc.X + (npc.Width - g.MeasureString(npcName, NameFont).Width) / 2;
            float textY = npc.Y - 15;
            g.DrawString(npcName, NameFont, NameBrush, textX, textY);

            // Draw NPC health bar below the NPC
            float healthBarWidth = 50;  // Set width of the health bar
            float healthBarHeight = 10;  // Set height of the health bar
            float healthBarX = npc.X + (npc.Width - healthBarWidth) / 2;  // Center the health bar
            float healthBarY = npc.Y + npc.Height + 5;  // Place it just below the NPC's body

            // Draw the background of the health bar (gray)
            g.FillRectangle(HealthBackgroundBrush, healthBarX, healthBarY, healthBarWidth, healthBarHeight);

            // Calculate the current health percentage
            float healthPercentage = npc.MaxHealth > 0 
                ? Math.Max(0, Math.Min(1, (float)npc.Health / npc.MaxHealth))  // Ensure it's between 0 and 1
                : 0; // Default to 0 if MaxHealth is zero or invalid

            // Draw the foreground of the health bar (green for health remaining)
            float healthBarForegroundWidth = healthBarWidth * healthPercentage;
            g.FillRectangle(HealthBarBrush, healthBarX, healthBarY, healthBarForegroundWidth, healthBarHeight);

            // Draw the current health number inside the health bar
            string healthText = $"{npc.Health}/{npc.MaxHealth}";
            float healthTextX = healthBarX + (healthBarWidth - g.MeasureString(healthText, HealthFont).Width) / 2;
            float healthTextY = healthBarY + (healthBarHeight - g.MeasureString(healthText, HealthFont).Height) / 2;
            g.DrawString(healthText, HealthFont, HealthBrush, healthTextX, healthTextY);
        }
    }

    
    private static void DrawOtherPlayers(Graphics g) {

        // Draw other Players
        foreach (NetworkMessage networkPlayer in MultiplayerClient.OtherPlayers) {
            // Ensure valid position and current world
            if (networkPlayer.X is null || networkPlayer.Y is null ||
                networkPlayer.CurrentWorldName == null || 
                networkPlayer.CurrentWorldName.ToLower() != Player.CurrentWorld!.Name.ToLower()) continue;

            // Draw other player body
            g.FillRectangle(new SolidBrush(Color.DarkOrange), (int)networkPlayer.X, (int)networkPlayer.Y, Player.Width, Player.Height);

            // Draw NPC name above the NPC
            string playerName = networkPlayer.Name ?? "Other Player";
            float textX = (float)networkPlayer.X + (Player.Width - g.MeasureString(playerName, NameFont).Width) / 2;
            float textY = (float)networkPlayer.Y - 15;
            g.DrawString(playerName, NameFont, NameBrush, textX, textY);

            // Draw other player health bar below the player
            if (networkPlayer.Health == null) networkPlayer.Health = 100;

            int maxPlayerHealth = 100;
            float healthBarWidth = 50;  // Set width of the health bar
            float healthBarHeight = 10;  // Set height of the health bar
            float healthBarX = (int)networkPlayer.X + (Player.Width - healthBarWidth) / 2;  // Center the health bar
            float healthBarY = (int)networkPlayer.Y + Player.Height + 5;  // Place it just below the NPC's body

            // Draw the background of the health bar (gray)
            g.FillRectangle(HealthBackgroundBrush, healthBarX, healthBarY, healthBarWidth, healthBarHeight);

            // Calculate the current health percentage
            float healthPercentage = Math.Max(0, Math.Min(1, (float)networkPlayer.Health / maxPlayerHealth)); // Ensure it's between 0 and 1

            // Draw the foreground of the health bar (green for health remaining)
            float healthBarForegroundWidth = healthBarWidth * healthPercentage;
            g.FillRectangle(HealthBarBrush, healthBarX, healthBarY, healthBarForegroundWidth, healthBarHeight);

            // Draw the current health number inside the health bar
            string healthText = $"{networkPlayer.Health}/{maxPlayerHealth}";
            float healthTextX = healthBarX + (healthBarWidth - g.MeasureString(healthText, HealthFont).Width) / 2;
            float healthTextY = healthBarY + (healthBarHeight - g.MeasureString(healthText, HealthFont).Height) / 2;
            g.DrawString(healthText, HealthFont, HealthBrush, healthTextX, healthTextY);
        }
    }
}