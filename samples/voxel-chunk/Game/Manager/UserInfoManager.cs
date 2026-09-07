using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public enum PlayerState { Alive, Die };

public class UserInfoManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static UserInfoManager _instance;
    private void Awake()
    {
        if (!_instance) _instance = this;
    }

    public PlayerState playerState = PlayerState.Alive;

    private int playerHealth = 20;
    private int playerMaxHealth = 20;

    public int PlayerHealth
    {
        get { return playerHealth; }
        set
        {
            switch (playerState)
            {
                case PlayerState.Alive:
                    if (value <= 0)
                    {
                        playerState = PlayerState.Die;
                        playerHealth = 0;
                    }
                    else if (value >= playerMaxHealth)
                    {
                        playerHealth = playerMaxHealth;
                    }
                    else
                    {
                        playerHealth = value;
                    }
                    UIManager._instance.DrawUI();
                    break;
                case PlayerState.Die:
                    break;
            }
        }
    }

    public int PlayerMaxHealth
    {
        get { return playerMaxHealth; }
        set { playerMaxHealth = value; }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerHealth);
        }
        else
        {
            playerHealth = (int)stream.ReceiveNext();
        }
    }
}
