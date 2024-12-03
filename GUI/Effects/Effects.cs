using System.Drawing;

namespace GUI;



public partial class GameForm : Form {

    public static async Task TriggerDamageEffect(bool shakeEffect = true) {

        // Trigger the blood splash effect at the player's location
        TriggerBloodSplashEffect(player);

        if (!shakeEffect) return;

        // Shake the form by briefly changing its location
        Point originalLocation = _location;

        Random rand = new Random();
        int shakeDuration = 10; // Duration in milliseconds
        int shakeAmount = 10; // The amount of shake (in pixels)

        // Shake the screen 5 times (for example)
        for (int i = 0; i < 5; i++) {
            int offsetX = rand.Next(-shakeAmount, shakeAmount + 1);
            int offsetY = rand.Next(-shakeAmount, shakeAmount + 1);

            _location = new Point(originalLocation.X + offsetX, originalLocation.Y + offsetY);
            await Task.Delay(shakeDuration);  // Brief delay to create the shake effect
        }

        // Return the form to its original position
        _location = originalLocation;
    }

    public static async Task TriggerBloodSplashEffect(ICharacter character) {
        if (_controls is null) return;

        Point location = new Point(character.X + character.Height / 2, character.Y + character.Width / 2);

        // Set up parameters for the blood splash
        int splashSize = player.Width + player.Height + 5;  // Size of the blood splatter (in pixels)
        int splashAlpha = 220;  // Initial alpha (partially transparent)

        // Create a new PictureBox to draw the blood splash
        PictureBox bloodSplash = new PictureBox {
            Width = splashSize,
            Height = splashSize,
            Location = new Point(location.X - splashSize / 2, location.Y - splashSize / 2),  // Center it on the player location
            BackColor = Color.Transparent
        };

        // Create a Bitmap for the blood splash
        Bitmap splashBitmap = new Bitmap(splashSize, splashSize);
        bloodSplash.Image = splashBitmap;
        _controls.Add(bloodSplash);
        bloodSplash.BringToFront();

        // Fade out the blood splash
        for (int alpha = splashAlpha; alpha >= 0; alpha -= 10) {
            using (Graphics g = Graphics.FromImage(splashBitmap)) {
                g.Clear(Color.Transparent);  // Clear the previous drawing

                // Redraw the square with reduced alpha
                using Brush squareBrush = new SolidBrush(Color.FromArgb(alpha, Color.DarkRed));
                int squareSize = splashSize / 2;
                int squareOffset = (splashSize - squareSize) / 2;
                g.FillRectangle(squareBrush, squareOffset, squareOffset, squareSize, squareSize);
            }

            bloodSplash.Refresh();  // Refresh to show the updated image
            await Task.Delay(40);  // Smooth fade-out effect
        }

        // Remove the blood splash
        _controls.Remove(bloodSplash);
        bloodSplash.Dispose();
    }
}