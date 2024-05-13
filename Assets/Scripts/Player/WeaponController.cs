using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : NetworkBehaviour
{
    [SerializeField] private Equipment equipment;
    private PlayerInputListner inputListner;
    [SerializeField] private BasicCamController camController;
    private Animator animator;
    private void Awake()
    {
        inputListner = GetComponent<PlayerInputListner>();
        animator = GetComponent<Animator>();
    }
    public override void FixedUpdateNetwork()
    {


    }
    public void WeaponControls()
    {



    }
    public void ResetAim()
    {
        camController.ResetCamera();
    }
}
