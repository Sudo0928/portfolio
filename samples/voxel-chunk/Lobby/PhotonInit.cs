using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    public static PhotonInit _instance;
    private string Version = "0.1";
    public string PlayerName = "UserName";
    public byte maxPlayer = 20;
    public InputField text;

    [Header("Prefabs")]
    public GameObject PlayerInfo, RoomInfo;
    [Header("UI")]
    public GameObject EntName, Lobby, CreateRoomSetting, Room, EnterPassword, Warring, EnterRoomName, Waitting;
    [Header("Grid Layout")]
    public GameObject PlayerList, RoomList;
    [Header("PassWord")]
    public InputField CreateRoomName, CreateRoomPassword, EnteredPassword, FindRoomName;
    [Header("PassWord")]
    public Toggle toggle;
    [Header("RoomSetting")]
    public Text RoomName;
    [Header("LockSprite")]
    public Sprite Lock, UnLock;
    [Header("Warring")]
    public Text WarringText, WaittingText;

    private string SaveRoomName, SaveRoomPassword;

    private bool LodingCheck = false, StartCo = true;

    public List<GameObject> RoomLists = new List<GameObject>();
    public List<GameObject> PlayerLists = new List<GameObject>();

    public static List<RoomInfo> myList = new List<RoomInfo>();

    public delegate void Test();

    private void Awake()
    {
        if (!_instance) _instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        PhotonNetwork.GameVersion = Version;
    }

    private void Update()
    {
        if (toggle.isOn)
        {
            CreateRoomPassword.interactable = true;
        }
        else
        {
            CreateRoomPassword.interactable = false;
        }
    }

    public void RemoveAll()
    {
        for(int i = 0; i < RoomLists.Count; i++)
        {
            Destroy(RoomLists[i]);
        }

        if(CreateRoomName.text != null)
        {
            SaveRoomName = CreateRoomName.text;
            CreateRoomName.text = null;
        }
        if(CreateRoomPassword.text != null || !toggle.isOn)
        {
            SaveRoomPassword = CreateRoomPassword.text;
            CreateRoomPassword.text = null;
        }
    }

    void MyListRenewal()
    {
        RemoveAll();
        for(int i = 0; i < myList.Count; i++)
        {
            GameObject temp = Instantiate(RoomInfo, RoomList.transform);
            RoomLists.Add(temp);
            if(myList[i].Name.Split('_').Length > 1)
            {
                temp.transform.GetChild(0).GetComponent<Image>().sprite = Lock;
                temp.transform.GetChild(1).GetComponent<Text>().text = myList[i].Name.Split('_')[0];
                temp.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = myList[i].PlayerCount.ToString() + "/" + myList[i].MaxPlayers;
            }
            else
            {
                temp.transform.GetChild(0).GetComponent<Image>().sprite = UnLock;
                temp.transform.GetChild(1).GetComponent<Text>().text = myList[i].Name;
                temp.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = myList[i].PlayerCount.ToString() + "/" + myList[i].MaxPlayers;
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }

    public void Enter()
    {
        if (text.text != "")
        {
            PlayerName = text.text;
            PhotonNetwork.NickName = PlayerName;
            StartCoroutine(WaittingMessage("로비"));
            PhotonNetwork.ConnectUsingSettings();
        }
        else StartCoroutine(SayWarring("이름을 입력해 주세요."));
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        EntName.SetActive(false);
        Lobby.SetActive(true);
        CreateRoomSetting.SetActive(false);
        Room.SetActive(false);
    }

    public override void OnJoinedLobby()
    {
        EntName.SetActive(false);
        Lobby.SetActive(true);
        CreateRoomSetting.SetActive(false);
        Room.SetActive(false);
        LodingCheck = true;
    }

    public void CreateRoom()
    {
        EntName.SetActive(false);
        Lobby.SetActive(false);
        CreateRoomSetting.SetActive(true);
        Room.SetActive(false);
    }

    public void FindRoom()
    {
        EnterRoomName.SetActive(true);
    }

    public void JoinRoom()
    {
        bool findRoom = false;
        bool existPassword = false;
        RoomInfo temp;

        if(myList.Count != 0)
        {
            for (int i = 0; i < myList.Count; i++)
            {
                Debug.Log(myList[i].Name.Split('_')[0]);
                if (myList[i].Name.Split('_')[0] == FindRoomName.text)
                {
                    if (myList[i].Name.Split('_').Length > 1)
                    {
                        existPassword = true;
                    }
                    else
                    {
                        existPassword = false;
                    }
                    temp = myList[i];
                    findRoom = true;
                }
                else
                {
                    findRoom = false;
                }
            }
        }
        else if(FindRoomName.text != "")
        {
            StartCoroutine(SayWarring("입장할 수 있는 방이 없습니다..."));
            return;
        }
        else
        {
            StartCoroutine(SayWarring("방 이름을 입력해 주세요."));
            return;
        }

        if(findRoom)
        {
            if(existPassword)
            {
                RoomClick.RoomName = FindRoomName.text;
                EnterPassword.SetActive(true);
            }
            else
            {
                StartCoroutine(WaittingMessage("방"));
                PhotonNetwork.JoinRoom(FindRoomName.text);
            }
        }
        else
        {
            StartCoroutine(SayWarring("해당 이름의 방을 찾지 못했습니다."));
        }
    }

    public void Back()
    {
        EntName.SetActive(false);
        Lobby.SetActive(true);
        CreateRoomSetting.SetActive(false);
        Room.SetActive(false);
    }

    public void Next()
    {
        if (CreateRoomName.text != "")
        {
            for (int i = 0; i < myList.Count; i++)
            {
                if (myList[i].Name.Split('_')[0] == CreateRoomName.text)
                {
                    StartCoroutine(SayWarring("같은 이름의 방이 존재합니다."));
                    return;
                }
            }
            if (toggle.isOn && CreateRoomPassword.text != "")
            {
                StartCoroutine(WaittingMessage("방"));
                PhotonNetwork.CreateRoom(CreateRoomName.text + "_" + CreateRoomPassword.text, 
                    new RoomOptions { MaxPlayers = maxPlayer, CleanupCacheOnLeave = false });
            }
            else
            {
                StartCoroutine(WaittingMessage("방"));
                PhotonNetwork.CreateRoom(CreateRoomName.text, 
                    new RoomOptions { MaxPlayers = maxPlayer, CleanupCacheOnLeave = false });
            }
        }
        else
        {
            StartCoroutine(SayWarring("방 이름을 입력하세요."));
            return;
        }
        toggle.isOn = false;

        EntName.SetActive(false);
        Lobby.SetActive(false);
        CreateRoomSetting.SetActive(false);
        Room.SetActive(true);
    }

    public void GameStart()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 1)
            {
                PhotonNetwork.LoadLevel(1);
            }
            else StartCoroutine(SayWarring("인원수가 부족합니다. [현재 인원수 : " + 
                PhotonNetwork.CurrentRoom.PlayerCount + "] [필요한 인원수 : 8]"));
        }
        else StartCoroutine(SayWarring("권한이 없습니다."));
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomPlayers();
    }

    public override void OnJoinedRoom()
    {
        UpdateRoomPlayers();
        EntName.SetActive(false);
        Lobby.SetActive(false);
        CreateRoomSetting.SetActive(false);
        Room.SetActive(true);
        EnterPassword.SetActive(false);
        EnterRoomName.SetActive(false);
        LodingCheck = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomPlayers();
    }

    public void UpdateRoomPlayers()
    {
        for (int i = 0; i < PlayerLists.Count; i++)
        {
            Destroy(PlayerLists[i]);
        }
        List<Player> players = new List<Player>(PhotonNetwork.CurrentRoom.Players.Values);

        for (int i = 0; i < players.Count; i++)
        {
            GameObject temp = Instantiate(PlayerInfo, PlayerList.transform);
            if (players[i].IsMasterClient) temp.transform.GetChild(1).gameObject.SetActive(true);
            PlayerLists.Add(temp);
            temp.transform.GetChild(0).GetComponent<Text>().text = players[i].NickName;
            RoomName.text = PhotonNetwork.CurrentRoom.Name.Split('_')[0];
        }
    }

    public override void OnLeftRoom()
    {
        StartCoroutine(WaittingMessage("로비"));
        PhotonNetwork.JoinLobby();
    }

    public void RandomRoom()
    {
        StartCoroutine(WaittingMessage("방"));

        List<string> publicRoom = new List<string>();

        if (myList.Count > 0)
        {
            for (int i = 0; i < myList.Count; i++)
            {
                if (myList[i].Name.Split('_').Length == 1)
                {
                    publicRoom.Add(myList[i].Name);
                }
            }
        }

        if(publicRoom.Count != 0)
        {
            PhotonNetwork.JoinRoom(publicRoom[Random.Range(0, publicRoom.Count)]);
        }
        else
        {
            LodingCheck = true;
            StartCoroutine(SayWarring("입장할 수 있는 방이 없습니다."));
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        StartCoroutine(Repeat());
        LodingCheck = true;
    }

    public void JoinLobby()
    {
        StartCoroutine(WaittingMessage("로비"));
        PhotonNetwork.LeaveRoom();
    }

    public void OnEnterPassword()
    {
        StartCoroutine(WaittingMessage("방"));
        PhotonNetwork.JoinRoom(RoomClick.RoomName + "_" + EnteredPassword.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LodingCheck = true;
        for (int i = 0; i < myList.Count; i++)
        {
            if (myList[i].Name.Split('_')[0] == RoomClick.RoomName && returnCode == 32758)
            {
                StartCoroutine(SayWarring("비밀번호가 일치하지 않습니다."));
                Debug.Log(returnCode);
                return;
            }
        }
        StartCoroutine(SayWarring("입장할 수 있는 방이 없습니다."));
    }

    IEnumerator Repeat()
    {
        while (true)
        {
            if (!StartCo)
            {
                StartCoroutine(SayWarring("입장할 수 있는 방이 없습니다..."));
                break;
            }
            yield return null;
        }
        yield return null;
    }

    IEnumerator WaittingMessage(string JoinName)
    {
        StartCo = true;
        LodingCheck = false;
        Waitting.SetActive(true);
        int i = 0;
        while (!LodingCheck)
        {
            WaittingText.text = "<color=#0ff>" + JoinName + "</color>" + "에 입장중입니다...".Substring(0,(JoinName.Length + 6) + (i%4));
            if (LodingCheck) break;
            i++;
            yield return new WaitForSeconds(0.25f);
        }
        Waitting.SetActive(false);
        StartCo = false;
    }

    IEnumerator SayWarring(string t)
    {
        Warring.SetActive(true);
        WarringText.text = t;
        yield return new WaitForSeconds(1);
        Warring.SetActive(false);
    }

    

    public void OnClickBack()
    {
        EnterPassword.SetActive(false);
        EnterRoomName.SetActive(false);
    }
}
