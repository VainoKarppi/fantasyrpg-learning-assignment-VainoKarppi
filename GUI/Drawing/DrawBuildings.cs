using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private static readonly Font BuildingFont = new Font("Arial", 10, FontStyle.Bold);
    private static readonly Brush TextBuildingBrush = new SolidBrush(Color.Black);
    private static readonly Brush ShopBuildingBrush = new SolidBrush(Color.Green);
    private static readonly Brush StorageBuildingBrush = new SolidBrush(Color.Blue);
    private static readonly Brush QuestBuildingBrush = new SolidBrush(Color.Red);
    private static readonly Brush DefaultBuildingBrush = new SolidBrush(Color.Gray);

    private static void DrawBuildings(Graphics g) {
        // Draw Buildings
        foreach (var building in Player.CurrentWorld!.Buildings) {
            // Choose a brush based on the building type
            Brush buildingBrush = building.BuildingType switch {
                World.BuildingType.Shop => ShopBuildingBrush,
                World.BuildingType.Storage => StorageBuildingBrush,
                World.BuildingType.Quest => QuestBuildingBrush,
                _ => DefaultBuildingBrush // Default for 'Block' type
            };

            // Draw the building as a rectangle
            g.FillRectangle(buildingBrush, building.X, building.Y, building.Width, building.Height);

            // Draw the building name centered above the rectangle
            SizeF nameSize = g.MeasureString(building.Name, BuildingFont);

            float nameX = building.X + (building.Width - nameSize.Width) / 2;
            float nameY = building.Y + (building.Height - nameSize.Height) / 2;

            g.DrawString(building.Name, BuildingFont, TextBuildingBrush, nameX, nameY);
        }
    }
}