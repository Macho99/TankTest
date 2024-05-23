using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : NetworkBehaviour
{
    private MeshCollider meshCollider;
    [Networked] public Item pickupItem { get; private set; }

    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
    }
    public void PackgingItem(Item item)
    {
        Mesh mesh = item.transform.GetComponentInChildren<MeshFilter>().mesh;
        meshCollider.sharedMesh = mesh;
        pickupItem = item;
        pickupItem.SetParent(transform);
        pickupItem.SetActive(true);
    }
    private void PickUp(Inventory inventory)
    {
        meshCollider.sharedMesh = null;
        inventory.AddItem(pickupItem);
        pickupItem = null;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority)
            return;

        if (pickupItem != null)
        {
            Inventory inventory = other.GetComponentInChildren<Inventory>();
            if (inventory != null)
            {

                PickUp(inventory);
                return;
            }
        }


    }
}
