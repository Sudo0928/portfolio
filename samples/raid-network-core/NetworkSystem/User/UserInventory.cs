using System;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using ProjectRaid.Core;
using ProjectRaid.Data;
using DG.Tweening;
using ProjectRaid.Combat;

public class UserInventory : NetworkBehaviour
{
    private const int COLUMNS = 10;
    private const int ROWS = 5;
    public const int SLOTS_PER_CATEGORY = COLUMNS * ROWS;

    // 카테고리별 슬롯 배열
    private readonly SyncDictionary<ItemType, List<ItemInstance>> items = new();
    public IReadOnlyDictionary<ItemType, List<ItemInstance>> Items => items;

    private UserManager userManager;
    public UserManager UserManager => userManager;

    [Server]
    public void Init(UserManager userManager, UserInventoryData userInventoryData)
    {
        this.userManager = userManager;

        foreach (ItemType cat in Enum.GetValues(typeof(ItemType)))
        {
            items[cat] = new List<ItemInstance>();
        }

        foreach (var item in userInventoryData.items)
        {
            items[item.Key] = item.Value;
        }
    }

    #region Client-API (조회)
    /// <summary>
    /// 특정 카테고리 인벤토리가 가득 찼는지 확인
    /// </summary>
    public bool IsCategoryFull(ItemType cat)
    {
        return items[cat].Count >= SLOTS_PER_CATEGORY;
    }

    /// <summary>아이템 ID로 현재 총 보유 개수 반환</summary>
    public int GetItemCount(int itemId)
    {
        Managers.Data.ItemDataLoader.TryGetByID(itemId, out var itemData);

        if (itemData == null)
        {
            Debug.LogError($"ItemData not found: {itemId}");
            return 0;
        }

        List<ItemInstance> itemInstances = items[itemData.ItemType];

        int count = 0;
        foreach (var item in itemInstances)
        {
            if (item.itemId == itemId) count += item.quantity;
        }

        return count;
    }

    /// <summary>
    /// 해당 아이템을 추가할 여유 공간이 충분한지 확인
    /// </summary>
    public bool CanAdd(ItemData data, int amount)
    {
        ItemType type = data.ItemType;

        List<ItemInstance> itemInstances = items[type];
        int maxStack = data.MaxStack;

        int space = 0;

        if (itemInstances.Count >= SLOTS_PER_CATEGORY) return false;

        if (amount <= 0)
        {
            Debug.LogError("Amount is less than 0");
            return false;
        }

        for (int i = 0; i < itemInstances.Count; ++i)
        {
            if (itemInstances[i].itemId == data.ID && itemInstances[i].quantity < maxStack)
            {
                space += maxStack - itemInstances[i].quantity;
            }
        }

        return space >= amount || ((SLOTS_PER_CATEGORY - itemInstances.Count) * (data.Stackable ? maxStack : 1)) + space >= amount;
    }

    /// <summary>
    /// 해당 아이템을 소비할 개수가 충분한지 확인
    /// </summary>
    public bool CanRemove(int itemID, int amount)
    {
        int available = GetItemCount(itemID);
        return available >= amount;
    }
    #endregion

    #region Server-API (조작)
    /// <summary>
    /// 인벤토리에 아이템 추가 (ID) / 
    /// 성공 여부 반환(공간 부족 시 false)
    /// </summary>
    [Server]
    public bool TryAdd(int id, int amount = 1)
    {
        if (!Managers.Data.ItemDataLoader.TryGetByID(id, out var data)) return false;
        return TryAdd(data, amount);
    }

    /// <summary>
    /// 인벤토리에 아이템 추가 (ItemData) / 
    /// 성공 여부 반환(공간 부족 시 false)
    /// </summary>
    [Server]
    public bool TryAdd(ItemData data, int amount)
    {
        if (!CanAdd(data, amount)) return false;

        ItemType type = data.ItemType;
        List<ItemInstance> itemInstances = items[type];
        int maxStack = data.MaxStack;

        if (itemInstances.Count >= SLOTS_PER_CATEGORY) return false;

        if (amount <= 0)
        {
            Debug.LogError("Amount is less than 0");
            return false;
        }

        int _amount = amount;

        for (int i = 0; i < itemInstances.Count; ++i)
        {
            ItemInstance itemInstance = itemInstances[i];

            if (itemInstance.itemId == data.ID && itemInstance.quantity < maxStack)
            {
                int space = maxStack - itemInstance.quantity;
                int add = Mathf.Min(space, _amount);
                itemInstance.quantity += add;
                _amount -= add;
            }

            if (_amount == 0)
            {
                items.Dirty(type);
                return true;
            }
        }

        while (_amount > 0)
        {
            int add = Mathf.Min(data.Stackable ? data.MaxStack : 1, _amount);
            itemInstances.Add(new ItemInstance(data.ID, add));
            _amount -= add;
        }

        items.Dirty(type);
        Managers.Event.DispatchEvent(new ChangeInventoryEvent(this, data.ID));
        return true;
    }
    [Server]
    public void TryRemoveAll(int itemID)
    {
        ItemData itemData = Managers.Data.ItemDataLoader.GetByID(itemID);

        if (itemData == null)
        {
            Debug.LogError($"ItemData not found: {itemID}");
            return;
        }

        List<ItemInstance> itemInstances = items[itemData.ItemType];

        for (int i = 0; i < itemInstances.Count; i++)
        {
            ItemInstance item = itemInstances[i];
            if (item.itemId != itemID) continue;

            items[itemData.ItemType].RemoveAt(i);
            return;
        }
        
        items.Dirty(itemData.ItemType);
    }

