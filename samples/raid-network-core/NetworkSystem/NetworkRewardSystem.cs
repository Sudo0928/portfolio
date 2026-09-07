using FishNet.Object;
using FishNet.Connection;

// 피쉬넷을 사용했으니 Reward를 받아야겠지?
public class NetworkRewardSystem : NetworkSystem
{
    public void GiveReward(int clientId, RewardData rewardData)
    {
        var userManager = Managers.Network.Systems.UserManagement.GetUser(clientId);
        GiveReward(userManager, rewardData);
    }

    [Server]
    public void GiveReward(UserManager userManager, RewardData rewardData)
    {
        GiveReward(userManager.Owner, rewardData);
    }

    [Server]
    public void GiveReward(NetworkConnection conn, RewardData rewardData)
    {
        var userManager = Managers.Network.Systems.UserManagement.GetUser(conn);
        switch(rewardData.type)
        {
            case RewardType.Exp:
                // userManager.UserCharacter.LevelController.GainExp(rewardData.amount);
                break;
            case RewardType.Gold:
                userManager.Currency.AddGold(rewardData.amount);
                break;
            case RewardType.Item:
                userManager.Inventory.TryAdd(rewardData.itemID, rewardData.amount);
                break;
        }
    }
}

public enum RewardType
{
    Exp,
    Gold,
    Item
}