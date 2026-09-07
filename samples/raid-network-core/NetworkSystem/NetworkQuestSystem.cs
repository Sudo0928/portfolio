using FishNet.Object;
using FishNet.Connection;

public class NetworkQuestSystem : NetworkSystem
{
    [Server]
    public void AddActiveQuest(int questID, NetworkConnection connection)
    {
        var userManager = Managers.Network.Systems.UserManagement.GetUser(connection);
        Managers.Data.QuestDataLoader.TryGetByID(questID, out var questData);
        userManager.Quest.AddActiveQuest(questData);
    }

    [Server]
    public void AddCompletedQuest(int questID, NetworkConnection connection)
    {
        var userManager = Managers.Network.Systems.UserManagement.GetUser(connection);
        userManager.Quest.AddCompletedQuest(questID);
    }
    
    [Server]
    public void RemoveActiveQuest(int questID, NetworkConnection connection)
    {
        var userManager = Managers.Network.Systems.UserManagement.GetUser(connection);
        userManager.Quest.RemoveActiveQuest(questID);
    }

    [Server]
    public void RemoveCompletedQuest(int questID, NetworkConnection connection)
    {
        var userManager = Managers.Network.Systems.UserManagement.GetUser(connection);
        userManager.Quest.RemoveCompletedQuest(questID);
    }
}