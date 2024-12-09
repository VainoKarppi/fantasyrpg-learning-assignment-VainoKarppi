namespace GUI;

public partial class GameForm : Form {

    private List<Button> Buttons = new List<Button>();

    private static readonly Brush ButtonBackgroundBrush = Brushes.Gray;
    private static readonly Brush WhiteBrush = Brushes.White;
    private static readonly Brush BlackBrush = Brushes.Black;
    private static readonly Font ButtonFont = new Font("Arial", 10, FontStyle.Bold);

    // Button creation and initialization logic.
    private void InitializeButtons() {
        int paddingRight = 10;
        int buttonAreaWidth = ScreenWidth / 4 - paddingRight;
        int buttonAreaHeight = ScreenHeight / 4 + 13;
        int buttonAreaLeft = ScreenWidth - buttonAreaWidth - paddingRight;
        int buttonAreaTop = ScreenHeight - buttonAreaHeight;

        // Define buttons with their action handlers.
        var buttons = new List<(string Name, Action Action)> {
            ("Help (H)", ShowHelp),
            ("Change Weapon (C)", ChangeWeapon),
            ("Change Armor (K)", ChangeArmor),
            ("Use Potion (P)", UsePotion),
            ("Change Quest (Q)", ChangeQuest),
            ("Stats (N)", ShowStats)
        };

        int buttonPadding = 5;
        int buttonWidth = buttonAreaWidth - 20;
        int buttonHeight = 28;
        int currentY = buttonAreaTop + buttonPadding;

        foreach (var (name, action) in buttons) {
            Button button = CreateButton(name, buttonWidth, buttonHeight, buttonAreaLeft, ref currentY, action);
            Buttons.Add(button);
        }
    }

    private Button CreateButton(string text, int width, int height, int left, ref int top, Action action) {
        Button button = new Button {
            Text = text,
            Width = width,
            Height = height,
            Location = new Point(left + 10, top)
        };

        // Attach the action to the button click.
        button.Click += (sender, e) => action.Invoke();

        top += height + 5; // Adjust top for next button placement.

        return button;
    }

    private static bool buttonsDrawn = false;

    private void DrawButtons(Graphics g) {
        int paddingRight = 10;
        int buttonAreaWidth = ScreenWidth / 4 - paddingRight;
        int buttonAreaHeight = ScreenHeight / 4 + 14;
        int buttonAreaLeft = ScreenWidth - buttonAreaWidth - paddingRight;
        int buttonAreaTop = ScreenHeight - buttonAreaHeight;

        // Draw button area background.
        g.FillRectangle(ButtonBackgroundBrush, buttonAreaLeft, buttonAreaTop, buttonAreaWidth, buttonAreaHeight);

        DrawMultiplayerStatus(g);

        // Draw the buttons only once.
        if (buttonsDrawn) return;
        buttonsDrawn = true;

        g.FillRectangle(BlackBrush, buttonAreaLeft + buttonAreaWidth, buttonAreaTop, paddingRight, buttonAreaHeight);

        foreach (var button in Buttons) {
            button.Location = new Point(buttonAreaLeft + 10, button.Location.Y);  // Adjust position if necessary
            button.Size = new Size(buttonAreaWidth - 20, 28);  // Adjust size if necessary

            // Disable spacebar/tab select.
            button.GotFocus += (s, e) => ActiveControl = null;

            // Only add the button to the controls collection once.
            if (!Controls.Contains(button)) Controls.Add(button);
        }

        DrawMultiplayerButton();
    }

    // Button Actions.
    private void ShowHelp() {
        MessageBox.Show("This is the Help menu. Provide instructions here.", "Help");
        Focus(); // Refocus game form.
    }

    private void OpenItemChangeForm<T>(string formTitle, List<T> items, Action<T> onItemSelected, string itemType = "this type of items", string selectText = "Equip") where T : class {
        Form changeForm = CreateChangeForm(formTitle, items, itemType);

        try {
            FlowLayoutPanel itemPanel = CreateItemPanel(items, onItemSelected, selectText);
            changeForm.Controls.Add(itemPanel);

            changeForm.ShowDialog();
        } catch (Exception ex) {
            MessageBox.Show(ex.Message);
            changeForm.Close();
        }
    }

    private FlowLayoutPanel CreateItemPanel<T>(List<T> items, Action<T> onItemSelected, string selectText) where T : class {
        var itemPanel = new FlowLayoutPanel() {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        foreach (var item in items) {
            var itemRow = new FlowLayoutPanel() {
                FlowDirection = FlowDirection.LeftToRight,
                Width = itemPanel.ClientSize.Width - 20,
                Height = 50,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(5)
            };

            string label = GetItemLabel(item);
            var itemLabel = new Label() { Text = label, Width = 300, TextAlign = ContentAlignment.MiddleLeft };
            var selectItemButton = new Button() { Text = selectText, AutoSize = true, TextAlign = ContentAlignment.MiddleCenter };

            selectItemButton.Click += (sender, e) => {
                onItemSelected(item);
                itemPanel.Parent?.Dispose(); // Close the form.
            };

            itemRow.Controls.Add(itemLabel);
            itemRow.Controls.Add(selectItemButton);

            itemPanel.Controls.Add(itemRow);
        }

        return itemPanel;
    }

    private Form CreateChangeForm<T>(string formTitle, List<T> items, string itemType) {
        Form changeForm = new Form() {
            Text = formTitle,
            ClientSize = new Size(400, 500),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterScreen
        };

        if (items.Count == 0) throw new Exception($"No {itemType} in inventory!");

        return changeForm;
    }

    private string GetItemLabel<T>(T item) {
        if (item is ItemWeapon weapon)
            return $"{weapon.Name} (Attack Power: {weapon.Damage})";
        if (item is ItemArmor armor)
            return $"{armor.Name} (Defense: {armor.Defense})";
        if (item is ItemPotion potion)
            return $"{potion.Name} (+{potion.Effect})";
        if (item is IQuest quest)
            return $"{quest.Name} ({quest.Description})";

        return "Unknown Item";
    }

    private void ChangeWeapon() {
        OpenItemChangeForm("Change Weapon", Player.GetInventoryWeapons(), Player.ChangeWeapon, "weapons");
        Invalidate();
    }

    private void ChangeArmor() {
        OpenItemChangeForm("Change Armor", Player.GetInventoryArmors(), Player.ChangeArmor, "armors");
        Invalidate();
    }

    private void UsePotion() {
        OpenItemChangeForm("Use Potion", Player.GetInventoryPotions(), Player.Actions.UsePotion, "potions", "Use");
        Invalidate();
    }

    private void ChangeQuest() {
        var availableQuests = Player.QuestList.Where(q => q != Player.CurrentQuest).ToList();

        if (availableQuests.Count == 0) {
            MessageBox.Show("No other quests available!");
            return;
        }

        OpenItemChangeForm("Change Quest", availableQuests, (quest) => {
            Player.CurrentQuest = quest;
        }, "quests", "Select");
        Invalidate();
    }

    private static void ShowStats() {
        var statsInfo = string.Join(Environment.NewLine,
            Player.PlayerStatistics.GetType()
                .GetProperties()
                .Select(prop => $"{prop.Name}: {prop.GetValue(Player.PlayerStatistics)}"));

        MessageBox.Show(statsInfo, "Stats");
    }
}
