

using System.Drawing.Drawing2D;
using System.Net;
using System.Text.Json;

namespace GUI;


public class Effect {
    public static readonly List<Effect> Effects = [];
    // Handle the attack action
    private readonly System.Threading.Timer attackTimer;
    private Point startPoint;
    private Point endPoint;

    private readonly Character Attacker;

    private float Progress = 0;
    private float Radius;

    private const float effectSpeed = 0.1f; // Adjust the speed of the swoosh
    private const int Width = 4;

    public enum EffectType {
        Melee,
        Mage,
        Ranged,
        Potion,
        Blood
    }
    private readonly EffectType Type;

    public Effect(Character start, Character? end, EffectType type) {
        startPoint = start.GetCenter(); // center
        Attacker = start;
        Type = type;
        
        // In case of blood effect, we need nothing else than pos
        if (type == EffectType.Blood) {
            Effects.Add(this);
            return;
        }

        if (start.CurrentState == Character.State.Attacking) return;
        start.CurrentState = Character.State.Attacking;
        
        attackTimer = new System.Threading.Timer(UpdateEffectProgress, null, 0, 20);


        if (end != null) endPoint = end.GetCenter(); // center

        // If endPoint not defined, draw on the right side from the start
        if (end == null && (type == EffectType.Mage || type == EffectType.Ranged)) endPoint = new Point(startPoint.X + (start.CurrentWeapon?.Range ?? 100), startPoint.Y);
        
        Effects.Add(this);
    }

    public Effect(Character start, Point end, EffectType type) {
        startPoint = start.GetCenter(); // center
        Attacker = start;
        Type = type;
        
        // In case of blood effect, we need nothing else than pos
        if (type == EffectType.Blood) {
            Effects.Add(this);
            return;
        }
        
        attackTimer = new System.Threading.Timer(UpdateEffectProgress, null, 0, 20);


        endPoint = end;

        Effects.Add(this);
    }

    private void UpdateEffectProgress(object? state) {
        if (Progress < 1.0f) {
            Progress += effectSpeed;
            GameForm.RefreshPage();
        } else {
            attackTimer.Dispose();
            Effects.Remove(this);
            GameForm.RefreshPage();

            // Reset character state after effect completion
            if (Attacker.CurrentWeapon != null) Task.Delay(Attacker.CurrentWeapon.ReloadTime).Wait();
            Attacker.CurrentState = Character.State.Idle;
        }
    }

    internal static void DrawEffects(Graphics g) {
        // TODO run on async?
        DrawSwordEffects(g);
        DrawMageEffects(g);
        DrawRangedEffects(g);
        DrawBloodEffects();

    }
    private static void DrawSwordEffects(Graphics g) {
        foreach (Effect effect in Effects) {
            if (effect.Type != EffectType.Melee) continue;

            effect.Radius = Math.Max(effect.Attacker.Width, effect.Attacker.Height) * 1.5f;

            // Calculate the current angle based on the progress (effectProgress is from 0.0 to 1.0)
            float angle = 360f * effect.Progress;

            // Convert angle to radians
            float angleInRadians = (float)(angle * Math.PI / 180);

            // Calculate the current position along the circular path
            float x = effect.startPoint.X + effect.Radius * (float)Math.Cos(angleInRadians);
            float y = effect.startPoint.Y + effect.Radius * (float)Math.Sin(angleInRadians);

            // Draw the swoosh arc
            using Pen swooshPen = new Pen(Color.Cyan, Width); // Adjustable color and thickness
            swooshPen.StartCap = LineCap.Round;
            swooshPen.EndCap = LineCap.Round;

            // Draw a line from the swoosh center to the current position
            g.DrawLine(swooshPen, effect.startPoint.X, effect.startPoint.Y, x, y);

            // Optionally, draw an arc segment
            RectangleF swooshBounds = new RectangleF(
                effect.startPoint.X - effect.Radius,
                effect.startPoint.Y - effect.Radius,
                effect.Radius * 2,
                effect.Radius * 2
            );

            g.DrawArc(swooshPen, swooshBounds, angle - 45, 45); // Adjust angles for the arc
        }
    }

    private static readonly Pen GlowPen = new Pen(Color.DarkCyan, 10) { StartCap = LineCap.Round, EndCap = LineCap.Round };
    private static readonly Pen EffectPen = new Pen(Color.Red, 6) { StartCap = LineCap.Round, EndCap = LineCap.Round };

    private static void DrawMageEffects(Graphics g) {
        foreach (Effect effect in Effects) {
            if (effect.Type != EffectType.Mage) continue;

            // Calculate current position along the line based on progress
            float currentX = effect.startPoint.X + (effect.endPoint.X - effect.startPoint.X) * effect.Progress;
            float currentY = effect.startPoint.Y + (effect.endPoint.Y - effect.startPoint.Y) * effect.Progress;

            // Draw the glow line
            g.DrawLine(GlowPen, effect.startPoint.X, effect.startPoint.Y, currentX, currentY);

            // Draw the main red line over the glow
            g.DrawLine(EffectPen, effect.startPoint.X, effect.startPoint.Y, currentX, currentY);
        }
    }

