using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager _instance;

    private void Awake()
    {
        if (!_instance) _instance = this;
    }

    public string assetMaterialsPath = "Assets/AtmosphericHouse/Materials/";

    public static List<Material> materials = new List<Material>();

    public void AddAllMaterials()
    {
        materials.Clear();

        string[] paths = AssetDatabase.GetAllAssetPaths();
        string[] filteredPaths = paths.Where(path => path.Contains(assetMaterialsPath)).OrderBy(path => path).ToArray();
        foreach (string path in filteredPaths)
        {
            if (!AssetDatabase.LoadAssetAtPath<Material>(path).IsUnityNull())
            {
                materials.Add(AssetDatabase.LoadAssetAtPath<Material>(path));
            }
        }
    }

    public void ChangeMaterials()
    {

    }
}

[CustomEditor(typeof(MaterialManager))]
public class MaterialListEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MaterialManager script = (MaterialManager)target;

        if (GUILayout.Button("Add All Materials"))
        {
            script.AddAllMaterials();
        }
    }
}
