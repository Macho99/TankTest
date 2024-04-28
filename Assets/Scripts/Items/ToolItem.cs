using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ToolItem : NetworkBehaviour
{
    [SerializeField] private ToolItemData toolItemData;
    private MultiParentConstraint rig;

    [Networked, OnChangedRender(nameof(OnActiveToolItem))] private float weight { get; set; }
    private void Awake()
    {
        rig = GetComponent<MultiParentConstraint>();
        rig.weight = 0f;
    }
    public override void Spawned()
    {
       
        weight   = 0f;
        transform.GetChild(0).gameObject.SetActive(false);
    }
    public void ActiveToolItem(bool isActive)
    {
        weight = isActive ? 1f : 0f;

     
    }
    private void OnActiveToolItem()
    {
        rig.weight = weight;

        transform.GetChild(0).gameObject.SetActive(rig.weight >= 1f ? true : false);

    }
}
