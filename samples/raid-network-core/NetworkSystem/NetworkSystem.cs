using System;
using UnityEngine;
using FishNet.Object;

public abstract class NetworkSystem : NetworkBehaviour, IDisposable
{
    [field: SerializeField] public bool IsServerOnlySystem { get; private set; } = false;
    public virtual bool Initialize() { return true; }
    public virtual void Dispose() { }
}
