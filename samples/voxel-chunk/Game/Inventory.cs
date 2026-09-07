using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;

public class Inventory : MonoBehaviourPunCallbacks, IPunObservable
{
    private void Awake()
    {
        if(!photonView.IsMine)
        {
            return;
        }
        GameManager._instance.PlayerInv = this;
    }

    public string temp;

    public List<Item> inventory = new List<Item>();

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 36; i++)
        {
            inventory.Add(new Item());
        }
        ItemAdd(46);
        ItemAdd(74);
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            temp = JsonConvert.SerializeObject(inventory);
        }
        else
        {
            inventory = JsonConvert.DeserializeObject<List<Item>>(temp);
        }
    }

    public void ItemAdd(Item item)
    {
        for(int i = 0; i < inventory.Count; i++)
        {
            if(ItemInfo.SameItem(inventory[i], item))
            {
                if(inventory[i].Count + item.Count > inventory[i].MaxCount)
                {
                    item.Count = inventory[i].Count + item.Count - item.MaxCount;
                    inventory[i].Count = inventory[i].MaxCount;
                }
                else
                {
                    inventory[i].Count += item.Count;
                    DrawInv();
                    DrawItemBar();
                    return;
                }
            }
            else if(inventory[i].ItemName == "")
            {
                if(item.Count > item.MaxCount)
                {
                    inventory[i] = new Item(item);
                    inventory[i].Count = inventory[i].MaxCount;
                    item.Count -= item.MaxCount;
                }
                else
                {
                    inventory[i] = item;
                    DrawInv();
                    DrawItemBar();
                    return;
                }
            }
        }
    }

    public void ItemAdd(int code)
    {
        Item item;
        GameManager._instance.items.TryGetValue(code, out item);
        ItemAdd(item);
        return;
    }

    public void DrawInv()
    {
        for(int i = 0; i < inventory.Count; i++)
        {
            if(UIManager._instance.Inventory.transform.GetChild(0).GetChild(i).childCount > 0)
            {
                for(int j = 0; j < UIManager._instance.Inventory.transform.GetChild(0).GetChild(i).childCount; j++)
                {
                    Destroy(UIManager._instance.Inventory.transform.GetChild(0).GetChild(i).GetChild(j).gameObject);
                }
            }
            if(inventory[i].ItemName != "")
            {
                GameObject temp = Instantiate(Resources.Load<GameObject>("Prefabs/ItemImg"), UIManager._instance.Inventory.transform.GetChild(0).GetChild(i));
                if(inventory[i].iType == Item.ItemType.Block)
                {
                    temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("Block/" + inventory[i].Sprite);
                }
                else
                {
                    temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("Item/" + inventory[i].Sprite);
                }
                
                if (inventory[i].Count > 1)
                {
                    temp.transform.GetChild(0).GetComponent<Text>().text = inventory[i].Count.ToString();
                }
                else temp.transform.GetChild(0).GetComponent<Text>().text = "";
            }
        }
    }

    public void DrawItemBar()
    {
        for(int i = 0; i < UIManager._instance.ItemBar.transform.childCount; i++)
        {
            if(UIManager._instance.ItemBar.transform.GetChild(0).childCount > 0)
            {
                for(int j = 0; j < UIManager._instance.ItemBar.transform.GetChild(i).childCount; j++)
                {
                    if(UIManager._instance.ItemBar.transform.GetChild(i).GetChild(j).name != "Pick")
                    {
                        Destroy(UIManager._instance.ItemBar.transform.GetChild(i).GetChild(j).gameObject);
                    }
                }
            }
            if(inventory[i].ItemName != "")
            {
                Debug.Log(inventory[i].ItemName);
                GameObject temp = Instantiate(Resources.Load<GameObject>("Prefabs/ItemBarImg"), UIManager._instance.ItemBar.transform.GetChild(i));
                if (inventory[i].iType == Item.ItemType.Block)
                {
                    temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("Block/" + inventory[i].Sprite);
                }
                else
                {
                    temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("Item/" + inventory[i].Sprite);
                }
                if (inventory[i].Count > 1)
                {
                    temp.transform.GetChild(0).GetComponent<Text>().text = inventory[i].Count.ToString();
                }
                else temp.transform.GetChild(0).GetComponent<Text>().text = "";
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(temp);
        }
        else
        {
            temp = (string)stream.ReceiveNext();
        }
    }
}
