using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomClick : MonoBehaviourPunCallbacks
{
    public Text Room;
    public static string RoomName;

    public void OnClickRoom()
    {
        for(int i = 0; i < PhotonInit.myList.Count; i++)
        {
            if(PhotonInit.myList[i].Name.Split('_')[0] == Room.text)
            {
                if (PhotonInit.myList[i].Name.Split('_').Length > 1)
                {
                    RoomName = Room.text;
                    PhotonInit._instance.EnterPassword.SetActive(true);
                }
                else PhotonNetwork.JoinRoom(Room.text);
                break;
            }
        }
    }
}
