using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json;
using UnityEngine.UI;

public class World : MonoBehaviourPunCallbacks
{
    public static int columnHeight = 1;
    public static int chunkSize = 8;
    public static int worldSize = 8;
    public static GameObject ChunksObject;

    public static Dictionary<string, Chunk> chunks;

    public static string BuildChunkName(Vector3 v)
    {
        return (int)v.x + "_" + (int)v.y + "_" + (int)v.z;
    }

    IEnumerator BuildWorld()
    {
        DirectoryInfo di = new DirectoryInfo(GameManager._instance.strPath + "/ChunksData/");
        FileInfo[] fi = di.GetFiles("*.json");

        if (fi.Length > 0)
        {
            for (int i = 0; i < fi.Length; i++)
            {
                string[] name = fi[i].Name.Replace(".json", "").Split('_');
                CreateChunk(int.Parse(name[0]), int.Parse(name[1]), int.Parse(name[2]));
            }
        }
        else
        {
            CreateChunk(0, 0, 0);
            yield return null;
        }

        foreach(KeyValuePair<string, Chunk> c in chunks)
        {
            c.Value.DrawChunk();
        }
    }

    public static void CreateChunk(int x, int y, int z)
    {
        Vector3 chunkPosition = new Vector3(x, y, z);

        Chunk c = new Chunk(chunkPosition, GameManager._instance.textureAtlas);

        c.chunk.transform.parent = ChunksObject.transform;

        chunks.Add(c.chunk.name, c);
    }

    private void Start()
    {
        ChunksObject = this.gameObject;
        chunks = new Dictionary<string, Chunk>();
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
        if (!File.Exists(GameManager._instance.strPath + "/ChunksData"))
        {
            Directory.CreateDirectory(GameManager._instance.strPath + "/ChunksData/");
        }
        StartCoroutine(BuildWorld());
    }

    public static IEnumerator GetWorldBlock(Vector3 pos)
    {
        #region 값 계산으로 청크 번호를 획득함.
        int Cx, Cy, Cz;
        if (pos.x < 0)
        {
            Cx = (int)((Mathf.Round(pos.x - chunkSize) + 1) / (float)chunkSize) * chunkSize;
        }
        else
        {
            Cx = (int)(Mathf.Round(pos.x) / (float)chunkSize) * chunkSize;
        }

        if (pos.y < 0)
        {
            Cy = (int)((Mathf.Round(pos.y - chunkSize) + 1) / (float)chunkSize) * chunkSize;
        }
        else
        {
            Cy = (int)(Mathf.Round(pos.y) / (float)chunkSize) * chunkSize;
        }

        if (pos.z < 0)
        {
            Cz = (int)((Mathf.Round(pos.z - chunkSize) + 1) / (float)chunkSize) * chunkSize;
        }
        else
        {
            Cz = (int)(Mathf.Round(pos.z) / (float)chunkSize) * chunkSize;
        }
        #endregion

        #region 해당 청크에서 몇번째 블럭을 눌렀는지 값을 얻음.
        int Block_x = (int)Mathf.Abs(Mathf.Round(pos.x) - Cx);
        int Block_y = (int)Mathf.Abs(Mathf.Round(pos.y) - Cy);
        int Block_z = (int)Mathf.Abs(Mathf.Round(pos.z) - Cz);
        #endregion

        string Cn = BuildChunkName(new Vector3(Cx, Cy, Cz));

        Chunk c;
        if (chunks.TryGetValue(Cn, out c))
        {
            BlockCreate.b = c.chunkData[Block_x, Block_y, Block_z];
        }
        else
        {
            c = new Chunk(new Vector3(Cx, Cy, Cz), GameManager._instance.textureAtlas);
            c.chunk.transform.parent = ChunksObject.transform;
            chunks.Add(c.chunk.name, c);
            BlockCreate.b = c.chunkData[Block_x, Block_y, Block_z];
        }

        yield return null;
    }

    public void SaveChunk()
    {
        string[] file = Directory.GetFiles(GameManager._instance.strPath + "/ChunksData/", "*.json");
        for (int i = 0; i < file.Length; i++)
        {
            File.Delete(file[i]);
        }

        List<string> CNameData = new List<string>(chunks.Keys);
        List<Chunk> CValueData = new List<Chunk>(chunks.Values);

        ChunkSave data;

        for(int i = 0; i < CNameData.Count; i++)
        {
            data = new ChunkSave();

            data.b = new Block.BlockType[chunkSize, chunkSize, chunkSize];

            for (int z = 0; z < chunkSize; z++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    for (int y = 0; y < chunkSize; y++)
                    {
                        data.b[x, y, z] = CValueData[i].chunkData[x, y, z].bType;
                    }
                }
            }

            data.ChunkPos = CNameData[i];

            string jdata = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(GameManager._instance.strPath + "/ChunksData/" + CNameData[i] + ".json", jdata);
        }
    }

    
}
