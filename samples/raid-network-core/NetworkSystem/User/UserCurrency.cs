using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class UserCurrency : NetworkBehaviour
{
    private readonly SyncVar<int> gold = new(0);
    public int Gold => gold.Value;

    private UserManager userManager;

    public void Init(UserManager userManager, UserCurrencyData userCurrencyData)
    {
        this.userManager = userManager;
        gold.Value       = userCurrencyData.gold;
    }

    public bool AddGold(int amount)
    {
        if(amount < 0)
        {
            Debug.LogError("Gold 값이 음수입니다.");
            return false;
        }

        gold.Value += amount;
        return true;
    }

    public bool SubGold(int amount)
    {
        if(amount < 0)
        {
            Debug.LogError("Gold 값이 음수입니다.");
            return false;
        }
        
        gold.Value -= amount;
        return true;
    }

    public bool SetGold(int amount)
    {
        if(amount < 0)
        {
            Debug.LogError("Gold 값이 음수입니다.");
            return false;
        }
        
        gold.Value = amount;
        return true;
    }
}
