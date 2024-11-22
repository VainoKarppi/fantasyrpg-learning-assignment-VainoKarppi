public interface IQuest {
    string Name { get; }
    string Description { get; }
    string StageDescription => MissionStages[CurrentStageIndex].StageDescription;
    int CurrentStageIndex { get; set; }

    Dictionary<ItemBase, int> RewardList { get; }
    List<IQuestStage> MissionStages { get; }
    Player QuestOwner { get; }
    
    void HandleRewards();
    void UpdateStageStatus();
}

public interface IQuestStage {
    string StageDescription { get; }
}
