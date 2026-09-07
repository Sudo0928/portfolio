using System;
using System.Linq;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
using FishNet.Transporting;
using Cysharp.Threading.Tasks;
using ProjectRaid.Extensions;

public class NetworkUserManagementSystem : NetworkSystem
{
    [SerializeField] private UserManager userManagerPrefab;

    public readonly SyncDictionary<int, UserManager> userManagers = new();

    public override bool Initialize()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

        return true;
    }

    public override void Dispose()
    {
        InstanceFinder.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        userManagers.Clear();
    }

    private void OnRemoteConnectionState(NetworkConnection connection, RemoteConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case RemoteConnectionState.Stopped:
                RemoveUserManager(connection.ClientId);
                break;
        }
    }

    public override void OnStartClient()
    {
        UserData userData;

        if (Managers.Instance.DebugMode)
        {
            userData = new UserData();
        }
        else
        {
            int index = Managers.UserData.currentUserDataIndex;
            userData = Managers.UserData.UserData[index];
        }

        byte[] payload = Managers.Persistence.EncodeData(userData);
        SendUserData(InstanceFinder.ClientManager.Connection, payload);
    }

    [TargetRpc]
    private void RequestUserData(NetworkConnection connection)
    {
        int index = Managers.UserData.currentUserDataIndex;
        UserData userData = Managers.UserData.UserData[index];
        byte[] payload = Managers.Persistence.EncodeData(userData);
        SendUserData(connection, payload);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendUserData(NetworkConnection connection, byte[] payload)
    {
        UserData userData = Managers.Persistence.DecodeData<UserData>(payload);
        CreateUserManager(connection, userData);
    }

    [TargetRpc]
    public void SendUserDataToClient(NetworkConnection connection, byte[] payload)
    {
        UserData userData = Managers.Persistence.DecodeData<UserData>(payload);
        Managers.UserData.SetUserData(userData);
    }

    [Server]
    [Obsolete]
    private void RemoveUserData(int clientID)
    {
        RemoveUserManager(clientID);
    }

    public UserManager GetUser(NetworkConnection connection)
    {
        return GetUser(connection.ClientId);
    }

    public UserManager GetUser(int clientID)
    {
        if (userManagers.TryGetValue(clientID, out UserManager userManager))
        {
            return userManager;
        }
        return null;
    }

    public UserManager GetUser()
    {
        return GetUser(InstanceFinder.ClientManager.Connection.ClientId);
    }

    public async UniTask<UserManager> GetUserAsync(NetworkConnection connection)
    {
        return await GetUserAsync(connection.ClientId);
    }

    public async UniTask<UserManager> GetUserAsync(int clientID)
    {
        var tcs = new UniTaskCompletionSource<UserManager>();

        if (userManagers.TryGetValue(clientID, out UserManager userManager))
        {
            tcs.TrySetResult(userManager);
        }
        else
        {
            userManagers.OnChange += OnUserManagersChange;
            _ = tcs.TimeoutAfter(TimeSpan.FromSeconds(Managers.Network.TimeoutSec), "User management timeout");
        }

        void OnUserManagersChange(SyncDictionaryOperation op, int key, UserManager value, bool asServer)
        {
            if (key == clientID)
            {
                userManagers.OnChange -= OnUserManagersChange;
                tcs.TrySetResult(value);
            }
        }

        try { return await tcs.Task; }
        finally { userManagers.OnChange -= OnUserManagersChange; }
    }

    public async UniTask<UserManager> GetUserAsync()
    {
        var clientID = InstanceFinder.ClientManager.Connection.ClientId;
        return await GetUserAsync(clientID);
    }

    [Server]
    public UserManager[] GetAllUser()
    {
        return userManagers.Values.ToArray();
    }

    [Server]
    public void CreateUserManager(NetworkConnection connection, UserData userData)
    {
        UserManager userManager = Instantiate(userManagerPrefab);
        userManager.NetworkObject.SetIsGlobal(true);
        InstanceFinder.ServerManager.Spawn(userManager.NetworkObject, connection);
        userManager.Init(userData);

        userManagers.Add(connection.ClientId, userManager);
    }

    [Server]
    public void RemoveUserManager(int clientID)
    {
        if (userManagers.TryGetValue(clientID, out UserManager userManager))
        {
            userManagers.Remove(clientID);
            
            if (userManager != null && userManager.NetworkObject != null && userManager.NetworkObject.IsSpawned)
            {
                InstanceFinder.ServerManager.Despawn(userManager.NetworkObject);
            }
        }
    }
}