using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScale : MonoBehaviour
{
    public float scale = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        this.transform.localScale = new Vector2(Screen.height / scale / 176, Screen.height / scale / 167);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
