using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

using Random = UnityEngine.Random;
public enum WeaponAnimLayerType { Base, Pistol, Rifle, Shotgun, Bazuka, Mily, Size }
public enum WeaponSwichingState { None, Changing }
public class WeaponController : NetworkBehaviour, IAfterSpawned
{
    [SerializeField] private Equipment equipment;
    private PlayerInputListner inputListner;
    [SerializeField] private MultiAimConstraint handAimIK;
    [SerializeField] private TwoBoneIKConstraint subHandIK;
    [SerializeField] private BasicCamController camController;
    [SerializeField, Capacity((int)AmmoType.Size)] public Ammo netAmmo { get; }
    private PlayerAnimEvent animEvent;
    private Animator animator;
    [Networked, OnChangedRender(nameof(UpdateMainWeapon))] private Weapon netMainWeapon { get; set; }
    private Weapon localWeapon;
    [Networked] private int weaponIndex { get; set; }
    [SerializeField] private WeaponPivotData[] weaponPivotData;
    private int weaponIndexOffset;
    [Networked] public WeaponSwichingState swichingState { get; private set; }
    private void Awake()
    {
        animEvent = GetComponent<PlayerAnimEvent>();
        inputListner = GetComponent<PlayerInputListner>();
        animator = GetComponent<Animator>();
        weaponIndexOffset = (int)EquipmentType.Rifle;

    }
    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            equipment.onMainWeaponUpdate += UpdateWeapon;
        }
    }
    public override void FixedUpdateNetwork()
    {


    }

    public void WeaponControls()
    {
        if (localWeapon != null)
        {
            if (inputListner.pressButton.IsSet(ButtonType.PutWeapon))
            {
                UnEquipWeapon();
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

        if (swichingState == WeaponSwichingState.Changing)
            return;

        if (inputListner.currentInput.buttons.IsSet(ButtonType.FirstWeapon))
        {
            EquipMainWeapon(EquipmentSlotType.FirstMainWeapon);
        }
        else if (inputListner.currentInput.buttons.IsSet(ButtonType.SecondWeapon))
        {
            EquipMainWeapon(EquipmentSlotType.SecondMainWeapon);
        }
        else if (inputListner.currentInput.buttons.IsSet(ButtonType.SubWeapon))
        {
            EquipMainWeapon(EquipmentSlotType.SubWeapon);
        }
        else if (inputListner.currentInput.buttons.IsSet(ButtonType.MilyWeapon))
        {
            EquipMainWeapon(EquipmentSlotType.MilyWeapon);
        }

        if (inputListner.currentInput.buttons.IsSet(ButtonType.Attack))
        {

            if (localWeapon != null)
            {
                if (!localWeapon.CanAttack())
                {
                    Debug.Log("공격불가");
                    return;
                }
                Debug.Log("공격가능");

                if (localWeapon is MilyWeapon)
                {

                    animator.SetFloat("AttackIndex", Random.Range(0, 4));
                }
                else
                {
                    Ray ray = new Ray();
                    ray.origin = camController.RayCasterTr.position;
                    ray.direction = camController.RayCasterTr.forward;
                    if (Physics.Raycast(ray, out RaycastHit hit, 100))
                    {
                        ((Gun)localWeapon).SetShotPoint(hit.point);
                        Debug.DrawLine(ray.origin, hit.point, Color.green);
                    }
                    else
                    {
                        ((Gun)localWeapon).SetShotPoint(Vector3.zero);
                    }

                  
                    animEvent.onFire += localWeapon.Attack;
                }
                animator.SetTrigger("Attack");

            }


        }
    }
    public void EquipMainWeapon(EquipmentSlotType slotType)
    {
        if (equipment.netItems[(int)slotType] == null)
        {
            Debug.Log("asdasd");
            return;
        }
        this.weaponIndex = (int)slotType;
        netMainWeapon = (Weapon)equipment.netItems[(int)slotType];
    }
    public void UpdateMainWeapon()
    {

        if (netMainWeapon == null)
        {
            if (localWeapon != null)
            {
                StopAllCoroutines();
                handAimIK.weight = 0f;
                subHandIK.weight = 0f;
                localWeapon.SetTarget(null);
                if (equipment.IsExistEquipmentItem(localWeapon))
                {
                    animator.SetTrigger("Put");
                    Debug.Log("put");
                    animEvent.onStartPutWeapon += OnStartPut;
                }
                else
                {
                    animator.SetLayerWeight((int)((WeaponItemSO)localWeapon.ItemData).AnimLayerType, 0f);

                }



            }

        }
        else
        {
            Debug.Log("asdadasd");
            swichingState = WeaponSwichingState.Changing;
            if (localWeapon == null)
            {
                localWeapon = netMainWeapon;
                SetupMainWeapon();
            }
            else
            {
                StopAllCoroutines();
                animEvent.onStartDrawWeapon -= OnStartDraw;
                animEvent.onEndDrawWeapon -= OnEndDraw;
                handAimIK.weight = 0f;
                subHandIK.weight = 0f;
                localWeapon.UnEquip();
                localWeapon.SetTarget(null);
                localWeapon.SetParent(weaponPivotData[WeaponPivotIndex()].subPivot);
                animator.SetLayerWeight((int)((WeaponItemSO)localWeapon.ItemData).AnimLayerType, 0f);
                Debug.Log((int)((WeaponItemSO)localWeapon.ItemData).AnimLayerType);
                Debug.Log(animator.GetLayerWeight((int)((WeaponItemSO)localWeapon.ItemData).AnimLayerType));
                SetupMainWeapon();

            }
        }

    }
    public void UpdateWeapon(int weaponIndex, Weapon weapon)
    {
        if (this.weaponIndex != weaponIndex)
            return;



        if (weapon == null)
        {
            if (netMainWeapon != null)
            {
                netMainWeapon = null;
                return;
            }
        }
        if (netMainWeapon == weapon)
        {
            netMainWeapon = null;
        }
        else if (netMainWeapon != weapon)
        {
            netMainWeapon = weapon;
        }
    }
    public void UnEquipWeapon()
    {
        if (netMainWeapon != null)
        {
            netMainWeapon = null;
        }
    }

    private void SetupMainWeapon()
    {
        localWeapon = netMainWeapon;
        animator.SetTrigger("Draw");
        localWeapon.SetTarget(subHandIK.data.target);
        animator.SetFloat("WeaponIndex", (float)((WeaponItemSO)localWeapon.ItemData).AnimLayerType);
        localWeapon.SetParent(weaponPivotData[WeaponPivotIndex()].subPivot);
        animEvent.onStartDrawWeapon += OnStartDraw;
        animEvent.onEndDrawWeapon += OnEndDraw;
        handAimIK.weight = 0f;
        subHandIK.weight = 0f;
    }
    public void OnStartPut()
    {
        Debug.Log(WeaponPivotIndex());
        localWeapon.SetParent(weaponPivotData[WeaponPivotIndex()].subPivot);
        animator.SetLayerWeight((int)((WeaponItemSO)localWeapon.ItemData).AnimLayerType, 0f);
        localWeapon = null;
        swichingState = WeaponSwichingState.None;
    }
    public void OnStartDraw()
    {
        localWeapon.SetActive(true);
        localWeapon.SetParent(weaponPivotData[WeaponPivotIndex()].mainPivot);

        Debug.Log("Start");
    }
    public void OnEndDraw()
    {
        StartCoroutine(DrawRoutine());

    }
    private IEnumerator DrawRoutine()
    {
        float speed = 2f;
        float currentValue = 0f;

        while (animator.GetLayerWeight((int)((WeaponItemSO)localWeapon.ItemData).AnimLayerType) < 1f)
        {
            currentValue += speed * Time.deltaTime;
            animator.SetLayerWeight((int)((WeaponItemSO)localWeapon.ItemData).AnimLayerType, currentValue);

            if (localWeapon.SubHandTarget != null)
            {
                handAimIK.weight += currentValue;
                subHandIK.weight += currentValue;
            }


            yield return null;
        }
        if (localWeapon.SubHandTarget != null)
        {
            handAimIK.weight += 1f;
            subHandIK.weight += 1f;
        }
        animator.SetLayerWeight((int)((WeaponItemSO)localWeapon.ItemData).AnimLayerType, 1f);
        swichingState = WeaponSwichingState.None;
    }
    public void ChangeWeaponWeight(float weight)
    {
        if (localWeapon == null)
            return;

        handAimIK.weight = weight;
        subHandIK.weight = weight;
    }


    public void ResetAim()
    {
        camController.ResetCamera();
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Draw");
        animator.ResetTrigger("Put");
        animator.ResetTrigger("Shot");
        if (!animator.GetCurrentAnimatorStateInfo((int)WeaponAnimLayerType.Size).IsName("Empty"))
        {

            animator.SetTrigger("Cancle");
            ChangeWeaponWeight(0f);
        }
    }

    public void AfterSpawned()
    {
        UpdateMainWeapon();
    }
    public int WeaponPivotIndex()
    {
        if (localWeapon == null)
            return -1;

        int index = (int)((WeaponItemSO)localWeapon.ItemData).EquipmentType - weaponIndexOffset;

        return index;
    }
}
[Serializable]
public class WeaponPivotData
{

    public EquipmentType Type;
    public Transform mainPivot;
    public Transform subPivot;


}