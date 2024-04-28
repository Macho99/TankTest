using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

public class PlayerRigManager : NetworkBehaviour
{

    public enum RigType { MultiAim }

    [SerializeField] private MultiRotationConstraint aimConstraint;
    [SerializeField] private TwoBoneIKConstraint leftHandRig;
    [SerializeField] private TwoBoneIKConstraint rightHandRig;
    [SerializeField] private Transform leftHandPivot;
    [Networked,OnChangedRender(nameof(OnChangeRigWeight))] private float multiAimweight { get; set; }

    private void Awake()
    {

    }
    public override void Spawned()
    {

        ChangeRightHandWegiht(0f);
        ChangeLeftHandWegiht(0f);
        ChangeRigWeight(1f);
    }
    public void ChangeLeftHandWegiht(float newWeight)
    {
        this.multiAimweight = newWeight;
        leftHandRig.weight = multiAimweight;
    }
    public void ChangeRightHandWegiht(float newWeight)
    {
        this.multiAimweight = newWeight;
        rightHandRig.weight = multiAimweight;
    }
  
    public void ChangeRigWeight(float newWeight)
    {
        this.multiAimweight = newWeight;
    }

    private void OnChangeRigWeight()
    {
        aimConstraint.weight = multiAimweight;
    }

}