    private static void DrawRangedEffects(Graphics g) {
        foreach (Effect effect in Effects) {
            if (effect.Type != EffectType.Ranged) continue;

            // Calculate current position along the line based on progress
            float currentX = effect.startPoint.X + (effect.endPoint.X - effect.startPoint.X) * effect.Progress;
            float currentY = effect.startPoint.Y + (effect.endPoint.Y - effect.startPoint.Y) * effect.Progress;


            // Set up the grey pen for drawing the short line effect
            using Pen rangedPen = new Pen(Color.Gray, 5);  // Grey color, 5px thickness
            rangedPen.StartCap = LineCap.Round;
            rangedPen.EndCap = LineCap.Round;

            // Length of the "bullet" (line)
            float bulletLength = 10f;

            // Calculate the direction the bullet is traveling in
            float dx = effect.endPoint.X - effect.startPoint.X;
            float dy = effect.endPoint.Y - effect.startPoint.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);  // Distance between start and end point

            // Normalize the direction to unit vector
            dx /= distance;
            dy /= distance;

            // Calculate the bullet's "short line" position
            float bulletEndX = currentX + dx * bulletLength;
            float bulletEndY = currentY + dy * bulletLength;

            // Draw the bullet as a short line
            g.DrawLine(rangedPen, currentX, currentY, bulletEndX, bulletEndY);
        }
    }

    


    

    public static async Task DrawBloodEffects() {
        if (GameForm.Form == null) return;

        int i = 0;
        foreach (Effect effect in Effects) {
            if (effect.Type != EffectType.Blood) continue;
            
            Effects.Remove(effect);

            World originWorld = effect.Attacker.CurrentWorld;

            // Set up parameters for the blood splash
            int splashSize = effect.Attacker.Width + effect.Attacker.Height + 5; // Size of the blood splatter (in pixels)
            int splashAlpha = 220;

            // Create a new PictureBox to draw the blood splash
            PictureBox bloodSplash = new PictureBox {
                Width = splashSize,
                Height = splashSize,
                Location = new Point(effect.Attacker.GetCenter().X - splashSize / 2, effect.Attacker.GetCenter().Y - splashSize / 2), // Center it on the character
                BackColor = Color.Transparent
            };

            // Create a Bitmap for the blood splash
            Bitmap splashBitmap = new Bitmap(splashSize, splashSize);
            bloodSplash.Image = splashBitmap;

            // Add the control to the form on the UI thread
            await Task.Run(() => {
                GameForm.Form.Invoke((MethodInvoker)(() => {
                    GameForm.Form.Controls.Add(bloodSplash);
                    GameForm.Form.Controls.SetChildIndex(bloodSplash, i); // Set child index to align with effect hierarchy
                }));
            });


            // Fade out the blood splash --> RUN as a new task to prevent UI blocking
            await Task.Run(async () => {
                for (int alpha = splashAlpha; alpha >= 0; alpha -= 10) {
                    if (originWorld != GameForm.Player.CurrentWorld) break; // Stop drawing if dead/reset

                    bloodSplash.Location = new Point(effect.Attacker.GetCenter().X - splashSize / 2, effect.Attacker.GetCenter().Y - splashSize / 2); // Center it on the character
                    
                    using (Graphics g = Graphics.FromImage(splashBitmap)) {
                        g.Clear(Color.Transparent); // Clear the previous drawing

                        // Draw the blood splash with reduced alpha
                        using Brush squareBrush = new SolidBrush(Color.FromArgb(alpha, Color.DarkRed));
                        int squareSize = splashSize / 2;
                        int squareOffset = (splashSize - squareSize) / 2;
                        g.FillRectangle(squareBrush, squareOffset, squareOffset, squareSize, squareSize);
                    }
                    bloodSplash.Refresh();
                    await Task.Delay(40);
                }
            });

            await Task.Run(() => {
                // Remove the control from form on the UI thread
                GameForm.Form.Invoke((MethodInvoker)(() => {
                    GameForm.Form.Controls.Remove(bloodSplash);
                    bloodSplash.Dispose();
                }));
            });

            i++;
        }
    }


    public static async void TriggerScreenShake() {
        if (GameForm.Form == null) return;

        Point originalLocation = GameForm.Form.Location;
        int shakeDuration = 200;
        int shakeIntensity = 10;

        Random random = new Random();
        int elapsed = 0;

        while (elapsed < shakeDuration) {
            int offsetX = random.Next(-shakeIntensity, shakeIntensity + 1);
            int offsetY = random.Next(-shakeIntensity, shakeIntensity + 1);
            GameForm.Form.Location = new Point(originalLocation.X + offsetX, originalLocation.Y + offsetY);

            await Task.Delay(30);
            elapsed += 50;
        }

        // Restore the form's original location
        GameForm.Form.Location = originalLocation;
    }
}
