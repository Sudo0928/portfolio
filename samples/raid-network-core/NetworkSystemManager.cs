using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NetworkSystemManager : NetworkSingleton<NetworkSystemManager>, IInitializable, IDisposable
{
    [SerializeField] private List<NetworkSystem> networkSystemPrefabs = new();
    private readonly Dictionary<Type, NetworkSystem> networkSystems   = new();

    #region Network Systems
    public NetworkSceneSystem          Scene          => GetNetworkSystem<NetworkSceneSystem>();
    public NetworkUserManagementSystem UserManagement => GetNetworkSystem<NetworkUserManagementSystem>();
    public NetworkCraftingSystem       Crafting       => GetNetworkSystem<NetworkCraftingSystem>();
    public NetworkRewardSystem         Reward         => GetNetworkSystem<NetworkRewardSystem>();
    public NetworkQuestSystem          Quest          => GetNetworkSystem<NetworkQuestSystem>();
    public NetworkVfxSystem            Vfx            => GetNetworkSystem<NetworkVfxSystem>();
    // public MinimapSystem            Minimap        => GetNetworkSystem<MinimapSystem>();
    #endregion

    public readonly SyncVar<bool> SystemSpawned = new(false);

    [Server]
    public UniTask<bool> Initialize()
    {
        if (SystemSpawned.Value) return UniTask.FromResult(true);

        foreach (NetworkSystem prefab in networkSystemPrefabs)
        {
            NetworkSystem networkSystem = Instantiate(prefab);
            networkSystem.NetworkObject.SetIsGlobal(true);
            InstanceFinder.ServerManager.Spawn(networkSystem.NetworkObject);
            networkSystem.Initialize();
            networkSystems.Add(networkSystem.GetType(), networkSystem);
        }
        SystemSpawned.Value = true;

        return UniTask.FromResult(true);
    }

    [Server]
    public void Dispose()
    {
        if (!InstanceFinder.IsServerStarted) return;

        if (!SystemSpawned.Value) return;

        foreach (NetworkSystem networkSystem in networkSystems.Values)
        {
            networkSystem.Dispose();
        }
        networkSystems.Clear();
        SystemSpawned.Value = false;
    }

    public T GetNetworkSystem<T>() where T : NetworkSystem
    {
        if (networkSystems.TryGetValue(typeof(T), out NetworkSystem networkSystem))
        {
            return (T)networkSystem;
        }
        else
        {
            networkSystem = FindAnyObjectByType<T>();
            networkSystems.Add(typeof(T), networkSystem);
            return (T)networkSystem;
        }
    }
}
