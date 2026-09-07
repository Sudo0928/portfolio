using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Object;
using FishNet.Managing;
using FishNet.Connection;
using FishNet.Transporting;
using FishNet.Transporting.Yak;
using FishNet.Transporting.Multipass;
using GameKit.Dependencies.Utilities.Types;

using Object = UnityEngine.Object;
using Channel = FishNet.Transporting.Channel;
using FishNet.Transporting.Tugboat;
using DarkTonic.MasterAudio;
using TMPro;

namespace ProjectRaid.Network
{
    [Serializable]
    public class NetworkManagerEx : IInitializable, IDisposable
    {
        private const string LogPrefix = "[NetworkManagerEx] ";
        [SerializeField] private NetworkSystemManager networkSystemManagerPrefab;

        private SteamManagerEx steamManager;
        public SteamManagerEx Steam => steamManager;

        private NetworkSystemManager networkSystemManager;
        public NetworkSystemManager Systems => networkSystemManager ??= Object.FindAnyObjectByType<NetworkSystemManager>();

        [field: SerializeField] public int MaxConnectionCount { get; private set; } = 4;

        private NetworkMode networkMode = NetworkMode.Host;

        [Tooltip("연결이 끊겼을 때 로드할 씬. 서버와 클라이언트 모두 이 씬을 로드합니다.")]
        [SerializeField]
        [Scene]
        private string _offlineScene;

        [Tooltip("캐릭터 선택 씬.")]
        [SerializeField]
        [Scene]
        private string _characterSelectionScene;

        [Tooltip("튜토리얼 씬. 서버와 클라이언트 모두 이 씬을 로드합니다.")]
        [SerializeField]
        [Scene]
        private string _tutorialScene;

        [Tooltip("연결 시 로드할 씬. 서버와 클라이언트 모두 이 씬을 로드합니다.")]
        [SerializeField]
        [Scene]
        private string _onlineScene;

        [Tooltip("디버그 모드일 때 로드할 씬. 서버와 클라이언트 모두 이 씬을 로드합니다.")]
        [SerializeField]
        [Scene]
        private string _debugScene;

        [SerializeField] private float timeoutSec = 10f;
        public float TimeoutSec => timeoutSec;
        private string _token;
        private readonly HashSet<int> _responded = new HashSet<int>();
        private UniTaskCompletionSource<bool> _clientDataTcs;

        public NetworkManager NetworkManager { get; private set; } = null;
        public Multipass Multipass { get; private set; } = null;

        public bool IsInitialized { get; private set; } = false;

        public UniTask<bool> Initialize()
        {
            NetworkManager = InstanceFinder.NetworkManager;
            if (NetworkManager == null)
            {
                Debug.LogError(LogPrefix + "NetworkManager 을 찾을 수 없습니다.");
                return UniTask.FromResult(false);
            }

            Multipass = NetworkManager.GetComponent<Multipass>();
            if (Multipass == null)
            {
                Debug.LogError(LogPrefix + "Multipass 을 찾을 수 없습니다.");
                return UniTask.FromResult(false);
            }

            NetworkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
            NetworkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;

            return UniTask.FromResult(true);
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case LocalConnectionState.Stopped:
                    if(Managers.Instance == null || Managers.Instance.DebugMode) return;
                    SceneManager.LoadScene(_offlineScene);
                    break;
            }
        }

        public void Update()
        {
            if (Managers.Instance.DebugMode) return;
            // Steam.Update();
        }

        public void Dispose()
        {
            NetworkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            NetworkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;

            if (networkSystemManager != null)
            {
                networkSystemManager.Dispose();
            }

            // Steam.Dispose();
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            OnServerConnectionStateAsync(args).Forget();
        }

