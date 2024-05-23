using Fusion;
using Fusion.Addons.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

using Random = UnityEngine.Random;

public enum WeaponState { None, Reload, Put, Draw, Shot }
public enum WeaponAnimLayerType { Base, Pistol, Rifle, Shotgun, Bazuka, SniperRifle, Mily, Size }

public class WeaponController : NetworkBehaviour, IAfterSpawned, IStateMachineOwner
{
    [SerializeField] private WeaponStates[] weaponStates;
    [SerializeField] private Equipment equipment;
    private PlayerInputListner inputListner;
    [SerializeField] private MultiAimConstraint handAimIK;
    [SerializeField] private TwoBoneIKConstraint subHandIK;
    [SerializeField] private BasicCamController camController;
    [SerializeField] private Inventory inventory;
    private PlayerController controller;
    [Networked] private int weaponIndex { get; set; }
    [Networked, Capacity(20)] private NetworkDictionary<int, Ammo> netAmmos { get; }
    private Animator animator;
    [Networked, OnChangedRender(nameof(UpdateMainWeapon))] public Weapon mainWeapon { get; private set; }
    private Weapon prevWeapon;
    [Networked, OnChangedRender(nameof(UpdateHandWeight))] private float handWeight { get; set; }
    [SerializeField] private WeaponPivotData[] weaponPivotData;
    private int weaponIndexOffset;
    private PlayerAnimEvent animEvent;
    private float transitionSpeed;
    public StateMachine<WeaponStates> stateMachine { get; private set; }

    [Networked] private WeaponState weaponState { get; set; }

