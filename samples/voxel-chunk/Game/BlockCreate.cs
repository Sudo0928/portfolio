using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;

public class BlockCreate : MonoBehaviourPunCallbacks
{
    public static Block b;

    public Inventory inventory;

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        if (!UIManager._instance.OpenInv)
        {
            RayCast();
        }
    }

    void RayCast()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5))
        {
            Chunk ck;

            // 레이를 맞은 오브젝트가 Chunk Dictionary가 없다면, 아래 구문을 실행 시키지 않음.
            if (!World.chunks.TryGetValue(hit.collider.name, out ck)) return;

            Vector3 point;


            point = hit.point - (hit.normal / 2f);
            Debug.Log(point);
            if (point.y - (int)point.y == 0.5f) point.y = hit.point.y;

            if (Input.GetMouseButtonDown(1))
            {
                point = hit.point + (hit.normal / 2f);
            }

            StartCoroutine(World.GetWorldBlock(point));

            ck = b.owner;

            bool update = false;

            if (Input.GetMouseButtonDown(0))
            {
                update = b.HitBlock();
                bool nullBlock = nullBlockCheck(ck);
                if (nullBlock)
                {
                    World.chunks.Remove(ck.chunk.name);
                    Destroy(ck.chunk);
                }
                photonView.RPC("RemoveBlock", RpcTarget.Others, point);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (inventory.inventory[int.Parse(UIManager._instance.Pick.transform.parent.name) - 1].ItemName != "" &&
                    inventory.inventory[int.Parse(UIManager._instance.Pick.transform.parent.name) - 1].iType == Item.ItemType.Block)
                {
                    Debug.Log(point);
                    update = b.BuildBlock(inventory.inventory[int.Parse(UIManager._instance.Pick.transform.parent.name) - 1].bType);
                    photonView.RPC("CreateBlock", RpcTarget.Others, point, int.Parse(UIManager._instance.Pick.transform.parent.name) - 1);
                }
            }

            BlockUpdata(update, ck, b);
        }
    }

    void BlockUpdata(bool update, Chunk Hit_c, Block b)
    {
        if (update)
        {
            List<string> Updates = new List<string>();
            float ThisChunk_x = Hit_c.chunk.transform.position.x;
            float ThisChunk_y = Hit_c.chunk.transform.position.y;
            float ThisChunk_z = Hit_c.chunk.transform.position.z;

            #region X에 관한것

            // 청크 안에서 X좌표의 0의 블럭을 클릭 했을때.
            if (b.position.x == 0)
                Updates.Add(World.BuildChunkName(new Vector3(ThisChunk_x - World.chunkSize, ThisChunk_y, ThisChunk_z)));

            // 청크 안에서 X좌표의 마지막 인덱스 블럭을 클릭 했을때.
            if (b.position.x == World.chunkSize - 1)
                Updates.Add(World.BuildChunkName(new Vector3(ThisChunk_x + World.chunkSize, ThisChunk_y, ThisChunk_z)));

            #endregion

            #region Y에 관한것

            // 청크 안에서 Y좌표의 0의 블럭을 클릭 했을때.
            if (b.position.y == 0)
                Updates.Add(World.BuildChunkName(new Vector3(ThisChunk_x, ThisChunk_y - World.chunkSize, ThisChunk_z)));

            // 청크 안에서 Y좌표의 마지막 인덱스 블럭을 클릭 했을때.
            if (b.position.y == World.chunkSize - 1)
                Updates.Add(World.BuildChunkName(new Vector3(ThisChunk_x, ThisChunk_y + World.chunkSize, ThisChunk_z)));

            #endregion

            #region Z에 관한것

            // 청크 안에서 Z좌표의 0의 블럭을 클릭 했을때.
            if (b.position.z == 0)
                Updates.Add(World.BuildChunkName(new Vector3(ThisChunk_x, ThisChunk_y, ThisChunk_z - World.chunkSize)));

            // 청크 안에서 Z좌표의 마지막 인덱스 블럭을 클릭 했을때.
            if (b.position.z == World.chunkSize - 1)
                Updates.Add(World.BuildChunkName(new Vector3(ThisChunk_x, ThisChunk_y, ThisChunk_z + World.chunkSize)));

            #endregion

            
            // 수정해야 되는 블럭 갯수 만큼 반복함.
            for (int i = 0; i < Updates.Count; i++)
            {
                // 수정해야 되는 이웃 청크를 수록.
                string Chunk_n = Updates[i];

                Chunk ck;

                // 수정 해야 되는 청크에 대한것.
                if (World.chunks.TryGetValue(Chunk_n, out ck))
                {
                    ck.ReDraw();
                }
            }
        }
    }

    bool nullBlockCheck(Chunk ck)
    {
        for (int x = 0; x < World.chunkSize; x++)
        {
            for (int y = 0; y < World.chunkSize; y++)
            {
                for (int z = 0; z < World.chunkSize; z++)
                {
                    if (ck.chunkData[x, y, z].bType != Block.BlockType.AIR) return false;
                }
            }
        }

        return true;
    }

    [PunRPC]
    public void CreateBlock(Vector3 point, int i)
    {
        Chunk ck;

        StartCoroutine(World.GetWorldBlock(point));

        ck = b.owner;

        bool update = false;

        update = b.BuildBlock(inventory.inventory[i].bType);

        BlockUpdata(update, ck, b);
    }

    [PunRPC]
    public void RemoveBlock(Vector3 point)
    {
        Chunk ck;

        StartCoroutine(World.GetWorldBlock(point));

        ck = b.owner;

        bool update = false;

        update = b.HitBlock();
        bool nullBlock = nullBlockCheck(ck);
        if (nullBlock)
        {
            World.chunks.Remove(ck.chunk.name);
            Destroy(ck.chunk);
        }

        BlockUpdata(update, ck, b);
    }
}
