using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager _instance;

    private void Awake()
    {
        if (!_instance) _instance = this;
    }

    public GameObject Inventory;
    public GameObject ItemBar;
    public GameObject Pick;
    public GameObject Container;

    public bool OpenUI = false;
    public bool OpenInv = false;
    public bool OpenContainer = false;

    public Image Health_UI;

    public Text Health_Text_UI;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Inventory.SetActive(false);
        Container.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenInv = !OpenInv;
        }
        UIOpen();
    }

    public void DrawUI()
    {
        Health_UI.fillAmount = (float)UserInfoManager._instance.PlayerHealth / UserInfoManager._instance.PlayerMaxHealth;

        Health_Text_UI.text = UserInfoManager._instance.PlayerHealth.ToString()
            + " / " + UserInfoManager._instance.PlayerMaxHealth.ToString();
    }

    private void UIOpen()
    {
        if (OpenInv)
        {
            Inventory.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Inventory.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }

        //if(OpenContainer)
        //{
        //    Container.SetActive(true);
        //    Cursor.lockState = CursorLockMode.None;
        //}
        //else
        //{
        //    Container.SetActive(false);
        //    Cursor.lockState = CursorLockMode.Locked;
        //}
    }
}
