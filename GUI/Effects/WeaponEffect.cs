

using System.Drawing.Drawing2D;

namespace GUI;

public partial class GameForm : Form {
    public class Effects {
        // Handle the attack action
        private readonly System.Windows.Forms.Timer attackTimer;
        private PointF startPoint;
        private PointF endPoint;
        
        private ICharacter Attacker;
        private ICharacter Target;

        private float effectProgress;
        private float swooshRadius;

        private const float effectSpeed = 0.1f; // Adjust the speed of the swoosh
        private const int Width = 4;

        public Effects() {
            attackTimer = new System.Windows.Forms.Timer();
            attackTimer.Interval = 20; // 20 ms for smooth animation
            attackTimer.Tick += AttackTimer_Tick;
        }

        internal void WeaponEffect(ICharacter attacker, ICharacter target) {
            if (attacker.CanAttack(target)) {

                Attacker = attacker;
                Target = target;

                startPoint = new PointF(attacker.X + attacker.Width / 2, attacker.Y + attacker.Height / 2); // center
                endPoint = new PointF(target.X + attacker.Width / 2, target.Y + target.Height / 2); // center

                attacker.CurrentState = ICharacter.State.Attacking;

                effectProgress = 0; // Reset the swoosh progress
                attackTimer.Start(); // Start the animation timer
            }
        }

        internal void DrawSwordEffect(Graphics g) {
            swooshRadius = Math.Max(Attacker.Width, Attacker.Height) * 1.5f;

            // Calculate the current angle based on the progress (effectProgress is from 0.0 to 1.0)
            float angle = 360f * effectProgress;

            // Convert angle to radians
            float angleInRadians = (float)(angle * Math.PI / 180);

            // Calculate the current position along the circular path
            float x = startPoint.X + swooshRadius * (float)Math.Cos(angleInRadians);
            float y = startPoint.Y + swooshRadius * (float)Math.Sin(angleInRadians);

            // Draw the swoosh arc
            using Pen swooshPen = new Pen(Color.Cyan, Width); // Adjustable color and thickness
            swooshPen.StartCap = LineCap.Round;
            swooshPen.EndCap = LineCap.Round;

            // Draw a line from the swoosh center to the current position
            g.DrawLine(swooshPen, startPoint.X, startPoint.Y, x, y);

            // Optionally, draw an arc segment
            RectangleF swooshBounds = new RectangleF(
                startPoint.X - swooshRadius,
                startPoint.Y - swooshRadius,
                swooshRadius * 2,
                swooshRadius * 2
            );

            g.DrawArc(swooshPen, swooshBounds, angle - 45, 45); // Adjust angles for the arc
        }

        internal void DrawMageEffect(Graphics g) {
            using Pen glowPen = new Pen(Color.DarkCyan, 10); // White glow, thicker line
            using Pen effectPen = new Pen(Color.Red, 6);  // Red effect line

            glowPen.StartCap = LineCap.Round;
            glowPen.EndCap = LineCap.Round;
            effectPen.StartCap = LineCap.Round;
            effectPen.EndCap = LineCap.Round;

            // Calculate current position along the line based on progress
            float currentX = startPoint.X + (endPoint.X - startPoint.X) * effectProgress;
            float currentY = startPoint.Y + (endPoint.Y - startPoint.Y) * effectProgress;

            // Draw the glow line
            g.DrawLine(glowPen, startPoint.X, startPoint.Y, currentX, currentY);

            // Draw the main red line over the glow
            g.DrawLine(effectPen, startPoint.X, startPoint.Y, currentX, currentY);
        }

        internal void DrawRangedEffect(Graphics g) {
            // Calculate the current angle based on the progress (effectProgress is from 0.0 to 1.0)
            float angle = 360f * effectProgress;

            // Convert angle to radians
            float angleInRadians = (float)(angle * Math.PI / 180);

            // Calculate the current position along the circular path
            float x = startPoint.X + swooshRadius * (float)Math.Cos(angleInRadians);
            float y = startPoint.Y + swooshRadius * (float)Math.Sin(angleInRadians);


            using Pen swooshPen = new Pen(Color.Red, 6);
            swooshPen.StartCap = LineCap.Round;
            swooshPen.EndCap = LineCap.Round;

            // Draw a line from the swoosh center to the current position
            g.DrawLine(swooshPen, startPoint.X, startPoint.Y, x, y);

            // Optionally, draw an arc segment
            RectangleF swooshBounds = new RectangleF(
                startPoint.X - swooshRadius,
                startPoint.Y - swooshRadius,
                swooshRadius * 2,
                swooshRadius * 2
            );

            g.DrawArc(swooshPen, swooshBounds, angle - 45, 45); // Adjust angles for the arc
        }

        private void AttackTimer_Tick(object? sender, EventArgs e) {
            if (effectProgress < 1.0f) {
                effectProgress += effectSpeed; // Increase the progress of the swoosh
            } else {
                attackTimer.Stop(); // Stop the animation when it finishes
                Attacker.CurrentState = ICharacter.State.Idle;

                // Trigger blood splash effect on target
                TriggerBloodSplashEffect(Target);
            }
            RefreshPage();
        }
    }

}