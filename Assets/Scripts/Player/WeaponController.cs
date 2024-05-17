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
    [SerializeField] private Inventory inventory;
    private PlayerController controller;
    [SerializeField, Capacity((int)AmmoType.Size)] public Ammo netAmmo { get; set; }
    private PlayerAnimEvent animEvent;
    private Animator animator;
    [Networked, OnChangedRender(nameof(UpdateMainWeapon))] private Weapon netMainWeapon { get; set; }
    private Weapon localWeapon;
    [Networked] private int weaponIndex { get; set; }
    [SerializeField] private WeaponPivotData[] weaponPivotData;
    private int weaponIndexOffset;

    [Networked] public WeaponSwichingState swichingState { get; private set; }


    private PlayerMainUI mainUI;
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


            if (inputListner.currentInput.buttons.IsSet(ButtonType.Adherence))
            {

                camController.ChangeCamera(BasicCamController.CameraType.Aim);
            }
            else if (inputListner.releaseButton.IsSet(ButtonType.Adherence))
            {

                camController.ChangeCamera(BasicCamController.CameraType.None);
            }

            if (localWeapon.weaponState != WeaponState.None)
                return;

            if (inputListner.pressButton.IsSet(ButtonType.PutWeapon))
            {
                localWeapon.ChangeState(WeaponState.Put);
                UnEquipWeapon();
                return;
            }

            if (inputListner.pressButton.IsSet(ButtonType.Reload))
            {
                if (localWeapon is Gun == false)
                    return;
                localWeapon.ChangeState(WeaponState.Reload);
                handAimIK.weight = 0f;
                subHandIK.weight = 0f;
                animEvent.onEndReload += () => { StartCoroutine(ChangeHandWeightRoutine()); localWeapon.ChangeState(WeaponState.None); };
                animator.SetTrigger("Reload");
                ((Gun)localWeapon).Reload();
                return;
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

                if (localWeapon is MilyWeapon)
                {
                    animator.SetFloat("AttackIndex", Random.Range(0, 4));
                }
                else
                {
                    animEvent.onFire += Fire;
                }
                animator.SetTrigger("Attack");

            }


        }
    }
    public void Fire()
    {
        if (localWeapon == null)
            return;

        Ray ray = new Ray();
        Vector3 origin = ((Gun)localWeapon).GetMuzzlePoint() - camController.RayCasterTr.position;
        //빗변의 길이와 방향

        float angle = Mathf.Abs(Vector3.Angle(origin, camController.RayCasterTr.forward));

        float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
        float newZ = origin.magnitude * cos;


        ray.origin = camController.RayCasterTr.position + camController.RayCasterTr.forward * newZ;
        ray.direction = camController.RayCasterTr.forward;
        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            ((Gun)localWeapon).SetShotPoint(hit.point);
            Debug.DrawLine(ray.origin, hit.point, Color.red, 0.1f);
        }
        else
        {
            ((Gun)localWeapon).SetShotPoint(ray.origin + ray.direction * 1000);
        }

        localWeapon.Attack();
    }

    public void EquipMainWeapon(EquipmentSlotType slotType)
    {
        if (equipment.netItems[(int)slotType] == null)
        {
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
                mainUI?.ChangeWeaponUI(null);


            }

        }
        else
        {
            Debug.Log("asdadasd");
            swichingState = WeaponSwichingState.Changing;
            if (localWeapon == null)
            {
                localWeapon = netMainWeapon;
                mainUI?.ChangeWeaponUI(localWeapon);
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
                netAmmo = null;

                return;
            }
        }
        if (netMainWeapon == weapon)
        {
            netMainWeapon = null;
            netAmmo = null;
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
        mainUI?.ChangeWeaponUI(localWeapon);
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
        localWeapon.ChangeState(WeaponState.None);
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
    private IEnumerator ChangeHandWeightRoutine()
    {
        float speed = 2f;
        float currentValue = 0f;

        while (currentValue < 1f)
        {
            currentValue += speed * Time.deltaTime;
            handAimIK.weight += currentValue;
            subHandIK.weight += currentValue;
            yield return null;
        }

        handAimIK.weight = 1f;
        subHandIK.weight = 1f;
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
        localWeapon.ChangeState(WeaponState.None);
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
        if (HasInputAuthority)
        {
            mainUI = GetComponent<PlayerController>().mainUI;
        }
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