using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Chunk
{
    public Material cubematerial;
    public Block[,,] chunkData;
    public GameObject chunk;

    public static List<string> LoadedChunks = new List<string>();

    public List<Component> t = new List<Component>();

    void BuildChunk()
    {
        bool dataFromFile = false;

        chunkData = new Block[World.chunkSize, World.chunkSize, World.chunkSize];

        DirectoryInfo di = new DirectoryInfo(GameManager._instance.strPath + "/ChunksData/");
        FileInfo[] file = di.GetFiles("*.json");
        
        if (file.Length > 0)
        {
            for(int i = 0; i < file.Length; i++)
            {
                if (file[i].Name.Replace(".json", "") == chunk.name)
                {
                    dataFromFile = true;
                    break;
                }
                else dataFromFile = false;
            }

            if(dataFromFile && !LoadedChunks.Contains(chunk.name))
            {
                string jdata = File.ReadAllText(GameManager._instance.strPath + "/ChunksData/" + chunk.name + ".json");

                ChunkSave data = Newtonsoft.Json.JsonConvert.DeserializeObject<ChunkSave>(jdata);

                for (int z = 0; z < World.chunkSize; z++)
                {
                    for (int x = 0; x < World.chunkSize; x++)
                    {
                        for (int y = 0; y < World.chunkSize; y++)
                        {
                            Vector3 pos = new Vector3(x, y, z);
                            chunkData[x, y, z] = new Block(data.b[x, y, z], pos, chunk.gameObject, this);
                        }
                    }
                }

                LoadedChunks.Add(chunk.name);
            }
            else
            {
                for (int z = 0; z < World.chunkSize; z++)
                {
                    for (int x = 0; x < World.chunkSize; x++)
                    {
                        for (int y = 0; y < World.chunkSize; y++)
                        {
                            Vector3 pos = new Vector3(x, y, z);
                            chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos, chunk.gameObject, this);
                        }
                    }
                }
            }
        }
        else
        {
            for (int z = 0; z < World.chunkSize; z++)
            {
                for (int x = 0; x < World.chunkSize; x++)
                {
                    for (int y = 0; y < World.chunkSize; y++)
                    {
                        Vector3 pos = new Vector3(x, y, z);
                        if (pos.y == 0 && chunk.transform.position.y == 0 && chunk.transform.position == Vector3.zero)
                        {
                            chunkData[x, y, z] = new Block(Block.BlockType.GRASS, pos, chunk, this);
                        }
                        else chunkData[x, y, z] = new Block(Block.BlockType.AIR, pos, chunk, this);
                    }
                }
            }
        }
    }

    public void DrawChunk()
    {
        for(int z = 0; z < World.chunkSize; z++)
        {
            for(int x = 0; x < World.chunkSize; x++)
            {
                for(int y = 0; y < World.chunkSize; y++)
                {
                    chunkData[x, y, z].Draw();
                }
            }
        }

        CombineQuads(chunk, cubematerial);

        MeshCollider meshCollider = chunk.gameObject.GetComponent<MeshCollider>();

        if(meshCollider == null)
        {
            meshCollider = chunk.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = chunk.GetComponent<MeshFilter>().mesh;
        }
        else
        {
            meshCollider.sharedMesh = chunk.transform.GetComponent<MeshFilter>().mesh;
        }
    }

    public void CombineQuads(GameObject chunk, Material cubematerial)
    {
        //1. 모든 자식 오브젝트의 Mesh를 결합
        MeshFilter[] meshFilters = chunk.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        MeshFilter meshFilter = chunk.gameObject.GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            //2. 상위 오브젝트에 새로운 Mesh 만들기
            MeshFilter mf = (MeshFilter)chunk.gameObject.AddComponent(typeof(MeshFilter));
            mf.mesh = new Mesh();

            //3. 자식 오브젝트 끼리 결합된 Mesh를 상위 오브젝트의 Mesh에 추가
            mf.mesh.CombineMeshes(combine);

            //4. 상위 객체를 위한 렌더러 만들기
            MeshRenderer renderer = chunk.gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            renderer.material = cubematerial;
        }
        else
        {
            //3. 자식 오브젝트 끼리 결합된 Mesh를 상위 오브젝트의 Mesh에 추가
            meshFilter.mesh.Clear();
            meshFilter.mesh = new Mesh();
            meshFilter.mesh.CombineMeshes(combine);
        }

        //5. 결합되지 않은 모든 항목 삭제
        foreach (Transform quad in chunk.transform)
        {
            GameObject.Destroy(quad.gameObject);
        }

        return;
    }

    public Chunk(Vector3 position, Material c)
    {
        chunk = new GameObject(World.BuildChunkName(position));
        chunk.transform.position = position;
        cubematerial = c;
        BuildChunk();
    }

    public Chunk(Vector3 position)
    {
        chunk = new GameObject(World.BuildChunkName(position));
        chunk.transform.position = position;
        cubematerial = GameManager._instance.textureAtlas;
    }

    public void ReDraw()
    {
        DrawChunk();
    }

    private bool Load()
    {
        bool Check = false;

        string[] file = Directory.GetFiles(GameManager._instance.strPath + "/ChunksData/", "*.json");

        if (file.Length > 0)
        {
            Check = true;
        }

        return Check;
    }
}
