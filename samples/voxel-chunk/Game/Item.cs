using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string ItemName = "";
    public string Sprite = "";
    public ItemType iType;
    public int Count = 1;
    public int MaxCount = 64;
    public Block.BlockType bType;

    public Item()
    {
        ItemName = "";
        Sprite = "";
        Count = 1;
        MaxCount = 64;
        iType = ItemType.Item;
        bType = Block.BlockType.AIR;
    }

    public Item(string itemName, string sprite, ItemType iType, Block.BlockType bType , int count, int maxCount)
    {
        ItemName = itemName;
        Sprite = sprite;
        this.iType = iType;
        Count = count;
        MaxCount = maxCount;
        this.bType = bType;
    }

    public Item(string itemName, string sprite, ItemType iType, int count, int maxCount)
    {
        ItemName = itemName;
        Sprite = sprite;
        this.iType = iType;
        Count = count;
        MaxCount = maxCount;
    }

    public Item(Item item)
    {
        ItemName = item.ItemName;
        Sprite = item.Sprite;
        this.iType = item.iType;
        Count = item.Count;
        MaxCount = item.MaxCount;
        this.bType = item.bType;
    }

    public enum ItemType
    {
        Use,
        Block,
        Weapon,
        Item
    }
}
