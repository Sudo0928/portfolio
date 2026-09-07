using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EventManger : MonoBehaviour
{
    public Toggle mode1, mode2, mode3;

    public void ModeClick()
    {
        switch(this.name)
        {
            case "개인전":
                if (mode1.interactable != true) break;
                mode1.isOn = true;
                mode2.isOn = false;
                mode3.isOn = false;
                break;
            case "2명":
                if (mode2.interactable != true) break;
                mode1.isOn = false;
                mode2.isOn = true;
                mode3.isOn = false;
                break;
            case "4명":
                if (mode3.interactable != true) break;
                mode1.isOn = false;
                mode2.isOn = false;
                mode3.isOn = true;
                break;
        }
    }
}
