using System.Drawing;


namespace GUI;


public partial class GameForm : Form {
    private void DrawTopBar(Graphics g) {
        // Draw grey background for TopBar

        // Draw a grey background for the whole screen along the x-axis, and 40 pixels in height on the y-axis
        using (Brush backgroundBrush = new SolidBrush(Color.LightGray)) {
            // Fill a rectangle across the top of the screen
            g.FillRectangle(backgroundBrush, 0, 0, Width, TopBarHeight); // Full width, 40px height
        }

        // Draw current world name at the center of the grey background
        string worldName = Player.CurrentWorld!.Name;
        using Font font = new Font("Arial", 16, FontStyle.Bold); // Adjust font size and style
        using Brush brush = new SolidBrush(Color.Black);

        // Calculate position to center the text horizontally
        SizeF textSize = g.MeasureString(worldName, font);
        float textX = (Width - textSize.Width) / 2; // Center horizontally
        float textY = (40 - textSize.Height) / 2; // Center vertically within the 40px height

        // Draw the text on top of the grey background
        g.DrawString(worldName, font, brush, textX, textY);
    }
    private void DrawPlayerStatus(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;

        // Draw background for stats bar
        g.FillRectangle(Brushes.Black, 0, statsBarTop, ClientSize.Width, StatsBarHeight);

        // Draw stats text
        var font = new Font("Arial", 10, FontStyle.Bold);
        var brush = Brushes.White;
        int padding = 10;


        string healthText = $"Health: {Player.Health}";
        string moneyText = $"Money: {Player.Money}";
        string manaText = $"Mana: {Player.Mana}";
        string weaponText = $"Weapon: {Player.CurrentWeapon?.Name}";
        string armorText = $"Armor: {Player.CurrentArmor?.Name}";

        g.DrawString(healthText, font, brush, padding, statsBarTop + padding);
        g.DrawString(moneyText, font, brush, padding, statsBarTop + padding + 20);
        g.DrawString(manaText, font, brush, padding, statsBarTop + padding + 40);
        g.DrawString(weaponText, font, brush, padding, statsBarTop + padding + 60);
        g.DrawString(armorText, font, brush, padding, statsBarTop + padding + 80);
    }
    private void DrawQuestStatus(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;
        int questBoxLeft = ClientSize.Width / 4;
        int questBoxWidth = ClientSize.Width / 4;

        // Draw quest and inventory background
        g.FillRectangle(Brushes.DarkGray, questBoxLeft, statsBarTop, questBoxWidth, StatsBarHeight);

        var font = new Font("Arial", 10, FontStyle.Bold);
        var brush = Brushes.White;
        int padding = 10;

        // Display current quest
        string? questName = Player.CurrentQuest?.Name;
        string? questDescription = Player.CurrentQuest?.Description;
        string? questStageDescription = Player.CurrentQuest?.StageDescription;

        if (questName is null) questName = "No Quest Active!";

        string text = questName;
        if(questDescription != null) text += $" - ({questDescription})";
        g.DrawString($"Current Quest:\n{text}", font, brush, questBoxLeft + padding, statsBarTop + padding);
        
        if (questStageDescription != null) {
            g.DrawString($"\nStatus: {questStageDescription}", font, brush, questBoxLeft + padding, statsBarTop + padding + 20);
        }
    }
    private void DrawInventory(Graphics g) {
        int statsBarTop = ClientSize.Height - StatsBarHeight;
        int inventoryBoxLeft = ClientSize.Width / 2; // Left edge of the third box (left half of the stats bar)
        int inventoryBoxWidth = ClientSize.Width / 4; // Only one quarter of the width

        // Draw inventory background
        g.FillRectangle(Brushes.Black, inventoryBoxLeft, statsBarTop, inventoryBoxWidth, StatsBarHeight);

        var font = new Font("Arial", 10, FontStyle.Bold);
        var brush = Brushes.White;
        int padding = 10;

        // Display inventory title
        string inventoryTitle = "Inventory:";
        g.DrawString(inventoryTitle, font, brush, inventoryBoxLeft + padding, statsBarTop + padding);

        // Group items by name and count their quantities
        var groupedItems = Player.InventoryItems
            .GroupBy(item => item.Name)
            .Select(group => new { Name = group.Key, Count = group.Count() });

        // Draw grouped inventory items
        int inventoryY = statsBarTop + padding + 20;
        foreach (var groupedItem in groupedItems) {
            // Format item display as "ItemName xCount" if quantity > 1
            string? itemDisplay = groupedItem.Count > 1 
                ? $"{groupedItem.Name} (x{groupedItem.Count})" 
                : groupedItem.Name;

            if (itemDisplay == null) continue;

            g.DrawString(itemDisplay, font, brush, inventoryBoxLeft + padding, inventoryY);
            inventoryY += 20;

            // Ensure items fit within the inventory box
            if (inventoryY > statsBarTop + StatsBarHeight - 20) break;
        }
    }


    private List<Button> Buttons = new List<Button>();

