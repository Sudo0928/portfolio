using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager _instance;

    private void Awake()
    {
        if (!_instance) _instance = this;
        PhotonNetwork.IsMessageQueueRunning = true;
        ItemDB();
    }

    public Inventory PlayerInv;
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public string strPath = "Assets/Resources";
    public Material textureAtlas;
    public Camera Main_Camera;

    private void Start()
    {
        GameObject temp = PhotonNetwork.Instantiate("Prefabs/Player", new Vector3(-74.5f, 3, -28.5f), Quaternion.identity);
        Main_Camera.transform.SetParent(temp.transform);
        Main_Camera.transform.localPosition = new Vector3(0, 1.8f, 0.2f);
    }

    void ItemDB()
    {
        items.Add(0, new Item("appleGold", "appleGold", Item.ItemType.Use, 1, 64));
        items.Add(1, new Item("arrow", "arrow", Item.ItemType.Use, 1, 64));
        items.Add(2, new Item("bow", "bow", Item.ItemType.Use, 1, 64));
        items.Add(3, new Item("diamond", "diamond", Item.ItemType.Use, 1, 64));
        items.Add(4, new Item("dirt", "dirt", Item.ItemType.Use, 1, 64));
        items.Add(5, new Item("enderPearl", "enderPearl", Item.ItemType.Use, 1, 64));
        items.Add(6, new Item("fireball", "fireball", Item.ItemType.Use, 1, 64));
        items.Add(7, new Item("hatchetDiamond", "hatchetDiamond", Item.ItemType.Use, 1, 64));

        items.Add(8, new Item("cloth_0", "cloth_0", Item.ItemType.Block, Block.BlockType.CLOTH_0, 1, 64));
        items.Add(9, new Item("cloth_1", "cloth_1", Item.ItemType.Block, Block.BlockType.CLOTH_1, 1, 64));
        items.Add(10, new Item("cloth_2", "cloth_2", Item.ItemType.Block, Block.BlockType.CLOTH_2, 1, 64));
        items.Add(11, new Item("cloth_3", "cloth_3", Item.ItemType.Block, Block.BlockType.CLOTH_3, 1, 64));
        items.Add(12, new Item("cloth_4", "cloth_4", Item.ItemType.Block, Block.BlockType.CLOTH_4, 1, 64));
        items.Add(13, new Item("cloth_5", "cloth_5", Item.ItemType.Block, Block.BlockType.CLOTH_5, 1, 64));
        items.Add(14, new Item("cloth_6", "cloth_6", Item.ItemType.Block, Block.BlockType.CLOTH_6, 1, 64));
        items.Add(15, new Item("cloth_7", "cloth_7", Item.ItemType.Block, Block.BlockType.CLOTH_7, 1, 64));
        items.Add(16, new Item("cloth_8", "cloth_8", Item.ItemType.Block, Block.BlockType.CLOTH_8, 1, 64));
        items.Add(17, new Item("cloth_9", "cloth_9", Item.ItemType.Block, Block.BlockType.CLOTH_9, 1, 64));
        items.Add(18, new Item("cloth_10", "cloth_10", Item.ItemType.Block, Block.BlockType.CLOTH_10, 1, 64));
        items.Add(19, new Item("cloth_11", "cloth_11", Item.ItemType.Block, Block.BlockType.CLOTH_11, 1, 64));
        items.Add(20, new Item("cloth_12", "cloth_12", Item.ItemType.Block, Block.BlockType.CLOTH_12, 1, 64));
        items.Add(21, new Item("cloth_13", "cloth_13", Item.ItemType.Block, Block.BlockType.CLOTH_13, 1, 64));
        items.Add(22, new Item("cloth_14", "cloth_14", Item.ItemType.Block, Block.BlockType.CLOTH_14, 1, 64));
        items.Add(23, new Item("cloth_15", "cloth_15", Item.ItemType.Block, Block.BlockType.CLOTH_15, 1, 64));

        items.Add(24, new Item("grass", "grass_side", Item.ItemType.Block, Block.BlockType.GRASS, 1, 64));
        items.Add(25, new Item("dirt", "dirt", Item.ItemType.Block, Block.BlockType.DIRT, 1, 64));
        items.Add(26, new Item("stone", "stone", Item.ItemType.Block, Block.BlockType.STONE, 1, 64));
        items.Add(27, new Item("obsidian", "obsidian", Item.ItemType.Block, Block.BlockType.OBSIDIAN, 1, 64));
        items.Add(28, new Item("sandstone", "sandstone_side", Item.ItemType.Block, Block.BlockType.SANDSTONE, 1, 64));
        items.Add(29, new Item("whiteStone", "whiteStone", Item.ItemType.Block, Block.BlockType.WHITESTONE, 1, 64));
        items.Add(30, new Item("wood", "wood", Item.ItemType.Block, Block.BlockType.WOOD, 1, 64));
        items.Add(31, new Item("wood_birch", "wood_birch", Item.ItemType.Block, Block.BlockType.WOOD_BRICH, 1, 64));
        items.Add(32, new Item("wood_jungle", "wood_jungle", Item.ItemType.Block, Block.BlockType.WOOD_JUNGLE, 1, 64));
        items.Add(33, new Item("wood_spruce", "wood_spruce", Item.ItemType.Block, Block.BlockType.WOOD_SPRUCE, 1, 64));

        items.Add(34, new Item("oreCoal", "oreCoal", Item.ItemType.Block, Block.BlockType.ORE_COAL, 1, 64));
        items.Add(35, new Item("oreIron", "oreIron", Item.ItemType.Block, Block.BlockType.ORE_IRON, 1, 64));
        items.Add(36, new Item("oreGold", "oreGold", Item.ItemType.Block, Block.BlockType.ORE_GOLD, 1, 64));
        items.Add(37, new Item("oreDiamond", "oreDiamond", Item.ItemType.Block, Block.BlockType.ORE_DIAMOND, 1, 64));
        items.Add(38, new Item("oreEmerald", "oreEmerald", Item.ItemType.Block, Block.BlockType.ORE_EMERALD, 1, 64));
        items.Add(39, new Item("oreLapis", "oreLapis", Item.ItemType.Block, Block.BlockType.ORE_LAPIS, 1, 64));
        items.Add(40, new Item("oreRedstone", "oreRedstone", Item.ItemType.Block, Block.BlockType.ORE_REDSTONE, 1, 64));

        items.Add(41, new Item("stoneslab_side", "stoneslab_side", Item.ItemType.Block, Block.BlockType.STONESLAB, 1, 64));

        items.Add(42, new Item("blockDiamond", "blockDiamond", Item.ItemType.Block, Block.BlockType.BLOCK_DIAMOND, 1, 64));
        items.Add(43, new Item("blockEmerald", "blockEmerald", Item.ItemType.Block, Block.BlockType.BLOCK_EMERALD, 1, 64));
        items.Add(44, new Item("blockGold", "blockGold", Item.ItemType.Block, Block.BlockType.BLOCK_GOLD, 1, 64));
        items.Add(45, new Item("blockIron", "blockIron", Item.ItemType.Block, Block.BlockType.BLOCK_IRON, 1, 64));
        items.Add(46, new Item("blockLapis", "blockLapis", Item.ItemType.Block, Block.BlockType.BLOCK_LAPIS, 1, 64));
        items.Add(47, new Item("blockRedstone", "blockRedstone", Item.ItemType.Block, Block.BlockType.BLOCK_REDSTONE, 1, 64));
        items.Add(48, new Item("bookshelf", "bookshelf", Item.ItemType.Block, Block.BlockType.BOOK_SHELF, 1, 64));
        items.Add(49, new Item("brick", "brick", Item.ItemType.Block, Block.BlockType.BRICK, 1, 64));
        items.Add(50, new Item("clay", "clay", Item.ItemType.Block, Block.BlockType.CLAY, 1, 64));
        items.Add(51, new Item("leaves", "leaves", Item.ItemType.Block, Block.BlockType.LEAVES, 1, 64));
        items.Add(52, new Item("leaves_jungle", "leaves_jungle", Item.ItemType.Block, Block.BlockType.LEAVES_JUNGLE, 1, 64));
        items.Add(53, new Item("leaves_jungle_opaque", "leaves_jungle_opaque", Item.ItemType.Block, Block.BlockType.LEAVES_JUNGLE_OPAQUE, 1, 64));
        items.Add(54, new Item("leaves_opaque", "leaves_opaque", Item.ItemType.Block, Block.BlockType.LEAVES_OPAQUE, 1, 64));
        items.Add(55, new Item("leaves_spruce", "leaves_spruce", Item.ItemType.Block, Block.BlockType.LEAVES_SPRUCE, 1, 64));
        items.Add(56, new Item("leaves_spruce_opaque", "leaves_spruce_opaque", Item.ItemType.Block, Block.BlockType.LEAVES_SPRUCE_OPAQUE, 1, 64));
        items.Add(57, new Item("netherBrick", "netherBrick", Item.ItemType.Block, Block.BlockType.NETHER_BRICK, 1, 64));
        items.Add(58, new Item("netherquartz", "netherquartz", Item.ItemType.Block, Block.BlockType.NETHER_QUARTZ, 1, 64));
        items.Add(59, new Item("quartzblock", "quartzblock_side", Item.ItemType.Block, Block.BlockType.QUARTZ_BLOCK, 1, 64));
        items.Add(60, new Item("quartzblock_chiseled", "quartzblock_chiseled", Item.ItemType.Block, Block.BlockType.QUARTZ_BLOCK_CHISELED, 1, 64));
        items.Add(61, new Item("quartzblock_lines", "quartzblock_lines", Item.ItemType.Block, Block.BlockType.QUARTZ_BLOCK_LINES, 1, 64));
        items.Add(62, new Item("snow", "snow", Item.ItemType.Block, Block.BlockType.SNOW, 1, 64));
        items.Add(63, new Item("snow_side", "snow_side", Item.ItemType.Block, Block.BlockType.SNOW_GRASS, 1, 64));
        items.Add(64, new Item("sponge", "sponge", Item.ItemType.Block, Block.BlockType.SPONGE, 1, 64));
        items.Add(65, new Item("stonebrick", "stonebrick", Item.ItemType.Block, Block.BlockType.STONE_BRICK, 1, 64));
        items.Add(66, new Item("stonebricksmooth", "stonebricksmooth", Item.ItemType.Block, Block.BlockType.STONE_BRICK_SMOOTH, 1, 64));
        items.Add(67, new Item("stonebricksmooth_carved", "stonebricksmooth_carved", Item.ItemType.Block, Block.BlockType.STONE_BRICK_SMOOTH_CARVED, 1, 64));
        items.Add(68, new Item("stonebricksmooth_cracked", "stonebricksmooth_cracked", Item.ItemType.Block, Block.BlockType.STONE_BRICK_SMOOTH_CRACKED, 1, 64));
        items.Add(69, new Item("stonebricksmooth_mossy", "stonebricksmooth_mossy", Item.ItemType.Block, Block.BlockType.STONE_BRICK_SMOOTH_MOSSY, 1, 64));
        items.Add(70, new Item("stoneMoss", "stoneMoss", Item.ItemType.Block, Block.BlockType.STONE_MOSSY, 1, 64));
        items.Add(71, new Item("tree_birch", "tree_birch", Item.ItemType.Block, Block.BlockType.TREE_BIRCH, 1, 64));
        items.Add(72, new Item("tree_jungle", "tree_jungle", Item.ItemType.Block, Block.BlockType.TREE_JUNGLE, 1, 64));
        items.Add(73, new Item("tree_side", "tree_side", Item.ItemType.Block, Block.BlockType.TREE_SIDE, 1, 64));

        items.Add(74, new Item("swordDiamond", "swordDiamond", Item.ItemType.Weapon, 1, 1));
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            PhotonView temp = players[i].GetComponent<PhotonView>();
            if (temp.OwnerActorNr == otherPlayer.ActorNumber)
            {
                PhotonNetwork.Destroy(players[i]);
            }
        }
    }
}
