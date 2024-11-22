





public partial class Quests {
    public class Massacare : QuestBase {
  
        public override string Description => "Kill 3 enemies";

        public Massacare(Player questOwner) : base(questOwner) {
            NpcCharacter.OnNpcKilled += HandleNpcKilled;

            // Add stages
            MissionStages.AddRange([
                new Stage("3 enemies left!"),
                new Stage("2 enemies left!"),
                new Stage("1 enemy left!")
            ]);

            // Add rewards
            RewardList = new() {
                { new MeleeWeapon.LegendarySword(), 1},
                { new ItemDrop.Gems.Diamond(), 2}
            };
        }

        private void HandleNpcKilled(NpcCharacter npc, Player? player) {
            if (player != null) {
                if (player.QuestList.Contains(this)) {
                    UpdateStageStatus();
                } else {
                    // Unsubscribe event
                    NpcCharacter.OnNpcKilled -= HandleNpcKilled;
                }
            }
        }
    }
}
