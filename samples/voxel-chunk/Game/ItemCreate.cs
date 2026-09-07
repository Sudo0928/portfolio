using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ItemCreate : MonoBehaviourPunCallbacks
{
    public enum ItemGenerator { First, Second, Third}
    public enum UpGrade { None, IronFurnace, GoldFurnace, EmeraldFurnace, DiamondFurnace}

    public ItemGenerator CType;
    public UpGrade upGrade = UpGrade.None;
    public float WaitTime = 1f;
    private GameObject item;

    private float NowTime;

    // Start is called before the first frame update
    void Start()
    {
        NowTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - NowTime > 5f)
        {
            NowTime = Time.time;
            StartCoroutine(Create());
        }
    }

    IEnumerator Create()
    {
        bool Iron = false, Gold = false, Emerald = false;
        switch (CType)
        {
            case ItemGenerator.First:
                break;
            case ItemGenerator.Second:
                break;
            case ItemGenerator.Third:
                switch (upGrade)
                {
                    case UpGrade.None:
                        Collider[] colliders = Physics.OverlapBox(this.transform.position - new Vector3(0, 1, 0), new Vector3(1, 1, 1), Quaternion.identity, 1 << LayerMask.NameToLayer("Item"));
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            if(colliders[i] != null)
                            {
                                if (colliders[i].GetComponent<ItemInfo>().item.ItemName == "ingotIron")
                                {
                                    colliders[i].GetComponent<ItemInfo>().item.Count += 4;
                                    Iron = true;
                                    yield return new WaitForSeconds(2f);
                                }
                                else if (colliders[i].GetComponent<ItemInfo>().item.ItemName == "ingotGold")
                                {
                                    colliders[i].GetComponent<ItemInfo>().item.Count += 1;
                                    Gold = true;
                                    yield return new WaitForSeconds(2f);
                                }
                            } 
                        }
                        if(!Iron)
                        {
                            PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity).GetComponent<ItemInfo>().item = new Item("ingotIron", "ingotIron", Item.ItemType.Item, 4, 64);
                            yield return new WaitForSeconds(2f);
                        }
                        if(!Gold)
                        {
                            PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity).GetComponent<ItemInfo>().item = new Item("ingotGold", "ingotGold", Item.ItemType.Item, 1, 64);
                        }
                        break;
                    case UpGrade.IronFurnace:
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("ingotIron", "ingotIron", Item.ItemType.Item, 8, 64);
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("ingotGold", "ingotGold", Item.ItemType.Item, 2, 64);
                        break;
                    case UpGrade.GoldFurnace:
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("ingotIron", "ingotIron", Item.ItemType.Item, 8, 64);
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("ingotGold", "ingotGold", Item.ItemType.Item, 2, 64);
                        break;
                    case UpGrade.EmeraldFurnace:
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("ingotIron", "ingotIron", Item.ItemType.Item, 8, 64);
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("ingotGold", "ingotGold", Item.ItemType.Item, 2, 64);
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("emerald", "emerald", Item.ItemType.Item, 1, 64);
                        break;
                    case UpGrade.DiamondFurnace:
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("ingotIron", "ingotIron", Item.ItemType.Item, 16, 64);
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("ingotGold", "ingotGold", Item.ItemType.Item, 4, 64);
                        item = PhotonNetwork.Instantiate("Prefabs/Item", this.transform.position, Quaternion.identity);
                        item.GetComponent<ItemInfo>().item = new Item("emerald", "emerald", Item.ItemType.Item, 2, 64);
                        break;
                }
                break;
        }
    }
}
