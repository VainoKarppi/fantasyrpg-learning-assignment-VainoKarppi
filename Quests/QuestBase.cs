public abstract class QuestBase : IQuest {
    public virtual string Name { get; set; }
    public Player QuestOwner { get; set; }


    public virtual string Description { get; set; } = "No description available!";
    public virtual int CurrentStageIndex { get; set; } = 0;
    public virtual Dictionary<ItemBase, int> RewardList { get; set; } = new();
    public virtual List<IQuestStage> MissionStages { get; set; } = new();


    public QuestBase(Player questOwner) {
        Name = GetType().Name;
        QuestOwner = questOwner;

        Quests.TriggerQuestCreatedEvent(this);
    }

    public virtual void HandleRewards() {
        foreach (KeyValuePair<ItemBase, int> reward in RewardList) {
            for (int i = 0; i < reward.Value; i++) {
                QuestOwner.AddItem(reward.Key);
            }
        }
    }

    public virtual void HandleQuestEnded() {
        
    }

    public virtual void UpdateStageStatus() { 
        CurrentStageIndex++; // Increment to next stage

        if (CurrentStageIndex == MissionStages.Count) {
            Quests.TriggerQuestEndedEvent(this, true);
            HandleRewards();

            // TODO Decide if this should be handled automatically or inside an event?
            QuestOwner.RemoveQuest(this);

            // Mark quest as completed
            QuestOwner.CompletedQuests.Add(Name);
        } else {
            Quests.TriggerQuestUpdatedEvent(this);
        }
    }

    internal class Stage(string description) : IQuestStage {
        public string StageDescription { get; } = description;
    }
}


// ADD BASE EVENTS FOR QUESTS
public partial class Quests {
    // EVENTS
    public static event Action<IQuest>? OnQuestCreated;
    public static event Action<IQuest, bool>? OnQuestEnded;
    public static event Action<IQuest>? OnQuestUpdated;

    public static void TriggerQuestCreatedEvent(IQuest quest) {
        OnQuestCreated?.Invoke(quest);
    }

    public static void TriggerQuestEndedEvent(IQuest quest, bool success) {
        OnQuestEnded?.Invoke(quest, success);
    }

    public static void TriggerQuestUpdatedEvent(IQuest quest) {
        OnQuestUpdated?.Invoke(quest);
    }
}