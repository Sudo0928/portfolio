using UnityEngine;
using FishNet.Object;
using ProjectRaid.Data;

public class NetworkVfxSystem : NetworkSystem
{
    [ObserversRpc]
    public void PlayVfx(int particleID)
    {
        VfxData vfxData = Managers.Data.VfxDataLoader.GetByID(particleID);
        ParticleSystem vfx = vfxData.vfx;

        if (vfx.isPlaying)
            vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        vfx.Play();
    }
}
