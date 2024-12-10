using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private void OpenShopMenu() {
        if (Shop is null) return;

        // Create a new form to ask for the player's choice
        Form shopMenuForm = new Form() {
            Text = "Shop Menu",
            ClientSize = new Size(250, 100), // Define size of the menu form
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterScreen
        };

        // Label to prompt the user
        Label promptLabel = new Label() {
            Text = "What would you like to do?",
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 40
        };
        shopMenuForm.Controls.Add(promptLabel);

        // Button to open "Buy" interface
        Button buyButton = new Button() {
            Text = "Buy Items",
            Width = 100,
            Height = 30,
            Top = 50,
            Left = 30
        };
        buyButton.Click += (sender, e) => {
            shopMenuForm.Hide();
            OpenShopBuy(shopMenuForm);
            shopMenuForm.Show();
        };
        shopMenuForm.Controls.Add(buyButton);

        // Button to open "Sell" interface
        Button sellButton = new Button() {
            Text = "Sell Items",
            Width = 100,
            Height = 30,
            Top = 50,
            Left = 140
        };
        sellButton.Click += (sender, e) => {
            shopMenuForm.Hide();
            OpenShopSell(shopMenuForm);
            shopMenuForm.Show();
        };
        shopMenuForm.Controls.Add(sellButton);

        // Show the menu as a modal dialog
        shopMenuForm.ShowDialog();
    }


    private void OpenShopBuy(Form shopForm) {
        if (Shop is null) return;

        // Create a new form for the Shop interface
        Form buyForm = new Form() {
            Text = $"Shop - Buy",
            ClientSize = new Size(350, 500), // Define the size of the shop window
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterScreen,
            Owner = shopForm
        };

        // Create a FlowLayoutPanel for dynamic item buttons
        FlowLayoutPanel itemPanel = new FlowLayoutPanel() {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        buyForm.Controls.Add(itemPanel);


        // Create buttons for each item and add to the FlowLayoutPanel
        foreach (IBuyable item in Shop.ItemsForSale) {
            // Create a panel for each item to contain the text and button
            FlowLayoutPanel itemRow = new FlowLayoutPanel() {
                FlowDirection = FlowDirection.TopDown,  // Stack the controls vertically
                Width = 600, // Limit width for each row
                Height = 80, // Increased height to accommodate both label and button
                AutoSize = true,  // Automatically adjust height to fit content
                WrapContents = false,  // Prevent wrapping
                Padding = new Padding(0),  // No padding around the row
                Margin = new Padding(5)  // Optional: Add margin between rows
            };

            // Label to show item name
            Label itemLabel = new Label() {
                Text = item.Name,
                Height = 30,  // Set a fixed height for the label
                AutoSize = true, // Automatically adjust label width to fit text
                TextAlign = ContentAlignment.MiddleCenter // Center the text in the label
            };

            // Button to "Buy" the item
            Button buyButton = new Button() {
                Text = $"Buy - {item.BuyPrice} Coins",
                Height = 30,  // Fixed height for the button
                AutoSize = true,  // Automatically adjust button width to fit text
                TextAlign = ContentAlignment.MiddleCenter
            };
            buyButton.Click += (sender, e) => {
                if (Player.Money >= item.BuyPrice) {
                    MessageBox.Show($"You bought {item.Name} for {item.BuyPrice} Coins.\nRemaining Money: {Player.Money}");
                    Shop.BuyItem(Player, item);
                    Invalidate();
                } else {
                    MessageBox.Show("Not enough money to buy this item.");
                }
            };

            // Add item name (centered) to the panel
            itemRow.Controls.Add(itemLabel);

            // Add buy button to the panel
            itemRow.Controls.Add(buyButton);

            // Add the row to the item panel
            itemPanel.Controls.Add(itemRow);
        }

        // Show the Shop form as a modal dialog
        buyForm.ShowDialog();    
    }

    private void OpenShopSell(Form shopForm) {
        if (Shop is null) return;

        // Create a new form for the Shop interface if not already created
        Form sellForm = new Form() {
            Text = $"Shop - Sell",
            ClientSize = new Size(350, 500), // Define the size of the shop window
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterScreen,
            Owner = shopForm
        };

        // Create a FlowLayoutPanel for dynamic item buttons
        FlowLayoutPanel itemPanel = new FlowLayoutPanel() {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        sellForm.Controls.Add(itemPanel);

        // Group items by name and count quantities
        var itemsToSell = Player.InventoryItems
            .Where(item => item is ISellable)
            .GroupBy(item => item.Name)
            .Select(group => new { Name = group.Key, Count = group.Count(), Items = group.ToList() });

        int itemsAddedToList = 0;

        // Method to update the list of items in the sell form
        void RefreshItemList() {
            if (itemsToSell.Count() == 0) sellForm.Close();

            // Clear existing controls in the item panel to refresh the item list
            itemPanel.Controls.Clear();

            foreach (var item in itemsToSell) {
                itemsAddedToList++;

                // Extract an example item from the group for sellable details
                ISellable itemToSell = (ISellable)item.Items.First();

                // Create a panel for each group to contain the text and button
                FlowLayoutPanel itemRow = new FlowLayoutPanel() {
                    FlowDirection = FlowDirection.TopDown,  // Stack the controls vertically
                    Width = itemPanel.ClientSize.Width - 30, // Limit width for each row
                    Height = 80, // Increased height to accommodate both label and button
                    AutoSize = true,  // Automatically adjust height to fit content
                    WrapContents = false,  // Prevent wrapping
                    Padding = new Padding(0),  // No padding around the row
                    Margin = new Padding(5)  // Optional: Add margin between rows
                };

                string? text = item.Name;
                if (item.Count > 1) text += $" (x{item.Count})";
                if (text == null) continue;

                // Label to show item name
                Label itemLabel = new Label() {
                    Text = text,
                    Height = 20, // Set a fixed height for the label
                    AutoSize = true, // Automatically adjust label width to fit text
                    TextAlign = ContentAlignment.MiddleCenter // Center the text in the label
                };

                // Button to "Sell" one item from the group
                Button sellButton = new Button() {
                    Text = $"Sell - {itemToSell.SellPrice} Coins",
                    Height = 30,  // Fixed height for the button
                    AutoSize = true,  // Automatically adjust button width to fit text
                    TextAlign = ContentAlignment.MiddleCenter
                };

                // Sell action
                sellButton.Click += (sender, e) => {
                    Shop.SellItem(Player, itemToSell);

                    // Refresh the item list
                    RefreshItemList();

                    // Show the sale message
                    MessageBox.Show($"You sold {item.Name} for {itemToSell.SellPrice} Coins.\nCurrent Money: {Player.Money}");

                    Invalidate();
                };

                // Add item name label
                itemRow.Controls.Add(itemLabel);

                // Add sell button
                itemRow.Controls.Add(sellButton);

                // Add the row to the item panel
                itemPanel.Controls.Add(itemRow);
            }

        }

        // Initially refresh the item list when opening the shop
        RefreshItemList();

        // Show the updated form if there are items to sell
        if (itemsAddedToList > 0) {
            sellForm.ShowDialog(); // Show the Shop form as a modal dialog
        } else {
            MessageBox.Show($"You have no items to sell!"); // Show dialog to player saying no items available to sell
        }

        // After the sellForm is done, dispose of it correctly.
        sellForm.FormClosed += (sender, e) => {
            sellForm.Dispose(); // Dispose the form to release resources
        };
    }

}