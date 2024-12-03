using System.Drawing;

namespace GUI;


public partial class GameForm : Form {
    private void MassacareQuest() {
        // check if quest has already been completed
        if (player.CompletedQuests.Contains("Massacare")) return;

        // Only start the quest, if not added already
        if (player.QuestList.FirstOrDefault(x => x.Name == "Massacare") == null) {
            IQuest quest = new Quests.Massacare(player);
            player.AddQuest(quest);

            // Show message that the player has received a new quest
            string questMessage = $"New Quest: {quest.Name}\n\n{quest.Description}";
            string dialogMessage = "Good luck with your mission! Remember, you can check your quest progress anytime in the quest menu.\nOnce you have completed the quest, return to me warrior!";
            
            // Show the quest details and dialog message
            MessageBox.Show($"{questMessage}\n\n{dialogMessage}", "New Quest Acquired", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Invalidate();
        }
        

        // Check if last stage and returning to start
        if (player.CurrentQuest?.Name == "Massacare" && player.CurrentQuest.CurrentStageIndex == 3) {
            // Quest completed (update it to last stage)!
            player.CurrentQuest.UpdateStageStatus();

            string questMessage = $"Quest: {player.CurrentQuest?.Name} Completed!";
            string dialogMessage = "Good job Warrior!\nI added the reward to your inventory. Thanks again!";

            MessageBox.Show($"{questMessage}\n\n{dialogMessage}", "Quest Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Invalidate();
        }
    }
}