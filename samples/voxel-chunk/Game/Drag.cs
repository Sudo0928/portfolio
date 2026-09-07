using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Drag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static Transform itemParentTr;
    public static Transform itemTr;

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemParentTr = this.gameObject.transform.parent;
        itemTr = this.gameObject.transform;
        this.transform.SetParent(UIManager._instance.Inventory.transform);
        this.GetComponent<Image>().raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameManager._instance.PlayerInv.DrawItemBar();
        this.GetComponent<Image>().raycastTarget = true;
        if(itemTr.parent.name == "Inventory")
        {
            itemTr.SetParent(itemParentTr);
            itemTr.localPosition = Vector3.zero;
        }
    }
}