        private async UniTaskVoid OnServerConnectionStateAsync(ServerConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case LocalConnectionState.Started:
                    if (IsInitialized) return;

                    if (networkSystemManagerPrefab == null) { Debug.LogError(LogPrefix + "networkSystemManagerPrefab 이(가) 설정되지 않았습니다."); return; }

                    var networkSystem = Object.Instantiate(networkSystemManagerPrefab);
                    networkSystem.NetworkObject.SetIsGlobal(true);
                    NetworkManager.ServerManager.Spawn(networkSystem.NetworkObject);
                    await networkSystem.Initialize();
                    networkSystemManager = networkSystem;

                    if (Managers.Instance.DebugMode) networkSystemManager.Scene.LoadGloablScene(SceneManager.GetActiveScene().name);
                    else if(Managers.UserData.GetCurrentUserData().hasCompletedTutorial) networkSystemManager.Scene.LoadGloablScene(_onlineScene);
                    else networkSystemManager.Scene.LoadGloablScene(_characterSelectionScene);

                    if (networkMode == NetworkMode.Host)
                    {
                        RegisterClientBroadcasts();

                        Multipass.SetClientTransport<Yak>();
                        Multipass.SetClientAddress("localhost");

                        Multipass.StartConnection(false);
                    }

                    IsInitialized = true;
                    break;
                case LocalConnectionState.Stopped:
                    if (!IsInitialized) return;

                    if (networkSystemManager != null)
                    {
                        networkSystemManager.Dispose();
                        NetworkManager.ServerManager.Despawn(networkSystemManager.NetworkObject);
                    }
                    
                    IsInitialized = false;
                    break;
            }
        }

        public UniTask StartServerAsync()
        {
            NetworkManager.ServerManager.RegisterBroadcast<RequestClientData>(OnRequestClientData);
            Multipass.StartConnection(true);
            return UniTask.CompletedTask;
        }

        public async UniTask<bool> StartHostAsync()
        {
            networkMode = NetworkMode.Host;
            await StartServerAsync();
            return true;
        }

        public UniTask StartClientAsync(string address = "localhost")
        {
            networkMode = NetworkMode.Client;
            RegisterClientBroadcasts();

            if (Managers.Instance.DebugMode) Multipass.SetClientTransport<Yak>();
            else Multipass.SetClientTransport<Yak>();
            Multipass.SetClientAddress(address);

            Multipass.StartConnection(false);
            return UniTask.CompletedTask;
        }

        public async UniTask<bool> StopServerAsync()
        {
            bool ok = await BeginShutdown();
            Multipass.StopConnection(true);
            NetworkManager.ServerManager.UnregisterBroadcast<RequestClientData>(OnRequestClientData);
            ResetShutdownState();
            return ok;
        }

        public async UniTask<bool> StopClientAsync()
        {
            if (!NetworkManager.IsClientStarted) return true;

            _clientDataTcs = new UniTaskCompletionSource<bool>();

            var req = new RequestClientData { Token = null };
            NetworkManager.ClientManager.Broadcast(req, Channel.Reliable);

            bool ok = false;
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSec));
            try
            {
                await _clientDataTcs.Task.AttachExternalCancellation(timeoutCts.Token);
                ok = true;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning($"[NetworkManagerEx] 클라이언트 중단 취소됨({timeoutSec}s).");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkManagerEx] 클라이언트 중단 중 오류: {ex}");
                return false;
            }
            finally
            {
                UnregisterClientBroadcasts();
                Multipass.StopConnection(false);
                _clientDataTcs = null;
                _token = null;
            }

            return ok;
        }

        public void OpenServer(int maxMembers = 4)
        {
            // Steam.CreateLobby(maxMembers);
        }

        private async UniTask<bool> BeginShutdown(float timeout = 10f)
        {
            _responded.Clear();
            _token = Guid.NewGuid().ToString("N");

            var targets = new HashSet<int>(NetworkManager.ServerManager.Clients.Keys);

            var notice = new ShutdownNotice { Token = _token };
            NetworkManager.ServerManager.Broadcast(notice, true, Channel.Reliable);

            float end = Time.realtimeSinceStartup + timeout;

            while (Time.realtimeSinceStartup < end)
            {
                targets.IntersectWith(NetworkManager.ServerManager.Clients.Keys);

                if (_responded.IsSupersetOf(targets))
                    return true;

                await UniTask.Delay(200);
            }

            return false;
        }

        private void RegisterClientBroadcasts()
        {
            NetworkManager.ClientManager.RegisterBroadcast<ShutdownNotice>(OnClientReceiveNotice);
            NetworkManager.ClientManager.RegisterBroadcast<DeliveryClientData>(OnClientReceiveClientData);
        }

        private void UnregisterClientBroadcasts()
        {
            NetworkManager.ClientManager.UnregisterBroadcast<ShutdownNotice>(OnClientReceiveNotice);
            NetworkManager.ClientManager.UnregisterBroadcast<DeliveryClientData>(OnClientReceiveClientData);
        }

        private void ResetShutdownState()
        {
            _token = null;
            _responded.Clear();
        }

        [Client]
        private void OnClientReceiveNotice(ShutdownNotice msg, Channel channel)
        {
            if (!NetworkManager.IsClientStarted) return;

            _token = msg.Token;
            var req = new RequestClientData { Token = msg.Token };
            NetworkManager.ClientManager.Broadcast(req, Channel.Reliable);
        }

        [Client]
        private void OnClientReceiveClientData(DeliveryClientData msg, Channel channel)
        {
            if (msg.Payload == null) return;
            if (!string.IsNullOrEmpty(_token) && msg.Token != _token) return;

            UserData data = Managers.Persistence.DecodeData<UserData>(msg.Payload);
            Managers.UserData.UpdatedData(data);
            Managers.UserData.SaveAllData();

            _clientDataTcs?.TrySetResult(true);
        }

        [Server]
        private void OnRequestClientData(NetworkConnection connection, RequestClientData request, Channel channel)
        {
            if (!string.IsNullOrEmpty(_token) && request.Token != _token) return;

            UserManager userManager = networkSystemManager.UserManagement.GetUser(connection);
            if (userManager == null)
            {
                Debug.LogError("[NetworkManagerEx] UserManager 을 찾을 수 없습니다.");
                return;
            }

            UserData userData = userManager.ConvertUserData();
            byte[] payload = Managers.Persistence.EncodeData(userData);

            var deliver = new DeliveryClientData { Token = _token, Payload = payload };
            NetworkManager.ServerManager.Broadcast(connection, deliver, true, channel);
            _responded.Add(connection.ClientId);
        }

        [Server]
        public void KickClient(NetworkConnection connection)
        {
            NetworkManager.ServerManager.Kick(connection, FishNet.Managing.Server.KickReason.Unset);
        }
    }

    public enum NetworkMode
    {
        Server,
        Client,
        Host
    }
}