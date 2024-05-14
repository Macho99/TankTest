using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : NetworkBehaviour
{
    [SerializeField] private Equipment equipment;
    private PlayerInputListner inputListner;
    [SerializeField] private BasicCamController camController;
    private PlayerAnimEvent animEvent;
    private Animator animator;
    private void Awake()
    {
        animEvent = GetComponent<PlayerAnimEvent>();
        inputListner = GetComponent<PlayerInputListner>();
        animator = GetComponent<Animator>();
    }
    public override void FixedUpdateNetwork()
    {


    }
    public void WeaponControls()
    {
        if (equipment.GetMainWeapon() != null)
        {
            if (inputListner.pressButton.IsSet(ButtonType.PutWeapon))
            {
                equipment.UnEquipMainWeapon();
                return;
            }


            if (inputListner.currentInput.buttons.IsSet(ButtonType.Adherence))
            {
                camController.ChangeCamera(BasicCamController.CameraType.Aim);
            }
            else if (inputListner.releaseButton.IsSet(ButtonType.Adherence))
            {
                camController.ChangeCamera(BasicCamController.CameraType.None);
            }
        }

        if (equipment.swichingState == WeaponSwichingState.Changing)
            return;

        if (inputListner.currentInput.buttons.IsSet(ButtonType.FirstWeapon))
        {
            equipment.EquipMainWeapon(EquipmentSlotType.FirstMainWeapon);
        }
        else if (inputListner.currentInput.buttons.IsSet(ButtonType.SecondWeapon))
        {
            equipment.EquipMainWeapon(EquipmentSlotType.SecondMainWeapon);
        }
        else if (inputListner.currentInput.buttons.IsSet(ButtonType.SubWeapon))
        {
            equipment.EquipMainWeapon(EquipmentSlotType.SubWeapon);
        }
        else if (inputListner.currentInput.buttons.IsSet(ButtonType.MilyWeapon))
        {
            equipment.EquipMainWeapon(EquipmentSlotType.MilyWeapon);
        }

        if (inputListner.currentInput.buttons.IsSet(ButtonType.Attack))
        {
            Weapon weapon = equipment.GetMainWeapon();
            if (weapon != null)
            {

                animEvent.onFire += equipment.GetMainWeapon().Attack;

            }


        }




    }
    public void ChangeWeaponWeight(float weight)
    {
        equipment.ChangeWeaponWeight(weight);
    }
    public void ResetAim()
    {
        camController.ResetCamera();
    }
}