    [Server]
    public bool TryRemove(ItemData data, int amount = 1)
    {
        int available = GetItemCount(data.ID);
        // if (available < amount) return false;

        if (data == null)
        {
            Debug.LogError($"ItemData not found: {data}");
            return false;
        }

        List<ItemInstance> itemInstances = items[data.ItemType];
        List<int> removeIndexes = new List<int>();

        for (int i = 0; amount > 0; i++)
        {
            if (itemInstances[i].itemId != data.ID) continue;

            if (itemInstances[i].quantity <= amount)
            {
                amount -= itemInstances[i].quantity;
                itemInstances[i].quantity = 0;
                removeIndexes.Add(i);
            }
            else
            {
                itemInstances[i].quantity -= amount;
                amount = 0;
            }
        }

        foreach (var index in removeIndexes)
        {
            items[data.ItemType].RemoveAt(index);
        }

        items.Dirty(data.ItemType);
        Managers.Event.DispatchEvent(new ChangeInventoryEvent(this, data.ID));
        return true;
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템을 주어진 수량만큼 제거. 부족 시 false
    /// </summary>
    [Server]
    public bool TryRemove(int itemId, int amount = 1)
    {
        ItemData itemData = Managers.Data.ItemDataLoader.GetByID(itemId);
        if (itemData == null)
        {
            Debug.LogError($"ItemData not found: {itemId}");
            return false;
        }

        return TryRemove(itemData, amount);
    }

    /// <summary>
    /// 슬롯 내용을 직접 비우기(버리기/창고보관 용도)
    /// </summary>
    [Server]
    public void ClearSlot(ItemType type, int index)
    {
        if (index < 0 || index >= SLOTS_PER_CATEGORY) return;
        items[type].RemoveAt(index);
        items.Dirty(type);
    }

    [Server]
    public bool UseItem(ItemInstance itemInstance, int amount = 1)
    {
        ItemData itemData = Managers.Data.ItemDataLoader.GetByID(itemInstance.itemId);
        if (!HasConsumableItem(itemData)) return false;

        ConsumableData consumableData = itemData.Additive as ConsumableData;
        if(!TryRemove(itemData, amount)) return false;

        var baseStats = consumableData.BaseStats;
        // var aggregator = userManager.UserCharacter.StatAggregator;
        StatModifierType statModifierType = baseStats.IsPercent ? StatModifierType.Percent : StatModifierType.Flat;

        userManager.UserCharacter.ModifyStat(StatType.Heal, statModifierType, baseStats.HP, baseStats.Duration);
        userManager.UserCharacter.ModifyStat(StatType.MaxStamina, statModifierType, baseStats.Stamina, baseStats.Duration);
        userManager.UserCharacter.ModifyStat(StatType.RegenStamina, statModifierType, baseStats.Stamina, baseStats.Duration);
        userManager.UserCharacter.ModifyStat(StatType.Strength, statModifierType, baseStats.Attack, baseStats.Duration);
        userManager.UserCharacter.ModifyStat(StatType.Defense, statModifierType, baseStats.Defense, baseStats.Duration);
        userManager.UserCharacter.ModifyStat(StatType.CriticalRate, statModifierType, baseStats.CriticalChance, baseStats.Duration);
        userManager.UserCharacter.ModifyStat(StatType.CriticalDamage, statModifierType, baseStats.CriticalDamage, baseStats.Duration);
        
        return true;
    }

    // 리소스(HP/스태미나) 적용 헬퍼
    private void ApplyResourceIfPositive(float value, StatType refStatType, StatModifierType statModifierType, Action<float> apply)
    {
        // if (value <= 0f) return;
        // float amountToApply = (statModifierType == StatModifierType.Percent)
        //     ? userManager.UserCharacter.StatAggregator.GetStat(refStatType) / value
        //     : value;
        // apply(amountToApply);
    }

    public bool HasConsumableItem(ItemInstance itemInstance)
    {
        ItemData itemData = Managers.Data.ItemDataLoader.GetByID(itemInstance.itemId);
        return HasConsumableItem(itemData);
    }

    public bool HasConsumableItem(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogError($"ItemData is null");
            return false;
        }

        if (itemData.ItemType != ItemType.Consumable)
        {
            Debug.LogError($"ItemData is not consumable: {itemData.ID}");
            return false;
        }

        if (itemData.Additive is ConsumableData) return true;

        return false;
    }
    #endregion

    #region HELPER
    /// <summary>
    /// 갈무리 전 체크: 소비·기타 카테고리 각각 1칸 이상 빈칸이 있는지 반환
    /// </summary>
    public bool HasHarvestSpace() => !IsCategoryFull(ItemType.Consumable) && !IsCategoryFull(ItemType.Material);

    /// <summary>
    /// 특정 인덱스의 아이템 인스턴스 정보 반환
    /// </summary>
    public ItemInstance GetItemInstanceToIndex(ItemType type, int index) => items[type][index];

    public ItemInstance GetItemInstance(int itemId)
    {
        foreach (var item in items)
        {
            foreach (var itemInstance in item.Value)
            {
                if (itemInstance.itemId == itemId) return itemInstance;
            }
        }
        return null;
    }

    public ItemInstance GetItemInstance(ItemType type, int itemId)
    {
        foreach (var itemInstance in items[type])
        {
            if (itemInstance.itemId == itemId) return itemInstance;
        }
        return null;
    }
    #endregion
}
