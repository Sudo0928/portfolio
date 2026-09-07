using System;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.Yak;
using Steamworks;

[Serializable]
public class NetworkManagerEx
{
    private NetworkManager networkManager;
    private Multipass multipass;

    public void Init()
    {
        // SteamAPI.Init();
        
        // networkManager = InstanceFinder.NetworkManager;
        // multipass = InstanceFinder.TransportManager.GetTransport<Multipass>();

        // multipass.SetClientTransport<Yak>();
    }

    public void ChangeTransport<T>() where T : Transport
    {
        multipass.SetClientTransport<T>();
    }
}
