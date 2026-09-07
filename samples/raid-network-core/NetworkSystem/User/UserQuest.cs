using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using ProjectRaid.Quest;

public class UserQuest : NetworkBehaviour
{
    private readonly SyncDictionary<int, Quest> activeQuests = new();
    private readonly SyncList<int> completedQuestIds = new();

    public readonly SyncDictionary<int, QuestDTO> activeQuestsDTO = new();

    public IReadOnlyDictionary<int, Quest> ActiveQuests => activeQuests;
    public IReadOnlyList<int> CompletedQuestIds => completedQuestIds;

    private UserManager userManager;

    [Server]
    public void Init(UserManager userManager, UserQuestData userQuestData)
    {
        this.userManager = userManager;

        foreach(var quest in userQuestData.activeQuests.Values)
        {
            AddActiveQuest(quest.questId);
        }
        foreach(var questID in userQuestData.completedQuestIds)
        {
            AddCompletedQuest(questID);
        }
    }

    public override void OnStartClient()
    {
        activeQuestsDTO.OnChange += OnTest;
    }

    private void OnTest(SyncDictionaryOperation op, int key, QuestDTO value, bool asServer)
    {
        var a = activeQuestsDTO.Values;
    }

    [Server]
    public void AddActiveQuest(int questID)
    {
        if(activeQuests.ContainsKey(questID)) return;
        Managers.Data.QuestDataLoader.TryGetByID(questID, out var questData);
        AddActiveQuest(questData);
    }

    [Server]
    public void AddActiveQuest(Quest quest)
    {
        if(activeQuests.ContainsKey(quest.ID)) return;

        activeQuests[quest.ID]    = quest.Clone();
        activeQuestsDTO[quest.ID] = quest.ToDTO();

        quest.onCompleted           += CompleteActiveQuest;
        quest.onSuccessCountChanged += UpdateActiveQuest;

        quest.Init(userManager.Owner.ClientId);
    }

    [Server]
    private void UpdateActiveQuest(Quest quest)
    {
        activeQuestsDTO[quest.ID] = quest.ToDTO();
    }

    [Server]
    public void AddCompletedQuest(int questID) => completedQuestIds.Add(questID);

    [Server]
    public void AddCompletedQuest(Quest quest) => AddCompletedQuest(quest.ID);

    [Server]
    public void RemoveActiveQuest(int questID)
    {
        if(!activeQuests.ContainsKey(questID)) return;

        activeQuests[questID].onCompleted           -= CompleteActiveQuest;
        activeQuests[questID].onSuccessCountChanged -= UpdateActiveQuest;
        activeQuests[questID].Dispose();
        
        activeQuests.Remove(questID);
        activeQuestsDTO.Remove(questID);
    }

    [Server]
    public void RemoveActiveQuest(Quest quest) => RemoveActiveQuest(quest.ID);

    [Server]
    public void RemoveCompletedQuest(int questID)
    {
        if(!completedQuestIds.Contains(questID)) return;
        completedQuestIds.Remove(questID);
    }

    [Server]
    public void RemoveCompletedQuest(Quest quest) => RemoveCompletedQuest(quest.ID);

    [Server]
    public void CompleteActiveQuest(int questID)
    {
        if(completedQuestIds.Contains(questID)) return;
        completedQuestIds.Add(questID);
        RemoveActiveQuest(questID);
    }

    [Server]
    public void CompleteActiveQuest(Quest quest) => CompleteActiveQuest(quest.ID);
}
