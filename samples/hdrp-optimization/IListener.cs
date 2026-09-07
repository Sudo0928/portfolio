using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EVENT_TYPE
{
    Interaction,
    OnLIghtSwitch,
    OffLightSwitch
};

public interface IListener
{
    void OnEvent(EVENT_TYPE type, Component sender, object Param = null);
}
