public abstract class QuestBase : IQuest {
    public virtual string Name { get; set; }
    public Player QuestOwner { get; set; }

    public bool Completed { get; set; } = false;


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
            Completed = true;
            Quests.TriggerQuestEndedEvent(this, true);
            HandleRewards();

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
        OnQuestCreated?.InvokeFireAndForget(quest);
    }

    public static void TriggerQuestEndedEvent(IQuest quest, bool success) {
        OnQuestEnded?.InvokeFireAndForget(quest, success);
    }

    public static void TriggerQuestUpdatedEvent(IQuest quest) {
        OnQuestUpdated?.InvokeFireAndForget(quest);
    }
}