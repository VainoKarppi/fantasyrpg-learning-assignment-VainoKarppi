using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private static void DrawNpcs(Graphics g, Player player) {
        // Draw NPCs
        foreach (NpcCharacter npc in player.CurrentWorld.NPCs) {
            // Draw NPC body
            g.FillRectangle(new SolidBrush(npc.Color), npc.X, npc.Y, npc.Width, npc.Height);

            // Draw NPC name above the NPC
            string? npcName = npc.Name ?? "Enemy";
            
            using (Font font = new Font("Arial", 9, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.Gray)) {
                float textX = npc.X + (npc.Width - g.MeasureString(npcName, font).Width) / 2;
                float textY = npc.Y - 15;

                g.DrawString(npcName, font, brush, textX, textY);
            }

            // Draw NPC health bar below the NPC
            float healthBarWidth = 50;  // Set width of the health bar
            float healthBarHeight = 10;  // Set height of the health bar
            float healthBarX = npc.X + (npc.Width - healthBarWidth) / 2;  // Center the health bar
            float healthBarY = npc.Y + npc.Height + 5;  // Place it just below the NPC's body

            // Draw the background of the health bar (gray)
            g.FillRectangle(new SolidBrush(Color.Gray), healthBarX, healthBarY, healthBarWidth, healthBarHeight);

            // Calculate the current health percentage
            float healthPercentage = Math.Max(0, Math.Min(1, npc.Health / npc.Health));  // Ensure it's between 0 and 1

            // Draw the foreground of the health bar (green for health remaining)
            float healthBarForegroundWidth = healthBarWidth * healthPercentage;  // Scale the width based on current health
            g.FillRectangle(new SolidBrush(Color.Green), healthBarX, healthBarY, healthBarForegroundWidth, healthBarHeight);

            // Draw the current health number inside the health bar
            string healthText = $"{npc.Health}/{npc.Health}";
            using Font healthFont = new Font("Arial", 7, FontStyle.Bold);
            using Brush healthBrush = new SolidBrush(Color.White);
            
            float healthTextX = healthBarX + (healthBarWidth - g.MeasureString(healthText, healthFont).Width) / 2;
            float healthTextY = healthBarY + (healthBarHeight - g.MeasureString(healthText, healthFont).Height) / 2;

            // Draw the health number inside the health bar
            g.DrawString(healthText, healthFont, healthBrush, healthTextX, healthTextY);
        }
    }
}