using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ItemInfo : MonoBehaviourPunCallbacks
{
    public Item item;
    public ExtrudeSprite extrudeSprite;
    public MeshRenderer meshRenderer;
    public Material[] materials = new Material[1];
    public bool sameItem = false;
    public bool HandItem = false;

    public void ExtrudeItem(bool HandItem)
    {
        materials[0] = Resources.Load<Material>("Materials/" + item.Sprite);
        gameObject.AddComponent<ExtrudeSprite>().spriteMat = materials[0];
        meshRenderer.materials = materials;
        if (HandItem)
        {
            Destroy(GetComponent<Rigidbody>());
            Destroy(GetComponent<BoxCollider>());
        }
        this.HandItem = HandItem;
    }

    private void Update()
    {
        if (HandItem)
        {

        }
        else
        {
            transform.Rotate(0, 1, 0);
            Collider[] colliders = Physics.OverlapBox(this.transform.position, new Vector3(1, 1, 1), Quaternion.identity, 1 << LayerMask.NameToLayer("Player"));
            if (colliders.Length > 0)
            {
                colliders[0].GetComponent<Inventory>().ItemAdd(this.item);
                PhotonNetwork.Destroy(this.gameObject);
                return;
            }
        }
    }

    public static bool SameItem(Item a, Item b)
    {
        if (a.ItemName == b.ItemName && a.Sprite == b.Sprite && a.iType == b.iType && a.MaxCount == b.MaxCount)
        {
            return true;
        }
        else return false;
    }
}
