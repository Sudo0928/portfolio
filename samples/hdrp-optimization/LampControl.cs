using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampControl : MonoBehaviour
{

    private Light _light;
    private MeshRenderer _meshRenderer;
    public Material _lightOn;
    public Material _lightOff;

    public List<MeshRenderer> _childMesh = new List<MeshRenderer>();

    // Start is called before the first frame update
    void Start() 
    {
        _light = GetComponent<Light>();
        _meshRenderer = transform.parent.GetComponent<MeshRenderer>();

        OffLight();
    }

    public void OnLight()
    {
        _light.enabled = true;
        _meshRenderer.material = _lightOn;

        if (_childMesh == null) return;

        for(int i = 0; i < _childMesh.Count; i++)
        {
            _childMesh[i].material = _lightOn;
        }
    }

    public void OffLight()
    {
        _light.enabled = false;
        _meshRenderer.material = _lightOff;

        if (_childMesh == null) return;

        for (int i = 0; i < _childMesh.Count; i++)
        {
            _childMesh[i].material = _lightOff;
        }
    }
}
