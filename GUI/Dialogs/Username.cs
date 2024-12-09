namespace GUI;


public partial class GameForm : Form {
    private static string PromptForUsername() {
        using var form = new Form();

        form.Text = "Enter Username";
        form.StartPosition = FormStartPosition.CenterScreen;
        form.FormBorderStyle = FormBorderStyle.FixedSingle;
        form.Width = 300;
        form.Height = 150;
        form.MaximizeBox = false;
        form.MinimizeBox = false;

        // Label
        var label = new Label {
            Text = "Please enter username:",
            Left = 10,
            Top = 10,
            Width = 260
        };
        form.Controls.Add(label);

        // TextBox
        var textBox = new TextBox {
            Left = 10,
            Top = 40,
            Width = 260,
            Text = "Player"
        };
        form.Controls.Add(textBox);

        // OK Button
        var okButton = new Button {
            Text = "OK",
            Left = 100,
            Width = 80,
            Top = 80,
            DialogResult = DialogResult.OK
        };
        form.Controls.Add(okButton);

        form.AcceptButton = okButton; // Trigger OK on Enter key press

        // Show Dialog and Validate Input
        while (true) {
            var result = form.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text)) {
                return textBox.Text;
            }

            if (result != DialogResult.OK) {
                Environment.Exit(1); // X -> Close game
            }
        }
    }
}