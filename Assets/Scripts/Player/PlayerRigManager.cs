using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRigManager : NetworkBehaviour
{

    public enum RigType { MultiAim }

    [SerializeField] private MultiRotationConstraint aimConstraint;
    private void Awake()
    {

    }
    public override void Spawned()
    {

    }
    public void ChangeRigWeight(float weight)
    {
        aimConstraint.weight = weight;

    }

}
