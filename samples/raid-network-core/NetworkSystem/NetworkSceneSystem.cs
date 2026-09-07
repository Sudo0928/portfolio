using System.Linq;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Connection;
using DarkTonic.MasterAudio;
using Cysharp.Threading.Tasks;
using GameKit.Dependencies.Utilities;
using System;
using System.Threading.Tasks;

public class NetworkSceneSystem : NetworkSystem
{
    private bool isInitialized = false;

    private NetworkManager networkManager;

    public override bool Initialize()
    {
        if (isInitialized) return true;

        networkManager = InstanceFinder.NetworkManager;

        isInitialized = true;

        return true;
    }

    public override void OnStartNetwork()
    {
        InstanceFinder.SceneManager.OnClientPresenceChangeStart += OnClientPresenceChangeStart;
        InstanceFinder.SceneManager.OnLoadStart += OnLoadStart;
        InstanceFinder.SceneManager.OnLoadPercentChange += OnLoadPercentChange;
        InstanceFinder.SceneManager.OnLoadEnd += OnLoadEnd;
    }

    public override void OnStopNetwork()
    {
        if (!isInitialized) return;

        InstanceFinder.SceneManager.OnLoadStart -= OnLoadStart;
        InstanceFinder.SceneManager.OnLoadPercentChange -= OnLoadPercentChange;
        InstanceFinder.SceneManager.OnLoadEnd -= OnLoadEnd;

        isInitialized = false;
    }

    private void OnClientPresenceChangeStart(ClientPresenceChangeEventArgs args)
    {
        UserManager user = Managers.Network.Systems.UserManagement.GetUser(args.Connection);
        if(user == null) return;

        UserData userData = user.ConvertUserData();
        byte[] payload = Managers.Persistence.EncodeData(userData);
        Managers.Network.Systems.UserManagement.SendUserDataToClient(args.Connection, payload);
    }

    private void OnLoadStart(SceneLoadStartEventArgs args)
    {
        if (networkManager.IsClientStarted)
        {
            // StopPlaylist();

            Debug.Log("OnLoadStart");
            Managers.UI.LoadingUI.SetProgress(0, false);
            Managers.UI.LoadingUI.OnShow();
        }
    }

    private void OnLoadPercentChange(SceneLoadPercentEventArgs args)
    {
        if (networkManager.IsServerOnlyStarted) return;

        foreach (var scene in args.QueueData.SceneLoadData.SceneLookupDatas)
        {
            if(scene.Name == "EmptyScene") return;
        }

        float progress = args.Percent - 0.01f;
        Managers.UI.LoadingUI.SetProgress(progress < 0 ? 0 : progress);
    }

    private void OnLoadEnd(SceneLoadEndEventArgs args)
    {
        if(args.QueueData.SceneLoadData.SceneLookupDatas.Any(scene => scene.Name == "EmptyScene")) return;
        InitializeSceneInitializer(args).Forget();
    }

    private async UniTask InitializeSceneInitializer(SceneLoadEndEventArgs args)
    {
        SceneInitializer sceneInitializer = null;

        foreach(var scene in args.QueueData.SceneLoadData.SceneLookupDatas)
        {
            var s = scene.GetScene(out _);
            if(s.IsValid() && s.isLoaded)
            {
                var roots = s.GetRootGameObjects();
                foreach(var root in roots) if(root.TryGetComponent(out SceneInitializer temp)) sceneInitializer = temp;
            }
        }

        // if (!sceneInitializer)
        // {
        //     foreach (var scene in args.QueueData.GlobalScenes)
        //     {
        //         string sceneName = scene.Split('/').Last().Replace(".unity", "");
        //         var sceneObj = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);

        //         if(!sceneObj.IsValid() || !sceneObj.isLoaded) continue;
                
        //         var rootObjects = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName).GetRootGameObjects();
        //         foreach (var rootObject in rootObjects) if (rootObject.TryGetComponent(out SceneInitializer temp)) sceneInitializer = temp;
        //     }
        // }

        if (args.QueueData.AsServer)
        {
            if (sceneInitializer && !sceneInitializer.IsInitialized.Value) { await sceneInitializer.OnSceneInitAsync(args); sceneInitializer.IsInitialized.Value = true; }
        }
        else
        {
            if(!sceneInitializer) 
            {
                var rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                foreach (var rootObject in rootObjects) if (rootObject.TryGetComponent(out TutorialSceneInitializer temp)) sceneInitializer = temp;
            }
            await Managers.UI.LoadingUI.SetProgressAsync(1f).ToUniTask(PlayerLoopTiming.Update);
            await UniTask.WaitForSeconds(0.5f);
            if (sceneInitializer) await sceneInitializer.OnClientInitAsync();
            Managers.UI.LoadingUI.OnHide();
        }
    }

