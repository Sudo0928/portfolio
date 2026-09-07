using FishNet.Object;
using FishNet.Object.Synchronizing;
using ProjectRaid.Core;
using ProjectRaid.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserEquipment : NetworkBehaviour
{

    private UserManager userManager;

    private ItemInstance weapon;
    public ItemInstance Weapon => weapon;

    
    public void Init(UserManager userManager, ItemInstance weapon)
    {
        this.userManager = userManager;
        this.weapon = weapon;
        if(weapon != null)
        {
            EquipWeapon(weapon);
        }
    }

    public void ChangeWeapon(ItemInstance itemInstance)
    {
        if(itemInstance == null) return;

        UnequipWeapon();
        EquipWeapon(itemInstance);
    }

    public void UnequipWeapon()
    {
        if(weapon == null) return;
        if(!TryGetWeaponData(weapon.itemId, out WeaponData weaponData)) return;
        userManager.UserCharacter.ModifyStat(StatType.MaxHealth, StatModifierType.Flat, -weaponData.BaseStats.HP, 0f);
        userManager.UserCharacter.ModifyStat(StatType.Heal, StatModifierType.Flat, -weaponData.BaseStats.HP, 0f);
        userManager.UserCharacter.ModifyStat(StatType.MaxStamina, StatModifierType.Flat, -weaponData.BaseStats.Stamina, 0f);
        userManager.UserCharacter.ModifyStat(StatType.RegenStamina, StatModifierType.Flat, -weaponData.BaseStats.Stamina, 0f);
        userManager.UserCharacter.ModifyStat(StatType.Strength, StatModifierType.Flat, -weaponData.BaseStats.Attack, 0f);
        userManager.UserCharacter.ModifyStat(StatType.Defense, StatModifierType.Flat, -weaponData.BaseStats.Defense, 0f);
        userManager.UserCharacter.ModifyStat(StatType.CriticalRate, StatModifierType.Flat, -weaponData.BaseStats.CriticalChance, 0f);
        userManager.UserCharacter.ModifyStat(StatType.CriticalDamage, StatModifierType.Flat, -weaponData.BaseStats.CriticalDamage, 0f);
        userManager.UserCharacter.ModifyStat(StatType.SeverPower, StatModifierType.Flat, -weaponData.CutValue, 0f);
        userManager.UserCharacter.ModifyStat(StatType.BreakPower, StatModifierType.Flat, -weaponData.DestructionValue, 0f);
        weapon.isEquipped = false;
        weapon = null;
    }

    public void EquipWeapon(ItemInstance itemInstance)
    {
        if(itemInstance == null) return;
        if(!TryGetWeaponData(itemInstance.itemId, out WeaponData weaponData)) return;
        userManager.UserCharacter.ModifyStat(StatType.MaxHealth, StatModifierType.Flat, weaponData.BaseStats.HP, 0f);
        userManager.UserCharacter.ModifyStat(StatType.Heal, StatModifierType.Flat, weaponData.BaseStats.HP, 0f);
        userManager.UserCharacter.ModifyStat(StatType.MaxStamina, StatModifierType.Flat, weaponData.BaseStats.Stamina, 0f);
        userManager.UserCharacter.ModifyStat(StatType.RegenStamina, StatModifierType.Flat, weaponData.BaseStats.Stamina, 0f);
        userManager.UserCharacter.ModifyStat(StatType.Strength, StatModifierType.Flat, weaponData.BaseStats.Attack, 0f);
        userManager.UserCharacter.ModifyStat(StatType.Defense, StatModifierType.Flat, weaponData.BaseStats.Defense, 0f);
        userManager.UserCharacter.ModifyStat(StatType.CriticalRate, StatModifierType.Flat, weaponData.BaseStats.CriticalChance, 0f);
        userManager.UserCharacter.ModifyStat(StatType.CriticalDamage, StatModifierType.Flat, weaponData.BaseStats.CriticalDamage, 0f);
        userManager.UserCharacter.ModifyStat(StatType.SeverPower, StatModifierType.Flat, weaponData.CutValue, 0f);
        userManager.UserCharacter.ModifyStat(StatType.BreakPower, StatModifierType.Flat, weaponData.DestructionValue, 0f);
        weapon = itemInstance;
        weapon.isEquipped = true;
    }

    private bool TryGetWeaponData(int itemID, out WeaponData weaponData)
    {
        weaponData = null;
        if(!Managers.Data.ItemDataLoader.TryGetByID(itemID, out ItemData itemData)) return false;
        if(itemData.ItemType != ItemType.Equipment) return false;
        weaponData = itemData.Additive as WeaponData;
        if(weaponData == null) return false;
        return true;
    }
}
