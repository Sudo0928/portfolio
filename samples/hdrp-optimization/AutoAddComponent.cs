using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class AutoAddComponent : MonoBehaviour
{
    public List<MeshRenderer> _meshRenderers;

    public Transform targetObject;

    public void AutoAddComponentToAll()
    {
        _meshRenderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).ToList();
    }

    public void AutoAddComponentToChild()
    {
        _meshRenderers = new List<MeshRenderer>();
        FindChildMeshRenderers(targetObject);
        foreach (var item in _meshRenderers)
        {
            item.AddComponent<ChangeStyle>().InsertMaterials();
        }
    }

    public void AutoRemoveComponentToChild()
    {
        _meshRenderers = new List<MeshRenderer>();
        FindChildMeshRenderers(targetObject);
        foreach (var item in _meshRenderers)
        {
            DestroyImmediate(item.GetComponent<ChangeStyle>());
        }
    }

    public void FindChildMeshRenderers(Transform target)
    {
        for (int i = 0; i < target.childCount; i++)
        {
            FindChildMeshRenderers(target.GetChild(i));
            if (target.GetChild(i).TryGetComponent(out MeshRenderer temp))
            {
                _meshRenderers.Add(temp);
            }
        }
    }
}

[CustomEditor(typeof(AutoAddComponent))]
public class AutoAddComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AutoAddComponent script = target as AutoAddComponent;

        if (GUILayout.Button("AutoAddComponentToChild"))
        {
            script.AutoAddComponentToChild();
        }

        if (GUILayout.Button("AutoRemoveComponentToChild"))
        {
            script.AutoRemoveComponentToChild();
        }

        if (GUILayout.Button("AutoAddComponentToAll"))
        {
            script.AutoAddComponentToAll();
        }
    }
}