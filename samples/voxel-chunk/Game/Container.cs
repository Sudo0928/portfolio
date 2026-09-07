using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Container : MonoBehaviour
{
    public List<Item> container = new List<Item>();

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 54; i++)
        {
            container.Add(new Item());
        }
    }

    public void DrawContainer()
    {
        for (int i = 0; i < container.Count; i++)
        {
            if (UIManager._instance.Container.transform.GetChild(0).GetChild(i).childCount > 0)
            {
                for (int j = 0; j < UIManager._instance.Container.transform.GetChild(0).GetChild(i).childCount; j++)
                {
                    Destroy(UIManager._instance.Container.transform.GetChild(0).GetChild(i).GetChild(j).gameObject);
                }
            }
            if (container[i].ItemName != "")
            {
                GameObject temp = Instantiate(Resources.Load<GameObject>("Prefabs/ItemImg"), UIManager._instance.Container.transform.GetChild(0).GetChild(i));
                temp.GetComponent<Image>().sprite = Resources.Load<Sprite>("Item/" + container[i].Sprite);
                if (container[i].Count > 1)
                {
                    temp.transform.GetChild(0).GetComponent<Text>().text = container[i].Count.ToString();
                }
                else temp.transform.GetChild(0).GetComponent<Text>().text = "";
            }
        }
    }
}
