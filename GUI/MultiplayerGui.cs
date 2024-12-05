using System.Net;
using System.Net.Sockets;


namespace GUI;


public partial class GameForm : Form {

    private void DrawMultiplayerStatus(Graphics g) {
        // Draw the multiplayer status text at the top-right below the multiplayer button    
        if (MultiplayerClient.Client != null) {
            if (!MultiplayerClient.Client.Connected || MultiplayerClient.Client.Client.RemoteEndPoint is null) return;
            IPEndPoint? remoteEndPoint = MultiplayerClient.Client.Client.LocalEndPoint as IPEndPoint;
            if (remoteEndPoint is null) return;

            string address = remoteEndPoint.Address.AddressFamily == AddressFamily.InterNetworkV6
                ? remoteEndPoint.Address.MapToIPv4().ToString()
                : remoteEndPoint.Address.ToString();


            string Mode = MultiplayerServer.IsHost() ? "Hosting" : "Connected";

            string status = $"{Mode}: {address}:{remoteEndPoint.Port}";
            Font statusFont = new Font("Arial", 10, FontStyle.Bold);
            Brush statusBrush = Brushes.Gray;

            // Calculate the position
            int statusX = ScreenWidth - 310; // Adjust for padding
            int statusY = TopBarHeight / 2; // Slightly below the multiplayer button

            g.DrawString(status, statusFont, statusBrush, statusX, statusY);
        }
    }

    private void DrawMultiplayerButton() {
        // Add multiplayer button to the top-right
        Button multiPlayer = new Button {
            Text = "Multiplayer",
            Location = new Point(ScreenWidth - 130, 5), // Positioned at the top-right with padding
            Size = new Size(110, 30)
        };

        // Add click event if needed
        multiPlayer.Click += (sender, e) => {
            ShowMultiplayerDialog();
        };

        // Add the multiplayer button to controls if not already present
        if (!Controls.Contains(multiPlayer)) {
            Controls.Add(multiPlayer);
        }
    }


    private void ShowMultiplayerDialog() {
        // Create a new form for the options
        Form multiplayerForm = new Form {
            Text = "Multiplayer Options",
            Size = new Size(300, 150),
            StartPosition = FormStartPosition.CenterParent
        };

        // Create a label for the prompt
        Label promptLabel = new Label {
            Text = "Do you want to Join or Host a multiplayer session?",
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Top,
            Height = 50
        };
        

        // Create a Join button
        Button joinButton = new Button {
            Text = "Join",
            Size = new Size(100, 30),
            Location = new Point(50, 70)
        };
        joinButton.Click += (sender, e) => {
            try {
                multiplayerForm.Enabled = false;
                string? ipAddress = PromptForIPAddress() ?? throw new Exception("Invalid IP address!");
                
                MultiplayerClient.Connect(ipAddress, 7842, Player!);

                // Start over
                GameInstance.RemoveAllNpcs();
                Player.ChangeWorld("Home");
                Player.X = ScreenWidth / 2;;
                Player.Y = (ScreenHeight - StatsBarHeight) / 2;
            } catch (Exception ex) {
                if (ex is SocketException) {
                    Console.WriteLine(ex.Message);
                    MessageBox.Show($"Failed to connect to server!");
                }
            }

            multiplayerForm.Close();
            Invalidate();
        };

        // Create a Join button
        Button disconnectButton = new Button {
            Text = "Disconnect",
            Size = new Size(100, 30),
            Location = new Point(50, 70)
            
        };
        disconnectButton.Click += (sender, e) => {
            MultiplayerClient.Disconnect();

            if (MultiplayerServer.IsHost()) MultiplayerServer.Stop();

            MultiplayerClient.OtherPlayers.Clear();

            multiplayerForm.Close();
            Invalidate();
        };
        

        // Create a Host button
        Button hostButton = new Button {
            Text = "Host",
            Size = new Size(100, 30),
            Location = new Point(160, 70)
        };
        hostButton.Click += (sender, e) => {
            // Start over
            Player!.ChangeWorld("Home");
            Player.X = ScreenWidth / 2;;
            Player.Y = (ScreenHeight - StatsBarHeight) / 2;

            new Thread(() => MultiplayerServer.Start("0.0.0.0", 7842)).Start();
            MultiplayerClient.Connect("127.0.0.1", 7842, Player); // Locally connect to self host server
            multiplayerForm.Close(); // Close the form after selection
            Invalidate();
            // Add logic to host a session here
        };


        if (MultiplayerClient.Client == null) {
            multiplayerForm.Controls.Add(promptLabel);
            multiplayerForm.Controls.Add(joinButton);
            multiplayerForm.Controls.Add(hostButton);
        } else {
            multiplayerForm.Controls.Add(disconnectButton);
        }

        // Show the form as a dialog
        multiplayerForm.ShowDialog();
    }

    private string? PromptForIPAddress() {
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Please enter the IP Address to connect:", 
            "Join Server", 
            "127.0.0.1");
        return IsValidHostOrIPAddress(input) ? input : null;
    }

    private bool IsValidHostOrIPAddress(string input) {
        // Check if it's a valid IP Address
        if (IPAddress.TryParse(input, out _))
            return true;

        // Check if it's a valid DNS Hostname
        var hostNameType = Uri.CheckHostName(input);
        return hostNameType == UriHostNameType.Dns;
    }
}