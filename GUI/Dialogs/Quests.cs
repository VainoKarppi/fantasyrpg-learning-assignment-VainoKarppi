using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private void StartQuest() {
        if (player.QuestList.FirstOrDefault(x => x.Name == "Massacare") == null) {
            IQuest quest = new Quests.Massacare(player);
            player.AddQuest(quest);

            // Show message that the player has received a new quest
            string questMessage = $"New Quest: {quest.Name}\n\n{quest.Description}";
            string dialogMessage = "Good luck with your mission! Remember, you can check your quest progress anytime in the quest menu.";
            
            // Show the quest details and dialog message
            MessageBox.Show($"{questMessage}\n\n{dialogMessage}", "New Quest Acquired", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}