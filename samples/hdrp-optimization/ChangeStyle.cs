using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ChangeStyle : MonoBehaviour
{
    public List<Material> clean = new List<Material>();
    public List<Material> worn = new List<Material>();

    public bool isClean = false;

    private MeshRenderer _meshRenderer;

    public string assetMaterialsPath = "Assets/AtmosphericHouse/Materials/";

    private void Start()
    {
        if(clean.IsUnityNull() || worn.IsUnityNull()) InsertMaterials();
        _meshRenderer = GetComponent<MeshRenderer>();
        ChangeTextures();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F)) ChangeTextures();
    }

    public void InsertMaterials()
    {
        clean.Clear();
        worn .Clear();
        _meshRenderer = GetComponent<MeshRenderer>();
        Material[] thisMaterials = _meshRenderer.sharedMaterials;
        Material[] material = FindAllMaterials();
        for (int i = 0; i < thisMaterials.Length; i++)
        {
            string name = thisMaterials[i].name.Replace("_clean", "").Replace("_worn", "");
            Material[] temp = material.Where(mat => mat.name.Contains(name)).ToArray();
            foreach (Material mat in temp)
            {
                if (mat.name.Contains("_clean")) clean.Add(mat);
                else if (mat.name.Contains("_worn")) worn.Add(mat);
                else
                {
                    clean.Add(mat);
                    worn.Add(mat);
                }
            }
        }
    }

    public void ChangeTextures()
    {
        isClean = !isClean;
        
        if (isClean)
        {
            if (gameObject.name.Contains("Decal")) _meshRenderer.enabled = false;
            else _meshRenderer.materials = clean.ToArray();
        }
        else
        {
            if (gameObject.name.Contains("Decal")) _meshRenderer.enabled = true;
            else _meshRenderer.materials = worn.ToArray();
        }
    }

    public Material[] FindAllMaterials()
    {
        List<Material> materials = new List<Material>();

        string[] paths = AssetDatabase.GetAllAssetPaths();
        string[] filteredPaths = paths.Where(path => path.Contains(assetMaterialsPath)).OrderBy(path => path).ToArray();
        foreach (string path in filteredPaths)
        {
            if (!AssetDatabase.LoadAssetAtPath<Material>(path).IsUnityNull())
            {
                materials.Add(AssetDatabase.LoadAssetAtPath<Material>(path));
            }
        }

        return materials.ToArray();
    }
}