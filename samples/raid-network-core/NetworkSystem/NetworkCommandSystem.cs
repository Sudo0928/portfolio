using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;

public class NetworkCommandSystem : NetworkSystem
{
    [Server][ContextMenu("LoadGloablScene")]
    public void LoadGloablScene()
    {
        NetworkSystemManager.Instance.Scene.LoadGloablScene("02_Academy");
    }
}