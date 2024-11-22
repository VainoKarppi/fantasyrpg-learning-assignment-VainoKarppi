using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private static void DrawRightArrow(Graphics g, string text) {
        int arrowX = ScreenWidth - 50;
        int arrowY = ScreenHeight / 2 - (StatsBarHeight / 2);
        int arrowSize = 20;

        using Font font = new Font("Arial", 12, FontStyle.Bold);
        using Brush brush = new SolidBrush(Color.DarkGray);
        using Pen pen = new Pen(Color.Gray, 2);

        // Draw the arrow
        Point[] arrowPoints = {
            new Point(arrowX, arrowY - arrowSize),
            new Point(arrowX + arrowSize, arrowY),
            new Point(arrowX, arrowY + arrowSize)
        };

        g.DrawPolygon(pen, arrowPoints);
        g.FillPolygon(brush, arrowPoints);

        // Draw the text
        g.DrawString(text, font, brush, arrowX - 100, arrowY - arrowSize + 5);
    }

    private static void DrawLeftArrow(Graphics g, string text) {
        int arrowX = 30;
        int arrowY = ScreenHeight / 2 - (StatsBarHeight / 2);
        int arrowSize = 20;

        using Font font = new Font("Arial", 12, FontStyle.Bold);
        using Brush brush = new SolidBrush(Color.DarkGray);
        using Pen pen = new Pen(Color.Gray, 2);

        // Draw the arrow pointing left
        Point[] arrowPoints = {
            new Point(arrowX, arrowY - arrowSize),
            new Point(arrowX - arrowSize, arrowY),
            new Point(arrowX, arrowY + arrowSize)
        };

        g.DrawPolygon(pen, arrowPoints);
        g.FillPolygon(brush, arrowPoints);

        // Draw the text
        g.DrawString(text, font, brush, arrowX + 5, arrowY - arrowSize + 3);
    }
}