using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private static readonly Font ArrowFont = new Font("Arial", 12, FontStyle.Bold);
    private static readonly Brush ArrowBrush = new SolidBrush(Color.DarkGray);
    private static readonly Pen ArrowPen = new Pen(Color.Gray, 2);

    private static void DrawRightArrow(Graphics g, string text) {
        int arrowX = ScreenWidth - 50;
        int arrowY = ScreenHeight / 2 - (StatsBarHeight / 2);
        int arrowSize = 20;

        // Draw the arrow
        Point[] arrowPoints = {
            new Point(arrowX, arrowY - arrowSize),
            new Point(arrowX + arrowSize, arrowY),
            new Point(arrowX, arrowY + arrowSize)
        };

        g.DrawPolygon(ArrowPen, arrowPoints);
        g.FillPolygon(ArrowBrush, arrowPoints);

        // Draw the text
        g.DrawString(text, ArrowFont, ArrowBrush, arrowX - 100, arrowY - arrowSize + 5);
    }

    private static void DrawLeftArrow(Graphics g, string text) {
        int arrowX = 30;
        int arrowY = ScreenHeight / 2 - (StatsBarHeight / 2);
        int arrowSize = 20;

        // Draw the arrow pointing left
        Point[] arrowPoints = {
            new Point(arrowX, arrowY - arrowSize),
            new Point(arrowX - arrowSize, arrowY),
            new Point(arrowX, arrowY + arrowSize)
        };

        g.DrawPolygon(ArrowPen, arrowPoints);
        g.FillPolygon(ArrowBrush, arrowPoints);

        // Draw the text
        g.DrawString(text, ArrowFont, ArrowBrush, arrowX + 5, arrowY - arrowSize + 3);
    }
}