using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class Drop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (this.name != "BackGround")
        {
            if(this.transform.parent.name == "ItemSlot")
            {
                if (this.transform.childCount > 0)
                {
                    this.transform.GetChild(0).SetParent(Drag.itemParentTr);
                    Drag.itemTr.SetParent(this.transform);
                    Drag.itemTr.localPosition = Vector3.zero;
                    Drag.itemParentTr.GetChild(0).localPosition = Vector3.zero;
                    Item temp = GameManager._instance.PlayerInv.inventory[int.Parse(this.name) - 1];
                    GameManager._instance.PlayerInv.inventory[int.Parse(this.name) - 1] = GameManager._instance.PlayerInv.inventory[int.Parse(Drag.itemParentTr.name) - 1];
                    GameManager._instance.PlayerInv.inventory[int.Parse(Drag.itemParentTr.name) - 1] = temp;
                }
                else
                {
                    Drag.itemTr.SetParent(this.transform);
                    Drag.itemTr.localPosition = Vector3.zero;
                    if (Drag.itemParentTr != this.transform)
                    {
                        GameManager._instance.PlayerInv.inventory[int.Parse(this.name) - 1] = GameManager._instance.PlayerInv.inventory[int.Parse(Drag.itemParentTr.name) - 1];
                        GameManager._instance.PlayerInv.inventory[int.Parse(Drag.itemParentTr.name) - 1] = new Item();
                    }
                }
            }
        }
    }
}