    [Server]
    public void LoadGloablScene(string sceneName, object[] parameters = null)
    {
        LoadEmptyScene();

        LoadGlobalSceneAsync(sceneName, parameters).Forget();
    }

    private async UniTask LoadGlobalSceneAsync(string sceneName, object[] parameters = null)
    {
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == sceneName) await UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

        SceneLoadData data = new SceneLoadData(sceneName);
        data.Options.AutomaticallyUnload = true;
        data.ReplaceScenes = ReplaceOption.All;
        if(parameters != null) data.Params.ServerParams = parameters;

        networkManager.SceneManager.LoadGlobalScenes(data);
    }

    [Server]
    public void LoadGloablScene(int sceneIndex)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(sceneIndex).name;
        LoadGloablScene(sceneName);
    }


    [Server]
    public void LoadConnectionScene(string sceneName, NetworkConnection connection)
    {
        LoadEmptyScene();

        SceneLoadData data = new SceneLoadData(sceneName);
        data.Options.AutomaticallyUnload = true;
        data.ReplaceScenes = ReplaceOption.All;

        networkManager.SceneManager.LoadConnectionScenes(connection, data);
    }

    [Server]
    public void LoadConnectionScene(int sceneIndex, NetworkConnection connection)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(sceneIndex).name;
        LoadConnectionScene(sceneName, connection);
    }

    [Server]
    public void LoadPartyScene(string sceneName, UserParty party)
    {
        SceneLoadData data = new SceneLoadData(sceneName);

        data.ReplaceScenes = ReplaceOption.All;
        data.Options.AutomaticallyUnload = true;
        data.Params.ServerParams = new object[] { party };

        foreach (var member in party.PartyMembers)
        {
            if(!InstanceFinder.ServerManager.Clients.TryGetValueIL2CPP(member, out var connection)) continue;
            networkManager.SceneManager.LoadConnectionScenes(connection, data);
        }
    }

    [Server]
    public void LoadPartyScene(int sceneIndex, UserParty party)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(sceneIndex).name;
        LoadPartyScene(sceneName, party);
    }

    private void StopPlaylist()
    {
        PlaylistController playlistController = MasterAudio.OnlyPlaylistController;
        float volume = playlistController.PlaylistVolume;
        playlistController.FadeToVolume(0f, 2f, () => { MasterAudio.StopPlaylist(); playlistController.PlaylistVolume = volume; });
    }

    [Server]
    internal void LoadTrainingScene(string sceneName, BossSO bossData)
    {
        LoadEmptyScene();

        SceneLoadData data = new SceneLoadData(sceneName);

        data.ReplaceScenes = ReplaceOption.All;
        data.Options.AutomaticallyUnload = true;
        data.Params.ServerParams = new object[] { bossData };

        networkManager.SceneManager.LoadGlobalScenes(data);
    }

    [Server]
    private void LoadEmptyScene()
    {
        var toEmpty = new SceneLoadData("EmptyScene");
        toEmpty.ReplaceScenes = ReplaceOption.All;
        toEmpty.Options.AutomaticallyUnload = true;
        InstanceFinder.SceneManager.LoadGlobalScenes(toEmpty);
    }
}
