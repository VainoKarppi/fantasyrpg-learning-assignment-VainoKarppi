using System.Reflection;

public interface IQuest {
    string Name { get; }
    string Description { get; }
    string StageDescription => MissionStages[CurrentStageIndex].StageDescription;
    int CurrentStageIndex { get; set; }
    bool Completed { get; set; }

    Dictionary<ItemBase, int> RewardList { get; }
    List<IQuestStage> MissionStages { get; }
    Player QuestOwner { get; }
    
    void HandleRewards();
    void UpdateStageStatus();


    public static IQuest? GetQuestByName(string name, Player questOwner) {
        if (name == null) return null;
        
        var questTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(QuestBase)) && !t.IsAbstract);

        
        foreach (var type in questTypes) {
            if (type.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                var constructor = type.GetConstructor([typeof(Player)]);
                if (constructor != null) {
                    return constructor.Invoke([questOwner]) as IQuest;
                }
            }
        }

        return null;
    }
}

public interface IQuestStage {
    string StageDescription { get; }
}
