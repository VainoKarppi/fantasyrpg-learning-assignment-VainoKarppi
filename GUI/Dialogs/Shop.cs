using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private void OpenShopBuy() {
        if (Shop is null) return;

        // Create a new form for the Shop interface
        Form shopForm = new Form() {
            Text = $"Shop - Buy",
            ClientSize = new Size(350, 500), // Define the size of the shop window
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterScreen
        };

        // Create a FlowLayoutPanel for dynamic item buttons
        FlowLayoutPanel itemPanel = new FlowLayoutPanel() {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        shopForm.Controls.Add(itemPanel);


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
                if (player.Money >= item.BuyPrice) {
                    MessageBox.Show($"You bought {item.Name} for {item.BuyPrice} Coins.\nRemaining Money: {player.Money}");
                    Shop.BuyItem(player, item);
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
        shopForm.ShowDialog();
    }

    private void OpenShopSell() {
        if (Shop is null) return;

        // Create a new form for the Shop interface
        Form shopForm = new Form() {
            Text = $"Shop - Sell",
            ClientSize = new Size(350, 500), // Define the size of the shop window
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            StartPosition = FormStartPosition.CenterScreen
        };

        // Create a FlowLayoutPanel for dynamic item buttons
        FlowLayoutPanel itemPanel = new FlowLayoutPanel() {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };
        shopForm.Controls.Add(itemPanel);


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
                if (player.Money >= item.BuyPrice) {
                    MessageBox.Show($"You bought {item.Name} for {item.BuyPrice} Coins.\nRemaining Money: {player.Money}");
                    Shop.BuyItem(player, item);
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
        shopForm.ShowDialog();
    }
}