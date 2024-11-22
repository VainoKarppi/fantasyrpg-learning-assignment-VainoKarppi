using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private static void DrawNpcs(Graphics g, Player player) {
        // Draw NPCs
        foreach (NpcCharacter npc in player.CurrentWorld.NPCs) {
            // Draw NPC body
            g.FillRectangle(new SolidBrush(npc.Color), npc.X, npc.Y, npc.Width, npc.Height);

            // Draw NPC name above the NPC
            string npcName = npc.Name;
            using (Font font = new Font("Arial", 9, FontStyle.Bold))
            
            using (Brush brush = new SolidBrush(Color.Gray)) {
                float textX = npc.X + (npc.Width - g.MeasureString(npcName, font).Width) / 2; 
                float textY = npc.Y - 15;

                g.DrawString(npcName, font, brush, textX, textY);
            }

            // Draw NPC health at the bottom of the NPC
            string npcHealth = $"Health: {npc.Health}";
            using Font healthFont = new Font("Arial", 8, FontStyle.Regular);
            using Brush healthBrush = new SolidBrush(Color.Red);

            float healthTextX = npc.X + (npc.Width - g.MeasureString(npcHealth, healthFont).Width) / 2;
            float healthTextY = npc.Y + npc.Height + 5;

            // Draw the health text
            g.DrawString(npcHealth, healthFont, healthBrush, healthTextX, healthTextY);
        }
    }
}