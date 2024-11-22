using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private static void DrawBuildings(Graphics g, World world) {
        foreach (var building in world.Buildings) {
            // Choose a color based on the building type
            Brush buildingBrush = building.BuildingType switch {
                World.BuildingType.Shop => new SolidBrush(Color.Green),
                World.BuildingType.Storage => new SolidBrush(Color.Blue),
                _ => new SolidBrush(Color.Gray) // Default for 'Block' type
            };

            // Draw the building as a rectangle
            g.FillRectangle(buildingBrush, building.X, building.Y, building.Width, building.Height);

            // Draw the building name centered above the rectangle
            using Font font = new Font("Arial", 10, FontStyle.Bold);
            using Brush textBrush = new SolidBrush(Color.Black);

            SizeF nameSize = g.MeasureString(building.Name, font);

            float nameX = building.X + (building.Width - nameSize.Width) / 2;
            float nameY = building.Y + (building.Height - nameSize.Height)  / 2;

            g.DrawString(building.Name, font, textBrush, nameX, nameY);
        }
    }
}