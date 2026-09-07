using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public enum Cubeside { BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK };
    public enum BlockType { AIR, GRASS, DIRT, STONE, CLOTH_0, CLOTH_1, CLOTH_2, CLOTH_3, CLOTH_4, CLOTH_5, CLOTH_6, CLOTH_7, CLOTH_8, CLOTH_9, CLOTH_10, CLOTH_11, CLOTH_12, CLOTH_13, CLOTH_14, CLOTH_15,
                            OBSIDIAN, SANDSTONE, WHITESTONE, WOOD, WOOD_BRICH, WOOD_JUNGLE, WOOD_SPRUCE, ORE_COAL, ORE_IRON, ORE_GOLD, ORE_DIAMOND, ORE_EMERALD, ORE_LAPIS, ORE_REDSTONE, STONESLAB, STONESLAB2,
                            BLOCK_DIAMOND, BLOCK_EMERALD, BLOCK_GOLD, BLOCK_IRON, BLOCK_LAPIS, BLOCK_REDSTONE, BOOK_SHELF, BRICK, CLAY, LEAVES, LEAVES_JUNGLE, LEAVES_JUNGLE_OPAQUE, LEAVES_OPAQUE, LEAVES_SPRUCE, LEAVES_SPRUCE_OPAQUE,
                            NETHER_BRICK, NETHER_QUARTZ, QUARTZ_BLOCK, QUARTZ_BLOCK_CHISELED, QUARTZ_BLOCK_LINES, SNOW, SNOW_GRASS, SPONGE, STONE_BRICK, STONE_BRICK_SMOOTH, STONE_BRICK_SMOOTH_CARVED, STONE_BRICK_SMOOTH_CRACKED, STONE_BRICK_SMOOTH_MOSSY,
                            STONE_MOSSY, TREE_BIRCH, TREE_JUNGLE, TREE_SIDE, TREE_SPRUCE};

    public BlockType bType;
    public bool bHalf = false;
    public bool isSolid;
    public Chunk owner;
    public GameObject parent;
    public Vector3 position;

    Vector2[,] blockUVs =
    {
        //                                      +0.0001  +0.0001                 -0.0001  +0.0001                +0.0001  -0.0001                 -0.0001  -0.0001
        /* GRASS TOP */                         {new Vector2(0.2126f, 0.9876f)  , new Vector2(0.2249f, 0.9876f) , new Vector2(0.2126f, 0.9999f)  , new Vector2(0.2249f, 0.9999f)}, //
        /* GRASS SIDE */                        {new Vector2(0.2251f, 0.9876f)  , new Vector2(0.2374f, 0.9876f) , new Vector2(0.2251f, 0.9999f)  , new Vector2(0.2374f, 0.9999f)}, //
        /* DIRT */                              {new Vector2(0.2001f, 0.9876f)  , new Vector2(0.2124f, 0.9876f) , new Vector2(0.2001f, 0.9999f)  , new Vector2(0.2124f, 0.9999f)},
        /* STONE */                             {new Vector2(0.3501f, 0.9876f)  , new Vector2(0.3624f, 0.9876f) , new Vector2(0.3501f, 0.9999f)  , new Vector2(0.3624f, 0.9999f)},

        /* CLOTH-0 */                           {new Vector2(0.0001f, 0.9876f)  , new Vector2(0.0124f, 0.9876f) , new Vector2(0.0001f, 0.9999f)  , new Vector2(0.0124f, 0.9999f)},
        /* CLOTH-1 */                           {new Vector2(0.0126f, 0.9876f)  , new Vector2(0.0249f, 0.9876f) , new Vector2(0.0126f, 0.9999f)  , new Vector2(0.0249f, 0.9999f)},
        /* CLOTH-2 */                           {new Vector2(0.0251f, 0.9876f)  , new Vector2(0.0374f, 0.9876f) , new Vector2(0.0251f, 0.9999f)  , new Vector2(0.0374f, 0.9999f)},
        /* CLOTH-3 */                           {new Vector2(0.0376f, 0.9876f)  , new Vector2(0.0499f, 0.9876f) , new Vector2(0.0376f, 0.9999f)  , new Vector2(0.0499f, 0.9999f)},
        /* CLOTH-4 */                           {new Vector2(0.0501f, 0.9876f)  , new Vector2(0.0624f, 0.9876f) , new Vector2(0.0501f, 0.9999f)  , new Vector2(0.0624f, 0.9999f)},
        /* CLOTH-5 */                           {new Vector2(0.0626f, 0.9876f)  , new Vector2(0.0749f, 0.9876f) , new Vector2(0.0626f, 0.9999f)  , new Vector2(0.0749f, 0.9999f)},
        /* CLOTH-6 */                           {new Vector2(0.0751f, 0.9876f)  , new Vector2(0.0874f, 0.9876f) , new Vector2(0.0751f, 0.9999f)  , new Vector2(0.0874f, 0.9999f)},
        /* CLOTH-7 */                           {new Vector2(0.0876f, 0.9876f)  , new Vector2(0.0999f, 0.9876f) , new Vector2(0.0876f, 0.9999f)  , new Vector2(0.0999f, 0.9999f)},
        /* CLOTH-8 */                           {new Vector2(0.1001f, 0.9876f)  , new Vector2(0.1124f, 0.9876f) , new Vector2(0.1001f, 0.9999f)  , new Vector2(0.1124f, 0.9999f)},
        /* CLOTH-9 */                           {new Vector2(0.1126f, 0.9876f)  , new Vector2(0.1249F, 0.9876f) , new Vector2(0.1126f, 0.9999f)  , new Vector2(0.1249f, 0.9999f)},
        /* CLOTH-10 */                          {new Vector2(0.1251f, 0.9876f)  , new Vector2(0.1374f, 0.9876f) , new Vector2(0.1251f, 0.9999f)  , new Vector2(0.1374f, 0.9999f)},
        /* CLOTH-11 */                          {new Vector2(0.1376f, 0.9876f)  , new Vector2(0.1499f, 0.9876f) , new Vector2(0.1376f, 0.9999f)  , new Vector2(0.1499f, 0.9999f)},
        /* CLOTH-12 */                          {new Vector2(0.1501f, 0.9876f)  , new Vector2(0.1624f, 0.9876f) , new Vector2(0.1501f, 0.9999f)  , new Vector2(0.1624f, 0.9999f)},
        /* CLOTH-13 */                          {new Vector2(0.1626f, 0.9876f)  , new Vector2(0.1749f, 0.9876f) , new Vector2(0.1626f, 0.9999f)  , new Vector2(0.1749f, 0.9999f)},
        /* CLOTH-14 */                          {new Vector2(0.1751f, 0.9876f)  , new Vector2(0.1874f, 0.9876f) , new Vector2(0.1751f, 0.9999f)  , new Vector2(0.1874f, 0.9999f)},
        /* CLOTH-15 */                          {new Vector2(0.1876f, 0.9876f)  , new Vector2(0.1999f, 0.9876f) , new Vector2(0.1876f, 0.9999f)  , new Vector2(0.1999f, 0.9999f)},

        /* OBSIDIAN */                          {new Vector2(0.2376f, 0.9876f)  , new Vector2(0.2499f, 0.9876f) , new Vector2(0.2376f, 0.9999f)  , new Vector2(0.2499f, 0.9999f)},

        #region SANDSTONE
        /* SANDSTONE SIDE */                    {new Vector2(0.2751f, 0.9876f)  , new Vector2(0.2874f, 0.9876f) , new Vector2(0.2751f, 0.9999f)  , new Vector2(0.2874f, 0.9999f)},
        // SANDSTONE TOP                        {new Vector2(0.2626f, 0.9876f)  , new Vector2(0.2749f, 0.9876f) , new Vector2(0.2626f, 0.9999f)  , new Vector2(0.2749f, 0.9999f)},
        // SANDSTONE BOTTOM                     {new Vector2(0.2501f, 0.9876f)  , new Vector2(0.2624f, 0.9876f) , new Vector2(0.2501f, 0.9999f)  , new Vector2(0.2624f, 0.9999f)},
        #endregion

        /* WHITESTONE */                        {new Vector2(0.2876f, 0.9876f)  , new Vector2(0.2999f, 0.9876f) , new Vector2(0.2876f, 0.9999f)  , new Vector2(0.2999f, 0.9999f)},
        
        /* WOOD */                              {new Vector2(0.3001f, 0.9876f)  , new Vector2(0.3124f, 0.9876f) , new Vector2(0.3001f, 0.9999f)  , new Vector2(0.3124f, 0.9999f)},
        /* WOOD BIRCH */                        {new Vector2(0.3126f, 0.9876f)  , new Vector2(0.3249f, 0.9876f) , new Vector2(0.3126f, 0.9999f)  , new Vector2(0.3249f, 0.9999f)},
        /* WOOD JUNGLE */                       {new Vector2(0.3251f, 0.9876f)  , new Vector2(0.3374f, 0.9876f) , new Vector2(0.3251f, 0.9999f)  , new Vector2(0.3374f, 0.9999f)},
        /* WOOD SPRUCE */                       {new Vector2(0.3376f, 0.9876f)  , new Vector2(0.3499f, 0.9876f) , new Vector2(0.3376f, 0.9999f)  , new Vector2(0.3499f, 0.9999f)},

        /* ORE COAL */                          {new Vector2(0.3626f, 0.9876f)  , new Vector2(0.3749f, 0.9876f) , new Vector2(0.3626f, 0.9999f)  , new Vector2(0.3749f, 0.9999f)},
        /* ORE IRON */                          {new Vector2(0.3751f, 0.9876f)  , new Vector2(0.3874f, 0.9876f) , new Vector2(0.3751f, 0.9999f)  , new Vector2(0.3874f, 0.9999f)},
        /* ORE GOLD */                          {new Vector2(0.3876f, 0.9876f)  , new Vector2(0.3999f, 0.9876f) , new Vector2(0.3876f, 0.9999f)  , new Vector2(0.3999f, 0.9999f)},
        /* ORE DIAMOND */                       {new Vector2(0.4001f, 0.9876f)  , new Vector2(0.4124f, 0.9876f) , new Vector2(0.4001f, 0.9999f)  , new Vector2(0.4124f, 0.9999f)},
        /* ORE EMERALD */                       {new Vector2(0.4126f, 0.9876f)  , new Vector2(0.4249f, 0.9876f) , new Vector2(0.4126f, 0.9999f)  , new Vector2(0.4249f, 0.9999f)},
        /* ORE LAPIS */                         {new Vector2(0.4251f, 0.9876f)  , new Vector2(0.4374f, 0.9876f) , new Vector2(0.4251f, 0.9999f)  , new Vector2(0.4374f, 0.9999f)},
        /* ORE REDSTONE */                      {new Vector2(0.4376f, 0.9876f)  , new Vector2(0.4499f, 0.9876f) , new Vector2(0.4376f, 0.9999f)  , new Vector2(0.4499f, 0.9999f)},

        /* STONESLAB SIDE */                    {new Vector2(0.4501f, 0.9876f)  , new Vector2(0.4624f, 0.9876f) , new Vector2(0.4501f, 0.99375f) , new Vector2(0.4624f, 0.99375f)},
        /* STONESLAB2 SIDE */                   {new Vector2(0.4501f, 0.9876f)  , new Vector2(0.4624f, 0.9876f) , new Vector2(0.4501f, 0.9999f)  , new Vector2(0.4624f, 0.9999f)},

        /* BLOCK DIAMOND */                     {new Vector2(0.4751f, 0.9876f)  , new Vector2(0.4874f, 0.9876f) , new Vector2(0.4751f, 0.9999f)  , new Vector2(0.4874f, 0.9999f)},
        /* BLOCK EMERALD */                     {new Vector2(0.4875f, 0.9875f)  , new Vector2(0.5f   , 0.9875f) , new Vector2(0.4875f, 1f     )  , new Vector2(0.5f   , 1f     )},
        /* BLOCK GOLD */                        {new Vector2(0.5f   , 0.9875f)  , new Vector2(0.5125f, 0.9875f) , new Vector2(0.5f   , 1f     )  , new Vector2(0.5125f, 1f     )},
        /* BLOCK IRON */                        {new Vector2(0.5125f, 0.9875f)  , new Vector2(0.525f , 0.9875f) , new Vector2(0.5125f, 1f     )  , new Vector2(0.525f , 1f     )},
        /* BLOCK LAPIS */                       {new Vector2(0.525f , 0.9875f)  , new Vector2(0.5375f, 0.9875f) , new Vector2(0.525f , 1f     )  , new Vector2(0.5375f, 1f     )},
        /* BLOCK REDSTONE */                    {new Vector2(0.5375f, 0.9875f)  , new Vector2(0.55f  , 0.9875f) , new Vector2(0.5375f, 1f     )  , new Vector2(0.55f  , 1f     )},

        /* BOOK SHELF */                        {new Vector2(0.55f  , 0.9875f)  , new Vector2(0.5625f, 0.9875f) , new Vector2(0.55f  , 1f     )  , new Vector2(0.5625f, 1f     )},

        /* BRICK */                             {new Vector2(0.5625f, 0.9875f)  , new Vector2(0.575f , 0.9875f) , new Vector2(0.5625f, 1f     )  , new Vector2(0.575f , 1f     )},
        /* CLAY */                              {new Vector2(0.575f , 0.9875f)  , new Vector2(0.5875f, 0.9875f) , new Vector2(0.575f , 1f     )  , new Vector2(0.5875f, 1f     )},
        /* LEAVES */                            {new Vector2(0.5875f, 0.9875f)  , new Vector2(0.6f   , 0.9875f) , new Vector2(0.5875f, 1f     )  , new Vector2(0.6f   , 1f     )},
        /* LEAVES JUNGLE */                     {new Vector2(0.6f   , 0.9875f)  , new Vector2(0.6125f, 0.9875f) , new Vector2(0.6f   , 1f     )  , new Vector2(0.6125f, 1f     )},
        /* LEAVES JUNGLE OPAQUE */              {new Vector2(0.6125f, 0.9875f)  , new Vector2(0.625f , 0.9875f) , new Vector2(0.6125f, 1f     )  , new Vector2(0.625f , 1f     )},
        /* LEAVES OPAQUE */                     {new Vector2(0.625f , 0.9875f)  , new Vector2(0.6375f, 0.9875f) , new Vector2(0.625f , 1f     )  , new Vector2(0.6375f, 1f     )},
        /* LEAVES SPRUCE */                     {new Vector2(0.6375f, 0.9875f)  , new Vector2(0.65f  , 0.9875f) , new Vector2(0.6375f, 1f     )  , new Vector2(0.65f  , 1f     )},
        /* LEAVES SPRUCE OPAQUE */              {new Vector2(0.65f  , 0.9875f)  , new Vector2(0.6625f, 0.9875f) , new Vector2(0.65f  , 1f     )  , new Vector2(0.6625f, 1f     )},
        /* NETHER BRICK */                      {new Vector2(0.6625f, 0.9875f)  , new Vector2(0.675f , 0.9875f) , new Vector2(0.6625f, 1f     )  , new Vector2(0.675f , 1f     )},
        /* NETHER QUARTZ */                     {new Vector2(0.675f , 0.9875f)  , new Vector2(0.6875f, 0.9875f) , new Vector2(0.675f , 1f     )  , new Vector2(0.6875f, 1f     )},

        #region QUARTZ BLOCK
        /* QUARTZ BLOCK SIDE */                 {new Vector2(0.75f  , 0.9875f)  , new Vector2(0.7625f, 0.9875f) , new Vector2(0.75f  , 1f     )  , new Vector2(0.7625f, 1f     )},
        // QUARTZ BLOCK TOP                     {new Vector2(0.7625f, 0.9875f)  , new Vector2(0.775f , 0.9875f) , new Vector2(0.7625f, 1f     )  , new Vector2(0.775f , 1f     )},
        // QUARTZ BLOCK BOTTOM                  {new Vector2(0.6875f, 0.9875f)  , new Vector2(0.7f   , 0.9875f) , new Vector2(0.6875f, 1f     )  , new Vector2(0.7f   , 1f     )},
        #endregion

        #region QUARTZ BLOCK SHISELED
        /* QUARTZ BLOCK CHISELED */             {new Vector2(0.7f   , 0.9875f)  , new Vector2(0.7125f, 0.9875f) , new Vector2(0.7f   , 1f     )  , new Vector2(0.7125f, 1f     )},
        // QUARTZ BLOCK CHISELED TOP            {new Vector2(0.7125f, 0.9875f)  , new Vector2(0.725f , 0.9875f) , new Vector2(0.7125f, 1f     )  , new Vector2(0.725f , 1f     )},
        #endregion

        #region QUARTZ BLOCK LINES
        /* QUARTZ BLOCK LINES */                {new Vector2(0.725f , 0.9875f)  , new Vector2(0.7375f, 0.9875f) , new Vector2(0.725f , 1f     )  , new Vector2(0.7375f, 1f     )},
        // QUARTZ BLOCK LINES TOP               {new Vector2(0.7375f, 0.9875f)  , new Vector2(0.75f  , 0.9875f) , new Vector2(0.7375f, 1f     )  , new Vector2(0.75f  , 1f     )},
        #endregion

        /* SNOW */                              {new Vector2(0.775f , 0.9875f)  , new Vector2(0.7875f, 0.9875f) , new Vector2(0.775f , 1f     )  , new Vector2(0.7875f, 1f     )},
        /* SNOW_SIDE */                         {new Vector2(0.7875f, 0.9875f)  , new Vector2(0.8f   , 0.9875f) , new Vector2(0.7875f, 1f     )  , new Vector2(0.8f   , 1f     )},
        /* SPONGE */                            {new Vector2(0.8f   , 0.9875f)  , new Vector2(0.8125f, 0.9875f) , new Vector2(0.8f   , 1f     )  , new Vector2(0.8125f, 1f     )},

        /* STONE BRICK */                       {new Vector2(0.8125f, 0.9875f)  , new Vector2(0.825f , 0.9875f) , new Vector2(0.8125f, 1f     )  , new Vector2(0.825f , 1f     )},

        /* STONE BRICK SMOOTH */                {new Vector2(0.825f , 0.9875f)  , new Vector2(0.8375f, 0.9875f) , new Vector2(0.825f , 1f     )  , new Vector2(0.8375f, 1f     )},
        /* STONE BRICK SMOOTH CARVED */         {new Vector2(0.8375f, 0.9875f)  , new Vector2(0.85f  , 0.9875f) , new Vector2(0.8375f, 1f     )  , new Vector2(0.85f  , 1f     )},
        /* STONE BRICK SMOOTH CRACKED */        {new Vector2(0.85f  , 0.9875f)  , new Vector2(0.8625f, 0.9875f) , new Vector2(0.85f  , 1f     )  , new Vector2(0.8625f, 1f     )},
        /* STONE BRICK SMOOTH MOSSY */          {new Vector2(0.8625f, 0.9875f)  , new Vector2(0.875f , 0.9875f) , new Vector2(0.8625f, 1f     )  , new Vector2(0.875f , 1f     )},

        /* STONE MOSSY */                       {new Vector2(0.875f , 0.9875f)  , new Vector2(0.8875f, 0.9875f) , new Vector2(0.875f , 1f     )  , new Vector2(0.8875f, 1f     )},

        #region TREE
        /* TREE BRICH */                        {new Vector2(0.8875f, 0.9875f)  , new Vector2(0.9f   , 0.9875f) , new Vector2(0.8875f, 1f     )  , new Vector2(0.9f   , 1f     )},
        /* TREE JUNGLE */                       {new Vector2(0.9f   , 0.9875f)  , new Vector2(0.9125f, 0.9875f) , new Vector2(0.9f   , 1f     )  , new Vector2(0.9125f, 1f     )},
        /* TREE SIDE */                         {new Vector2(0.9125f, 0.9875f)  , new Vector2(0.925f , 0.9875f) , new Vector2(0.9125f, 1f     )  , new Vector2(0.925f , 1f     )},
        /* TREE SPRUCE */                       {new Vector2(0.925f , 0.9875f)  , new Vector2(0.9375f, 0.9875f) , new Vector2(0.925f , 1f     )  , new Vector2(0.9375f, 1f     )},
        // TREE TOP                             {new Vector2(0.9375f, 0.9875f)  , new Vector2(0.95f  , 0.9875f) , new Vector2(0.9375f, 1f     )  , new Vector2(0.95f  , 1f     )},
        #endregion
    };

    public Block(BlockType b, Vector3 pos, GameObject p, Chunk o)
    {
        bType = b;
        if (bType == BlockType.STONESLAB) bHalf = true;
        else bHalf = false;
        owner = o;
        parent = p;
        position = pos;
        if (bType == BlockType.AIR) isSolid = false;
        else isSolid = true;
    }

    void CreateQuad(Cubeside side)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ScriptedMesh" + side.ToString();

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uvs = new Vector2[4];
        int[] triangles = new int[6];

        Vector2 uv00;
        Vector2 uv10;
        Vector2 uv01;
        Vector2 uv11;

        Vector3 p0;
        Vector3 p1;
        Vector3 p2;
        Vector3 p3;
        Vector3 p4;
        Vector3 p5;
        Vector3 p6;
        Vector3 p7;

        if (bHalf == true)
        {
            if (bType == BlockType.STONESLAB && (side == Cubeside.TOP || side == Cubeside.BOTTOM))
            {
                uv00 = new Vector2(0.4626f, 0.9876f);
                uv10 = new Vector2(0.4749f, 0.9876f);
                uv01 = new Vector2(0.4626f, 0.9999f);
                uv11 = new Vector2(0.4749f, 0.9999f);
            }
            else
            {
                uv00 = blockUVs[(int)(bType), 0];
                uv01 = blockUVs[(int)(bType), 1];
                uv10 = blockUVs[(int)(bType), 2];
                uv11 = blockUVs[(int)(bType), 3];
            }

            p0 = new Vector3(-0.5f, -0.5f, 0.5f);
            p1 = new Vector3(0.5f, -0.5f, 0.5f);
            p2 = new Vector3(0.5f, -0.5f, -0.5f);
            p3 = new Vector3(-0.5f, -0.5f, -0.5f);
            p4 = new Vector3(-0.5f, 0f, 0.5f);
            p5 = new Vector3(0.5f, 0f, 0.5f);
            p6 = new Vector3(0.5f, 0f, -0.5f);
            p7 = new Vector3(-0.5f, 0f, -0.5f);
        }
        else
        {
            if (bType == BlockType.GRASS && side == Cubeside.TOP)
            {
                uv00 = blockUVs[0, 0];
                uv10 = blockUVs[0, 1];
                uv01 = blockUVs[0, 2];
                uv11 = blockUVs[0, 3];
            }
            else if (bType == BlockType.GRASS && side == Cubeside.BOTTOM)
            {
                uv00 = blockUVs[(int)(BlockType.DIRT), 0];
                uv10 = blockUVs[(int)(BlockType.DIRT), 1];
                uv01 = blockUVs[(int)(BlockType.DIRT), 2];
                uv11 = blockUVs[(int)(BlockType.DIRT), 3];
            }
            else if (bType == BlockType.SANDSTONE && side == Cubeside.TOP)
            {
                uv00 = new Vector2(0.2625f, 0.9875f);
                uv10 = new Vector2(0.275f, 0.9875f);
                uv01 = new Vector2(0.2625f, 1f);
                uv11 = new Vector2(0.275f, 1f);
            }
            else if (bType == BlockType.SANDSTONE && side == Cubeside.BOTTOM)
            {
                uv00 = new Vector2(0.25f, 0.9875f);
                uv10 = new Vector2(0.2625f, 0.9875f);
                uv01 = new Vector2(0.25f, 1f);
                uv11 = new Vector2(0.2625f, 1f);
            }
            else if (bType == BlockType.BOOK_SHELF && (side == Cubeside.TOP || side == Cubeside.BOTTOM))
            {
                uv00 = blockUVs[(int)(BlockType.WOOD), 0];
                uv10 = blockUVs[(int)(BlockType.WOOD), 1];
                uv01 = blockUVs[(int)(BlockType.WOOD), 2];
                uv11 = blockUVs[(int)(BlockType.WOOD), 3];
            }
            else if (bType == BlockType.QUARTZ_BLOCK && side == Cubeside.TOP)
            {
                uv00 = new Vector2(0.7625f, 0.9875f);
                uv10 = new Vector2(0.775f, 0.9875f);
                uv01 = new Vector2(0.7625f, 1f);
                uv11 = new Vector2(0.775f, 1f);
            }
            else if (bType == BlockType.QUARTZ_BLOCK && side == Cubeside.BOTTOM)
            {
                uv00 = new Vector2(0.6875f, 0.9875f);
                uv10 = new Vector2(0.7f, 0.9875f);
                uv01 = new Vector2(0.6875f, 1f);
                uv11 = new Vector2(0.7f, 1f);
            }
            else if (bType == BlockType.QUARTZ_BLOCK_CHISELED && (side == Cubeside.TOP || side == Cubeside.BOTTOM))
            {
                uv00 = new Vector2(0.7125f, 0.9875f);
                uv10 = new Vector2(0.725f, 0.9875f);
                uv01 = new Vector2(0.7125f, 1f);
                uv11 = new Vector2(0.725f, 1f);
            }
            else if (bType == BlockType.QUARTZ_BLOCK_LINES && (side == Cubeside.TOP || side == Cubeside.BOTTOM))
            {
                uv00 = new Vector2(0.7375f, 0.9875f);
                uv10 = new Vector2(0.75f, 0.9875f);
                uv01 = new Vector2(0.7375f, 1f);
                uv11 = new Vector2(0.75f, 1f);
            }
            else if (bType == BlockType.SNOW_GRASS && side == Cubeside.TOP)
            {
                uv00 = blockUVs[(int)(BlockType.SNOW), 0];
                uv10 = blockUVs[(int)(BlockType.SNOW), 1];
                uv01 = blockUVs[(int)(BlockType.SNOW), 2];
                uv11 = blockUVs[(int)(BlockType.SNOW), 3];
            }
            else if (bType == BlockType.SNOW_GRASS && side == Cubeside.BOTTOM)
            {
                uv00 = blockUVs[(int)(BlockType.DIRT), 0];
                uv10 = blockUVs[(int)(BlockType.DIRT), 1];
                uv01 = blockUVs[(int)(BlockType.DIRT), 2];
                uv11 = blockUVs[(int)(BlockType.DIRT), 3];
            }
            else if ((bType == BlockType.TREE_BIRCH || bType == BlockType.TREE_JUNGLE || bType == BlockType.TREE_SIDE || bType == BlockType.TREE_SPRUCE) && (side == Cubeside.TOP || side == Cubeside.BOTTOM))
            {
                uv00 = new Vector2(0.9375f, 0.9875f);
                uv10 = new Vector2(0.95f, 0.9875f);
                uv01 = new Vector2(0.9375f, 1f);
                uv11 = new Vector2(0.95f, 1f);
            }
            else if (bType == BlockType.STONESLAB2 && (side == Cubeside.TOP || side == Cubeside.BOTTOM))
            {
                uv00 = new Vector2(0.4626f, 0.9876f);
                uv10 = new Vector2(0.4749f, 0.9876f);
                uv01 = new Vector2(0.4626f, 0.9999f);
                uv11 = new Vector2(0.4749f, 0.9999f);
            }
            else
            {
                uv00 = blockUVs[(int)(bType), 0];
                uv01 = blockUVs[(int)(bType), 1];
                uv10 = blockUVs[(int)(bType), 2];
                uv11 = blockUVs[(int)(bType), 3];
            }

            p0 = new Vector3(-0.5f, -0.5f, 0.5f);
            p1 = new Vector3(0.5f, -0.5f, 0.5f);
            p2 = new Vector3(0.5f, -0.5f, -0.5f);
            p3 = new Vector3(-0.5f, -0.5f, -0.5f);
            p4 = new Vector3(-0.5f, 0.5f, 0.5f);
            p5 = new Vector3(0.5f, 0.5f, 0.5f);
            p6 = new Vector3(0.5f, 0.5f, -0.5f);
            p7 = new Vector3(-0.5f, 0.5f, -0.5f);
        }

        switch (side)
        {
            case Cubeside.BOTTOM:
                vertices = new Vector3[] { p0, p1, p2, p3 };
                normals = new Vector3[] { Vector3.down, Vector3.down, Vector3.down, Vector3.down };
                uvs = new Vector2[] { uv10, uv11, uv01, uv00 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case Cubeside.TOP:
                vertices = new Vector3[] { p7, p6, p5, p4 };
                normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                uvs = new Vector2[] { uv10, uv11, uv01, uv00 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case Cubeside.LEFT:
                vertices = new Vector3[] { p7, p4, p0, p3 };
                normals = new Vector3[] { Vector3.left, Vector3.left, Vector3.left, Vector3.left };
                uvs = new Vector2[] { uv10, uv11, uv01, uv00 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case Cubeside.RIGHT:
                vertices = new Vector3[] { p5, p6, p2, p1 };
                normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                uvs = new Vector2[] { uv10, uv11, uv01, uv00 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case Cubeside.FRONT:
                vertices = new Vector3[] { p4, p5, p1, p0 };
                normals = new Vector3[] { Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
                uvs = new Vector2[] { uv10, uv11, uv01, uv00 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
            case Cubeside.BACK:
                vertices = new Vector3[] { p6, p7, p3, p2 };
                normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
                uvs = new Vector2[] { uv10, uv11, uv01, uv00 };
                triangles = new int[] { 3, 1, 0, 3, 2, 1 };
                break;
        }
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GameObject quad = new GameObject("Quad");
        quad.transform.position = position;
        quad.transform.parent = parent.transform;

        MeshFilter meshFilter = (MeshFilter)quad.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = mesh;
    }

    int ConvertBlockIndexToLocal(int i)
    {
        if (i == -1) i = World.chunkSize - 1;
        else if (i == World.chunkSize) i = 0;
        return i;
    }

    public bool HasSolidNeighbour(int x, int y, int z)
    {
        Block[,,] chunks;

        if (x < 0 || x >= World.chunkSize || y < 0 || y >= World.chunkSize || z < 0 || z >= World.chunkSize)
        {
            Vector3 neighbourChunkPos = this.parent.transform.position + new Vector3((x - (int)position.x) * World.chunkSize, (y - (int)position.y) * World.chunkSize, (z - (int)position.z) * World.chunkSize);
            string nName = World.BuildChunkName(neighbourChunkPos);

            x = ConvertBlockIndexToLocal(x);
            y = ConvertBlockIndexToLocal(y);
            z = ConvertBlockIndexToLocal(z);

            Chunk nChunk;
            if (World.chunks.TryGetValue(nName, out nChunk))
            {
                chunks = nChunk.chunkData;
            }
            else return false;
        }
        else chunks = owner.chunkData;

        try
        {
            return chunks[x, y, z].isSolid && !chunks[x, y, z].bHalf && bType != BlockType.STONESLAB &&
                    bType != BlockType.LEAVES && chunks[x, y, z].bType != BlockType.LEAVES &&
                    bType != BlockType.LEAVES_JUNGLE && chunks[x, y, z].bType != BlockType.LEAVES_JUNGLE &&
                    bType != BlockType.LEAVES_SPRUCE && chunks[x, y, z].bType != BlockType.LEAVES_SPRUCE;
        }
        catch (System.IndexOutOfRangeException ex) { }

        return false;
    }

    public void Draw()
    {
        if (bType == BlockType.AIR) return;

        if (!HasSolidNeighbour((int)position.x, (int)position.y, (int)position.z + 1))
            CreateQuad(Cubeside.FRONT);
        if (!HasSolidNeighbour((int)position.x, (int)position.y, (int)position.z - 1))
            CreateQuad(Cubeside.BACK);
        if (!HasSolidNeighbour((int)position.x, (int)position.y + 1, (int)position.z))
            CreateQuad(Cubeside.TOP);
        if (!HasSolidNeighbour((int)position.x, (int)position.y - 1, (int)position.z))
            CreateQuad(Cubeside.BOTTOM);
        if (!HasSolidNeighbour((int)position.x - 1, (int)position.y, (int)position.z))
            CreateQuad(Cubeside.LEFT);
        if (!HasSolidNeighbour((int)position.x + 1, (int)position.y, (int)position.z))
            CreateQuad(Cubeside.RIGHT);
    }

    public bool HitBlock()
    {
        bType = BlockType.AIR;
        bHalf = false;
        isSolid = false;
        owner.ReDraw();
        return true;
    }

    public bool BuildBlock(BlockType b)
    {
        bType = b;
        if (bType == BlockType.STONESLAB && bHalf == false) bHalf = true;
        else if (bType == BlockType.STONESLAB && bHalf == true)
        {
            bHalf = false;
            bType = BlockType.STONESLAB2;
        }
        else bHalf = false;
        isSolid = true;
        owner.ReDraw();
        return true;
    }
}