    private PlayerMainUI mainUI;
    private void Awake()
    {
        animEvent = GetComponentInParent<PlayerAnimEvent>();
        inputListner = GetComponentInParent<PlayerInputListner>();
        animator = GetComponentInParent<Animator>();
        weaponIndexOffset = (int)EquipmentType.Rifle;
        controller = GetComponentInParent<PlayerController>();
        transitionSpeed = 2f;
    }
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            equipment.onMainWeaponUpdate += ModifyMainWeapon;
        }

        if (HasInputAuthority)
        {
            mainUI = controller.mainUI;

        }

    }
    public int GetMainWeaponAnimLayer()
    {
        if (mainWeapon == null)
        {
            return (int)WeaponAnimLayerType.Base;
        }

        return (int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType;

    }
    private void ModifyMainWeapon(int index, Weapon weapon)
    {
        if (weaponIndex != index)
            return;


        if (weapon == null)
        {
            mainWeapon = null;
        }
        else
        {
            mainWeapon = weapon;
        }
    }

    public void WeaponControls()
    {
        Aiming();
        PressTakeOnMainweapon();
        Attack();
        Reloading();
    }


    private void PressTakeOnMainweapon()
    {


        if (inputListner.pressButton.IsSet(ButtonType.FirstWeapon))
        {
            ChangeMainWeapon(EquipmentSlotType.FirstMainWeapon);
        }
        else if (inputListner.pressButton.IsSet(ButtonType.SecondWeapon))
        {

            ChangeMainWeapon(EquipmentSlotType.SecondMainWeapon);
        }
        else if (inputListner.pressButton.IsSet(ButtonType.SubWeapon))
        {

            ChangeMainWeapon(EquipmentSlotType.SubWeapon);
        }
        else if (inputListner.pressButton.IsSet(ButtonType.MilyWeapon))
        {

            ChangeMainWeapon(EquipmentSlotType.MilyWeapon);
        }
        else if (inputListner.pressButton.IsSet(ButtonType.PutWeapon))
        {
            mainWeapon = null;
        }

    }

    private void Attack()
    {
        if (GetInput(out NetworkInputData input))
        {
            if (input.buttons.IsSet(ButtonType.Attack))
            {
                if (mainWeapon == null)
                    return;

                if (mainWeapon.CanAttack() == true)
                {
                    if (mainWeapon is Gun)
                    {
                        Ray ray = new Ray();
                        Vector3 origin = ((Gun)mainWeapon).GetMuzzlePoint() - camController.RayCasterTr.position;
                        float angle = Mathf.Abs(Vector3.Angle(origin, camController.RayCasterTr.forward));

                        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
                        float newZ = origin.magnitude * cos;

                        ray.origin = camController.RayCasterTr.position + camController.RayCasterTr.forward * newZ;
                        ray.direction = camController.RayCasterTr.forward;
                        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
                        {
                            ((Gun)mainWeapon).SetShotPoint(hit.point);
                            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.1f);
                        }
                        else
                        {
                            ((Gun)mainWeapon).SetShotPoint(ray.origin + ray.direction * 1000);
                        }
                        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red);
                    }
                    else if (mainWeapon is MilyWeapon)
                    {

                    }


                    animator.SetTrigger("Attack");
                    prevWeapon.Attack();
                    int totalAmmoCount = mainWeapon is Gun ? inventory.GetTotalAmmoItemCount((Gun)mainWeapon) : 1;
                    mainUI?.UpdateAmmo(mainWeapon, totalAmmoCount);
                }
            }



        }
    }
    private void Reloading()
    {
        if (!Runner.IsForward)
            return;

        if (inputListner.pressButton.IsSet(ButtonType.Reload))
        {
            if (!animator.GetCurrentAnimatorStateInfo((int)WeaponAnimLayerType.Size).IsName("Empty"))
                return;

            if (mainWeapon == null)
                return;
            if (weaponState != WeaponState.None)
                return;


            Gun mainGun = mainWeapon as Gun;
            if (mainGun == null)
                return;
            if (inventory.GetTotalAmmoItemCount(mainGun) <= 0)
                return;

            if (mainGun.currentAmmoCount >= ((GunItemSO)mainGun.ItemData).MaxAmmoCount)
                return;



            Debug.Log("reload");
            weaponState = WeaponState.Reload;
            handWeight = 0;
            animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)mainGun.ItemData).AnimLayerType);
            animEvent.onEndReload += EndReload;
            animator.SetTrigger("Reload");

        }

    }
    public void ChangeMainWeapon(EquipmentSlotType slotType)
    {
        if (weaponState != WeaponState.None)
            return;

        if (!animator.GetCurrentAnimatorStateInfo((int)WeaponAnimLayerType.Size).IsName("Empty"))
            return;


        Weapon weapon = equipment.GetWeapon(slotType);
        if (weapon != null)
        {
            if (weapon == mainWeapon)
                return;

            weaponState = WeaponState.Draw;
            mainWeapon = weapon;
            weaponIndex = (int)slotType;
        }
    }
    public void ChangeHandWeight(float weight = 0)
    {
        if (mainWeapon == null || mainWeapon is Gun == false)
        {
            handWeight = 0f;
            return;
        }

        handWeight = weight;
    }
    private void UpdateMainWeapon()
    {
        if (mainWeapon != prevWeapon)
        {
            if (prevWeapon == null)
            {
                if (mainWeapon != null)
                {
                    TakeOnMainWeapon();
                    UpdateAmmoCount();
                    prevWeapon = mainWeapon;

                }
            }
            else if (prevWeapon != null)
            {

                if (mainWeapon != null)
                {
                    TakeOffMainWeapon(prevWeapon);
                    prevWeapon = mainWeapon;
                    TakeOnMainWeapon();
                    UpdateAmmoCount();
                }
                else if (mainWeapon == null)
                {
                    if (equipment.IsHaveWeapon(prevWeapon))
                    {
                        PutMainWeapon(prevWeapon);
                        UpdateAmmoCount();
                    }
                    else
                    {
                        TakeOffMainWeapon(prevWeapon);
                        weaponState = WeaponState.None;
                        UpdateAmmoCount();
                        prevWeapon = mainWeapon;
                    }

                }
            }


        }
    }
    private void Aiming()
    {
        if (mainWeapon == null)
            return;


        if (GetInput(out NetworkInputData input))
        {
            if (input.buttons.IsSet(ButtonType.Adherence))
            {
                camController.ChangeCamera(BasicCamController.CameraType.Aim);
            }
        }

        if (inputListner.releaseButton.IsSet(ButtonType.Adherence))
        {
            camController.ChangeCamera(BasicCamController.CameraType.None);

        }
    }
    public void ResetAim()
    {
        camController.ChangeCamera(BasicCamController.CameraType.None);
    }
    public void UpdateAmmoCount()
    {
        int totalAmmoCount = 1;
        if (mainWeapon != null)
        {
            if (mainWeapon is Gun)
            {
                totalAmmoCount = inventory.GetTotalAmmoItemCount((Gun)mainWeapon);
            }
            else
            {
                totalAmmoCount = 1;
            }
        }
        mainUI?.ChangeWeaponUI(mainWeapon, totalAmmoCount);
    }
    private void PutMainWeapon(Weapon weapon)
    {
        camController.ResetCamera();
        weaponState = WeaponState.Put;
        weapon.SetTarget(null);
        handWeight = 0f;
        animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)weapon.ItemData).AnimLayerType);
        animEvent.onStartPutWeapon += StartPut;
        animator.SetTrigger("Put");

    }
    private void TakeOnMainWeapon()
    {
        camController.ResetCamera();
        handWeight = 0f;
        animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType);
        animEvent.onStartDrawWeapon += StartDraw;
        animEvent.onStartDrawWeapon += EndDraw;
        animator.SetTrigger("Draw");
    }
    private void TakeOffMainWeapon(Weapon prevWeapon)
    {
        camController.ResetCamera();
        handWeight = 0f;
        prevWeapon.SetTarget(null);
        animator.SetLayerWeight((int)((WeaponItemSO)prevWeapon.ItemData).AnimLayerType, 0f);
        prevWeapon.SetParent(SetupMainHandSubPivot(prevWeapon));

    }
    private void UpdateHandWeight()
    {
        subHandIK.weight = handWeight;
        handAimIK.weight = handWeight;
    }


    #region æ÷¥‘¿Ã∫•∆Æ

    private void EndReload()
    {
        handWeight = 1f;

        Gun mainGun = mainWeapon as Gun;

        int totalAmmoCount = inventory.GetTotalAmmoItemCount(mainGun);

        int currentAmmoCount = mainGun.currentAmmoCount;
        int maxAmmoCount = ((GunItemSO)mainGun.ItemData).MaxAmmoCount;

        int requiredCount = maxAmmoCount - currentAmmoCount;

        int amountCount = Mathf.Min(requiredCount, totalAmmoCount);


        mainGun.Reload(amountCount);
        AmmoType ammoType = ((GunItemSO)mainGun.ItemData).AmmoType;

        inventory.PullAmmoItemCount(ammoType, amountCount);
        totalAmmoCount -= requiredCount;

        mainUI?.UpdateAmmo(mainWeapon, totalAmmoCount);


        if (weaponState == WeaponState.Reload)
            weaponState = WeaponState.None;

    }
    private void StartPut()
    {
        prevWeapon.SetParent(SetupMainHandSubPivot(prevWeapon));
        animator.SetLayerWeight((int)((WeaponItemSO)prevWeapon.ItemData).AnimLayerType, 0f);
        prevWeapon = mainWeapon;
        weaponState = WeaponState.None;
        Debug.Log(weaponState);
    }

    private void StartDraw()
    {
        mainWeapon.SetParent(SetupMainHandMainPivot(mainWeapon));
        animator.SetLayerWeight((int)((WeaponItemSO)prevWeapon.ItemData).AnimLayerType, 0f);
    }
    private void EndDraw()
    {
        StartCoroutine(LerpWeaponAnimLayer());
    }
    private IEnumerator LerpWeaponAnimLayer()
    {
        float weight = 0f;

        while (animator.GetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType) < 1f)
        {
            weight += transitionSpeed * Time.deltaTime;
            animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, weight);
            handWeight = weight;
            yield return null;
        }
        animator.SetLayerWeight((int)((WeaponItemSO)mainWeapon.ItemData).AnimLayerType, 1f);
        mainWeapon.SetTarget(subHandIK.data.target);
        handWeight = 1f;

        weaponState = WeaponState.None;

    }
    #endregion


    public int WeaponPivotIndex(Weapon weapon)
    {
        if (weapon == null)
            return -1;

        int index = (int)((WeaponItemSO)weapon.ItemData).EquipmentType - weaponIndexOffset;

        return index;
    }
    public Transform SetupMainHandMainPivot(Weapon weapon)
    {
        if (weapon == null)
            return null;

        int index = WeaponPivotIndex(weapon);
        return weaponPivotData[index].mainPivot;
    }
    public Transform SetupMainHandSubPivot(Weapon weapon)
    {
        if (weapon == null)
            return null;

        int index = WeaponPivotIndex(weapon);
        return weaponPivotData[index].subPivot;
    }

    public void AfterSpawned()
    {
        UpdateMainWeapon();
        UpdateHandWeight();
    }

    public void CollectStateMachines(List<IStateMachine> stateMachines)
    {
        stateMachine = new StateMachine<WeaponStates>("Weapon", weaponStates);
        stateMachines.Add(stateMachine);
    }
}
[Serializable]
public class WeaponPivotData
{

    public EquipmentType Type;
    public Transform mainPivot;
    public Transform subPivot;


}