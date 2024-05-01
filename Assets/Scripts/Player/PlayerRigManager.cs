using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

public class PlayerRigManager : NetworkBehaviour
{

    public enum RigType { MultiAim }

    [SerializeField] private MultiRotationConstraint bodyConstraint;
    [SerializeField] private MultiAimConstraint aimConstraint;
    [SerializeField] private TwoBoneIKConstraint leftHandRig;
    [SerializeField] private TwoBoneIKConstraint rightHandRig;
    [SerializeField] private Transform leftHandPivot;
    [Networked, OnChangedRender(nameof(OnChangeBodyWeight))] public float Bodyweight { get; set; }
    [Networked, OnChangedRender(nameof(OnChangeLeftHandWeight))] public float LeftHandweight { get; set; }
    [Networked, OnChangedRender(nameof(OnAimWeight))] public float Aimweight { get; set; }


    private void Awake()
    {

    }
    public override void Spawned()
    {
        aimConstraint.weight = 1f;       
        Bodyweight = 0f;
    }

    private void OnChangeBodyWeight()
    {
        bodyConstraint.weight = Bodyweight;
    }
    private void OnChangeLeftHandWeight()
    {
        leftHandRig.weight = LeftHandweight;
    }
    private void OnAimWeight()
    {
        aimConstraint.weight = Aimweight;
    }
}