    private void InitializeButtons() {
        int paddingRight = 10;  // Padding to the right side of the button area
        int buttonAreaWidth = ScreenWidth / 4 - paddingRight;   // 1/4 of the screen width minus padding
        int buttonAreaHeight = ScreenHeight / 4 + 13;           // 1/4 of the screen height
        int buttonAreaLeft = ScreenWidth - buttonAreaWidth - paddingRight; // Right side with padding
        int buttonAreaTop = ScreenHeight - buttonAreaHeight;    // Bottom of the screen

        // Define button list with action handlers
        var buttons = new List<(string Name, Action Action)> {
            ("Help (H)", ShowHelp),
            ("Change Weapon (C)", ChangeWeapon),
            ("Change Armor (K)", ChangeArmor),
            ("Use Potion (P)", UsePotion),
            ("Change Quest (Q)", ChangeQuest),
            ("Stats (N)", ShowStats)
        };

        int buttonPadding = 5;   // Reduced padding for closer spacing between buttons
        int buttonWidth = buttonAreaWidth - 20;   // Width minus padding for button text alignment
        int buttonHeight = 28;    // Height of each button

        int currentY = buttonAreaTop + buttonPadding;  // Start position for buttons

        // Create the buttons once and store them in the list
        foreach (var (name, action) in buttons) {
            Button button = new Button {
                Text = name,
                Width = buttonWidth,
                Height = buttonHeight,
                Location = new Point(buttonAreaLeft + 10, currentY) // Place on the right bottom with padding
            };

            // Handle button click event
            button.Click += (sender, e) => action?.InvokeFireAndForget();

            // Add the button to the list (not to Controls yet)
            Buttons.Add(button);

            // Update the Y-position for the next button
            currentY += buttonHeight + buttonPadding; // Vertical spacing for buttons
        }
    }
    private static bool buttonsDrawn = false;


    private void DrawButtons(Graphics g) {
        int paddingRight = 10; // Padding to the right side of the button area

        int buttonAreaWidth = ScreenWidth / 4 - paddingRight;   // 1/4 of the screen width minus padding
        int buttonAreaHeight = ScreenHeight / 4 + 14;           // 1/4 of the screen height
        int buttonAreaLeft = ScreenWidth - buttonAreaWidth - paddingRight; // Right side with padding
        int buttonAreaTop = ScreenHeight - buttonAreaHeight;    // Bottom of the screen

        // Draw button area background
        g.FillRectangle(Brushes.Gray, buttonAreaLeft, buttonAreaTop, buttonAreaWidth, buttonAreaHeight);

        DrawMultiplayerStatus(g);

        // Keep drawing the background, but dont redrawn the buttons
        if (buttonsDrawn) return;
        buttonsDrawn = true;


        // Draw black padding area to the right
        g.FillRectangle(Brushes.Black, buttonAreaLeft + buttonAreaWidth, buttonAreaTop, paddingRight, buttonAreaHeight);

        // Draw each button (from the pre-created list)
        foreach (var button in Buttons) {
            button.Location = new Point(buttonAreaLeft + 10, button.Location.Y);  // Adjust position if necessary
            button.Size = new Size(buttonAreaWidth - 20, 28);  // Adjust size if necessary

            button.GotFocus += (s, e) => ActiveControl = null; // Disable spacebar/tab select button

            // Only add the button to the controls collection once (if it's not added already)
            if (!Controls.Contains(button)) {
                Controls.Add(button);
            }
        }

        DrawMultiplayerButton();
    }


    // Button actions
    private void ShowHelp() {
        // Show help asynchronously
        MessageBox.Show("This is the Help menu. Provide instructions here.", "Help");
        // After the message box closes, ensure the game form gets focus again
        Focus();
    }

    private void OpenItemChangeForm<T>(string formTitle, List<T> items, Action<T> onItemSelected, string itemType = "this type of items", string selectText = "Equip") where T : class {
        // Create a new form to list the player's inventory items
        Form changeForm = new Form() {
            Text = formTitle,
            ClientSize = new Size(400, 500), // Adjust the size
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterScreen
        };

        try {
            // Create a FlowLayoutPanel to list the items and buttons
            FlowLayoutPanel itemPanel = new FlowLayoutPanel() {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            changeForm.Controls.Add(itemPanel);

            if (items.Count == 0) throw new Exception($"No {itemType} in inventory!");

            // Create buttons for each item and add them to the panel
            foreach (var item in items) {
                // Create a new FlowLayoutPanel for each item row
                FlowLayoutPanel itemRow = new FlowLayoutPanel() {
                    FlowDirection = FlowDirection.LeftToRight,
                    Width = itemPanel.ClientSize.Width - 20,
                    Height = 50,
                    AutoSize = true,
                    WrapContents = false,
                    Padding = new Padding(5)
                };

                // Create label based on item type (Weapon, Armor, Potion, Quest)
                Label itemLabel = new Label() {
                    Text = GetItemLabel(item),
                    Width = 300, // Label width
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Button to select the item
                Button selectItemButton = new Button() {
                    Text = selectText,
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // Button click event to handle item selection
                selectItemButton.Click += (sender, e) => {
                    onItemSelected(item); // Trigger the item action
                    changeForm.Close(); // Close the form
                };

                // Add the label and button to the row
                itemRow.Controls.Add(itemLabel);
                itemRow.Controls.Add(selectItemButton);

                // Add the row to the panel
                itemPanel.Controls.Add(itemRow);
            }

            // Show the form modally to wait for user selection
            changeForm.ShowDialog();
        } catch (Exception ex) {
            MessageBox.Show(ex.Message);
            changeForm.Close(); // Close the form if an error occurs
        }
    }

    private string GetItemLabel<T>(T item) {
        if (item is ItemWeapon weapon)
            return $"{weapon.Name} (Attack Power: {weapon.Damage})";
        else if (item is ItemArmor armor)
            return $"{armor.Name} (Defense: {armor.Defense})";
        else if (item is ItemPotion potion)
            return $"{potion.Name} (+{potion.Effect})";
        else if (item is IQuest quest)
            return $"{quest.Name} ({quest.Description})";
        else
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
        OpenItemChangeForm("Use Potion", Player.GetInventoryPotions(), Player.PlayerActions.UsePotion, "potions", "Use");
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