using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DarkTonic.MasterAudio;

public class LightSwitch : MonoBehaviour, IListener
{
    private bool onSwitch = false;

    public string sType = "Plastic Switch 6";

    public List<LampControl> lampControls = new List<LampControl>();

    public void Start()
    {
        EventManager._Instance.AddListener(EVENT_TYPE.Interaction, this);
    }

    public void OnEvent(EVENT_TYPE type, Component sender, object Param = null)
    {
        if (!IsChild(sender)) return;

        ToggleLight();
        MasterAudio.PlaySound3DAtTransform(sType, transform);
    }

    public bool IsChild(Component sender)
    {
        if (sender.transform.Equals(transform)) return true;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).transform.Equals(sender.transform)) return true;
        }

        return false;
    }

    public void ToggleLight()
    {
        if(onSwitch)
        {
            OffLights();
            onSwitch = false;
        }
        else
        {
            OnLights();
            onSwitch = true;
        }
    }
    
    public void OnLights()
    {
        for (int i = 0; i < lampControls.Count; i++)
        {
            lampControls[i].OnLight();
            EventManager._Instance.PostNotification(EVENT_TYPE.OnLIghtSwitch, this);
        }
    }

    public void OffLights()
    {
        for (int i = 0; i < lampControls.Count; i++)
        {
            lampControls[i].OffLight();
            EventManager._Instance.PostNotification(EVENT_TYPE.OffLightSwitch, this);
        }
    }
}
