using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using ProjectRaid.Core;
using ProjectRaid.Data;
using System;

public class UserQuickSlot : NetworkBehaviour
{
    private ItemInstance[] quickSlots = new ItemInstance[4];
    public IReadOnlyList<ItemInstance> QuickSlots => quickSlots;
    public event Action<int, ItemInstance> OnChange;

    private UserManager userManager;

    [Server]
    public void Init(UserManager userManager, UserQuickSlotData userQuickSlotData)
    {
        this.userManager = userManager;

        for(int i = 0; i < userQuickSlotData.quickSlots.Count; i++)
        {
            ItemInstance itemInstance = Managers.Network.Systems.UserManagement.GetUser().Inventory.GetItemInstance(ItemType.Consumable, userQuickSlotData.quickSlots[i]);
            quickSlots[i] = itemInstance;
        }
    }

    public void AddQuickSlot(int slotID, ItemInstance itemInstance)
    {
        quickSlots[slotID] = itemInstance;
        OnChange?.Invoke(slotID, itemInstance);
    }

    public void RemoveQuickSlot(int slotID)
    {
        quickSlots[slotID] = null;
        OnChange?.Invoke(slotID, null);
    }

    public void UseQuickSlot(int slotID)
    {
        ItemInstance itemInstance = quickSlots[slotID];
        if (itemInstance == null) { Debug.LogError($"ItemInstance not found: {slotID}"); return; }
        if (!Managers.Network.Systems.UserManagement.GetUser().Inventory.UseItem(itemInstance)) return;
        if (itemInstance.quantity <= 0)
        {
            RemoveQuickSlot(slotID);
        }
        else OnChange?.Invoke(slotID, itemInstance);
    }
    
    public void ClearQuickSlot()
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            RemoveQuickSlot(i);
        }
    }

    // private readonly Dictionary<int, ItemInstance> quickSlots = new();
    // public IReadOnlyDictionary<int, ItemInstance> QuickSlots => quickSlots;

    // public event Action<int, ItemInstance> OnChange;

    // private UserManager userManager;

    // [Server]
    // public void AddOnChangeHandler(Dictionary<int, ItemInstance>.SyncDictionaryChanged onChange)
    // {
    //     quickSlots.OnChange -= onChange;
    //     quickSlots.OnChange += onChange;
    // }

    // [Server]
    // public void Init(UserManager userManager, UserQuickSlotData userQuickSlotData)
    // {
    //     this.userManager = userManager;

    //     foreach(var quickSlot in userQuickSlotData.quickSlots)
    //     {
    //         ItemInstance itemInstance = Managers.Network.Systems.UserManagement.GetUser().Inventory.GetItemInstance(ItemType.Consumable, quickSlot.Value);
    //         quickSlots[quickSlot.Key] = itemInstance;
    //     }
    // }

    // [ServerRpc]
    // public void RequestAddQuickSlot(int slotID, ItemInstance itemInstance)
    // {
    //     ItemData itemData = Managers.Data.ItemDataLoader.GetByID(itemInstance.itemId);
    //     if (itemData == null) { Debug.LogError($"ItemData not found: {itemInstance.itemId}"); return; }
    //     if (itemData.ItemType != ItemType.Consumable) { Debug.LogError($"ItemData is not consumable: {itemInstance.itemId}"); return; }
    //     if (ExistsInQuickSlot(itemData)) return;

    //     quickSlots[slotID] = itemInstance;
    // }

    // // 퀵슬롯에 이미 등록되어있는 아이템인지 확인
    // private bool ExistsInQuickSlot(ItemData data)
    // {
    //     var list = quickSlots.Values;
    //     foreach (var item in list)
    //     {
    //         if (item.itemId == data.ItemID)
    //             return true;
    //     }
    //     return false;
    // }

    // [ServerRpc]
    // public void RequestRemoveQuickSlot(int slotID)
    // {
    //     if(!quickSlots.ContainsKey(slotID)) { Debug.LogError($"QuickSlot not found: {slotID}"); return; }
    //     quickSlots.Remove(slotID);
    // }

    // [ServerRpc]
    // public void RequestClearQuickSlots()
    // {
    //     quickSlots.Clear();
    // }

    // [ServerRpc]
    // public void RequestUseQuickSlotItem(int slotID)
    // {
    //     var itemInstance = quickSlots[slotID];
    //     if(itemInstance == null) { Debug.LogError($"ItemInstance not found: {slotID}"); return; }
    //     if(!userManager.Inventory.UseItem(itemInstance)) return;
    //     itemInstance.quantity--;
    //     if(itemInstance.quantity <= 0)
    //     {
    //         quickSlots.Remove(slotID);
    //     }
    //     OnChange?.Invoke(slotID, itemInstance);
    // }
}
