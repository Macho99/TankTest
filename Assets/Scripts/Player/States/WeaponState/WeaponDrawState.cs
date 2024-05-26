using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDrawState : WeaponStates
{
    //float transitionSpeed = 2f;
    //protected override bool CanEnterState()
    //{
    //    if (Machine.ActiveState == this)
    //        return false;
        

    //    return true;
    //}
    //protected override void OnEnterState()
    //{

    //}
    //protected override void OnEnterStateRender()
    //{
    //    owner.animator.SetTrigger("Draw");
    //    weaponController.animEvent.onStartDrawWeapon += weaponController.StartDraw;
    //    weaponController.animEvent.onStartDrawWeapon += weaponController.EndDraw;
    //}
    //protected override void OnExitStateRender()
    //{
    //    weaponController.animEvent.onStartDrawWeapon -= weaponController.StartDraw;
    //    weaponController.animEvent.onStartDrawWeapon -= weaponController.EndDraw;

    //}
    //protected override void OnFixedUpdate()
    //{
    //    if (weaponController.mainWeapon == null)
    //    {
    //        Machine.ForceActivateState((int)WeaponState.None);
    //        return;
    //    }
      
    //}
    //protected override void OnExitState()
    //{

    //}
    //public void StartDraw()
    //{
    //    weaponController.mainWeapon.SetParent(weaponController.SetupMainHandMainPivot(weaponController.mainWeapon));
    //    owner.animator.SetLayerWeight((int)((WeaponItemSO)weaponController.prevWeapon.ItemData).AnimLayerType, 0f);
    //}
    //public void EndDraw()
    //{
    //    StartCoroutine(LerpWeaponAnimLayer());
    //}

    //private IEnumerator LerpWeaponAnimLayer()
    //{
    //    float weight = 0f;

    //    Weapon weapon = weaponController.mainWeapon;
    //    Animator animator = owner.animator;

    //    while (animator.GetLayerWeight((int)((WeaponItemSO)weapon.ItemData).AnimLayerType) < 1f)
    //    {
    //        weight += transitionSpeed * Time.deltaTime;
    //        animator.SetLayerWeight((int)((WeaponItemSO)weapon.ItemData).AnimLayerType, weight);
    //        weaponController.handWeight = weight;
    //        yield return null;
    //    }
    //    animator.SetLayerWeight((int)((WeaponItemSO)weapon.ItemData).AnimLayerType, 1f);
    //    weapon.SetTarget(weaponController.subHandIK.data.target);
    //    weaponController.handWeight = 1f;
    //}
}
