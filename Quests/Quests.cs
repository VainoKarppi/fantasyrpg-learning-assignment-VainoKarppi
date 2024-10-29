public interface IQuest {
    string Name { get; }
    string Description { get; }
    string StageDescription => MissionStages[CurrentStageIndex].StageDescription;
    int CurrentStageIndex { get; set; }

    Dictionary<ItemBase, int> RewardList { get; }
    List<IQuestStage> MissionStages { get; }
    Player QuestOwner { get; }


    

    /// <summary>
    /// Event triggered when a quest ends, indicating whether it succeeded or failed.
    /// </summary>
    /// <remarks>
    /// The event provides the quest instance that ended and a boolean indicating success (true) or failure (false).
    /// Useful for handling quest completion or failure logic, such as awarding rewards or updating logs.
    /// </remarks>
    static event Action<IQuest, bool>? OnQuestEnded;
    static event Action<IQuest>? OnQuestCreated;
    static event Action<IQuest>? OnQuestUpdated;

    

    void HandleRewards() {
        foreach (KeyValuePair<ItemBase, int> reward in RewardList) {
            for (int i = 0; i < reward.Value; i++) {
                QuestOwner.AddItem(reward.Key);
            }
        }
    }
}

public interface IQuestStage {
    string StageDescription { get; }
}






public abstract class Quests {
    
    public class Massacare : IQuest {
        public static event Action<IQuest>? OnQuestCreated;
        public static event Action<IQuest, bool>? OnQuestEnded;
        public static event Action<IQuest>? OnQuestUpdated;

        public int CurrentStageIndex { get; set; }

        public Player QuestOwner { get; }

        
        public string Name => "Massacare";
        public string Description => "Kill 3 enemies";


        public Dictionary<ItemBase, int> RewardList { get; } = new Dictionary<ItemBase, int>() {
            { new MeleeWeapon.LegendarySword(), 1},
            { new ItemDrop.Gems.Diamond(), 2}
        };

        public Massacare(Player player) {
            NpcCharacter.OnNpcKilled += HandleNpcKilled;
            QuestOwner = player;
            
            OnQuestCreated?.Invoke(this);
        }

        private void HandleNpcKilled(Player? player, NpcCharacter npc) {
            if (player != null) {
                CurrentStageIndex++; // Handle stage logic within the component
                if (CurrentStageIndex == MissionStages.Count - 1) {
                    OnQuestEnded?.Invoke(this, true);
                }
                OnQuestUpdated?.Invoke(this);
            }
        }

        
        public List<IQuestStage> MissionStages { get; } = new() {
            new Stage1(),
            new Stage2(),
            new Stage3()
        };

        public class Stage1 : IQuestStage {
            public string StageDescription => "3 enemies left!";
        }
        
        public class Stage2 : IQuestStage {
            public string StageDescription => "2 enemies left!";
        }

        public class Stage3 : IQuestStage {
            public string StageDescription => "1 enemy left!";
        }
    }
}
