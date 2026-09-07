using System;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using ProjectRaid.Data;

public class NetworkCraftingSystem : NetworkSystem
{
    public event Action<int, int> OnCraftItem;

    [ServerRpc(RequireOwnership = false)]
    public void RequestCraftItem(int recipeID, NetworkConnection conn = null)
    {
        var recipe = Managers.Data.CraftingDataLoader.GetByID(recipeID);

        if(!IsNearCraftingStation(conn, recipe.RequiredStationTag)) return;

        if(!CanCraftItem(recipe, conn)) return;
    }

    [Server]
    private bool CanCraftItem(RecipeData recipe, NetworkConnection conn = null)
    {
        var inventory = Managers.Network.Systems.UserManagement.GetUser(conn).Inventory;
        
        // 1. 재료 충분한지 확인
        foreach(var material in recipe.RequiredMaterials)
        {
            if(!inventory.CanRemove(material.ItemID, material.Quantity)) return false;
        }
        
        // 2. 재료 제거 (실제 삭제)
        foreach(var material in recipe.RequiredMaterials)
        {
            if(!inventory.TryRemove(material.ItemID, material.Quantity))
            {
                // 실패 시 롤백 필요 (하지만 1단계에서 이미 확인했으므로 발생하지 않아야 함)
                Debug.LogError($"Failed to remove material {material.ItemID}. This should not happen!");
                return false;
            }
        }
        
        // 3. 결과물 추가
        if(!inventory.TryAdd(recipe.ResultItem.ID, recipe.ResultCount))
        {
            // 실패 시 재료 복구
            RestoreMaterials(recipe, inventory);
            return false;
        }

        OnCraftItemEvent(conn, recipe.ID);
        
        return true;
    }
    
    [Server]
    private void RestoreMaterials(RecipeData recipe, UserInventory inventory)
    {
        // 제거된 재료들을 다시 복구
        foreach(var material in recipe.RequiredMaterials)
        {
            inventory.TryAdd(material.ItemID, material.Quantity);
        }
    }

    [TargetRpc]
    private void OnCraftItemEvent(NetworkConnection conn, int recipeId)
    {
        var recipe = Managers.Data.CraftingDataLoader.GetByID(recipeId);
        OnCraftItem?.Invoke(recipe.ResultItem.ID, recipe.ResultCount);
    }

    [Server]
    private bool IsNearCraftingStation(NetworkConnection conn, string stationTag)
    {
        // 추후 플레이어 주변에 제작 스테이션이 있는지 검증이 필요 할 때 구현.

        return true;
    }
}
