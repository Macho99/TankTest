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
        if (inputListner.pressButton.IsSet(ButtonType.FirstWeapon))
        {
            ResetAim();
            equipment.TakeOnMainWeapon(EquipmentSlotType.FirstMainWeapon);
        }
        else if (inputListner.pressButton.IsSet(ButtonType.SecondWeapon))
        {
            ResetAim();
            equipment.TakeOnMainWeapon(EquipmentSlotType.SecondMainWeapon);

        }
        else if (inputListner.pressButton.IsSet(ButtonType.SubWeapon))
        {
            ResetAim();
            equipment.TakeOnMainWeapon(EquipmentSlotType.SubWeapon);
        }
        else if (inputListner.pressButton.IsSet(ButtonType.MilyWeapon))
        {
            ResetAim();
            equipment.TakeOnMainWeapon(EquipmentSlotType.MilyWeapon);
        }
        else if (inputListner.pressButton.IsSet(ButtonType.PutWeapon))
        {
            if (equipment.GetMainWeapon() == null)
                return;
            ResetAim();
            equipment.TakeOffMainWeapon();
        }

        if (inputListner.currentInput.buttons.IsSet(ButtonType.Attack))
        {
            Weapon weapon = equipment.GetMainWeapon();

            if (weapon != null)
            {
                if (equipment.swichingState == WeaponSwichingState.Changing)
                    return;

                if (!weapon.CanAttack())
                    return;

                if (animator.GetCurrentAnimatorStateInfo((int)WeaponAnimLayer.Size).IsTag("Attack"))
                    return;


                animator.SetTrigger("Attack");
                if (weapon is Gun)
                {


                    Ray ray = new Ray();
                    ray.origin = camController.RayCasterTr.position;
                    ray.direction = camController.RayCasterTr.forward;
                    bool isHit = Physics.Raycast(ray, out RaycastHit hit, 900);

                    if (isHit)
                    {
                        ((Gun)equipment.GetMainWeapon()).Attack(hit.point);
                    }
                    else
                    {
                        ((Gun)equipment.GetMainWeapon()).Attack();
                    }


                }
                else
                {
                    equipment.GetMainWeapon().Attack();

                }

            }
        }

        if (inputListner.currentInput.buttons.IsSet(ButtonType.Adherence))
        {
            if (equipment.GetMainWeapon() != null)
            {
                if (equipment.swichingState == WeaponSwichingState.Changing)
                    return;

                if (equipment.GetMainWeapon() is Gun)
                {
                    camController.ChangeCamera(BasicCamController.CameraType.Aim);
                }
            }
        }
        if (inputListner.releaseButton.IsSet(ButtonType.Adherence))
        {
            ResetAim();
        }


    }
    public void ResetAim()
    {
        camController.ResetCamera();
    }
}
