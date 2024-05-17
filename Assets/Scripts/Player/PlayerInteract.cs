using Fusion;
using Fusion.Addons.SimpleKCC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerInteract : NetworkBehaviour
{
    private float rayDistance;
    [SerializeField] private Transform raycastTr;
    [SerializeField] private ItemContainer itemContainer;
    private PlayerInputListner inputListner;
    public Action<bool, DetectData> onDetect;
    private DetectData interactData;
    [Networked] public NetworkBool IsDetect { get; set; }

    private InteractBehavior[] interactBehaviors;
    private InteractObject interactObject;

    public InteractObject InteractObject { get { return interactObject; } set { interactObject = value; } }

    public DetectData InteractData { get { return interactData; } set => interactData = value; }

    private void Awake()
    {
        rayDistance = 5f;
        interactBehaviors = new InteractBehavior[(int)InteractType.Size];
        PlayerController controller = GetComponent<PlayerController>();
        inputListner = GetComponent<PlayerInputListner>();
    }
    public override void Spawned()
    {
        if (HasInputAuthority)
        {
            SetupLocalPlayerUI();
        }


    }

    public override void FixedUpdateNetwork()
    {

        RaycastDetect();


        if (Runner.IsForward)
        {
            if (inputListner.pressButton.IsSet(ButtonType.Interact))
            {
                if (TryInteract())
                {
                    interactObject.StartInteraction();
                }
            }


        }



    }
    public void RaycastDetect()
    {

        Ray ray = new Ray();
        ray.origin = raycastTr.position;
        ray.direction = raycastTr.transform.forward;

        //Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.TryGetComponent(out IDetectable detectObject))
            {
                detectObject.OnEnterDetect(out DetectData interctData);
                this.interactData = interctData;
                IsDetect = true;
                onDetect?.Invoke(IsDetect, this.interactData);
                return;
            }
        }

        if (interactData != null)
        {
            if (interactData.detectable != null)
                interactData.detectable.OnExitDetect();

            interactData = null;
        }
        interactObject = null;
        IsDetect = false;
        onDetect?.Invoke(IsDetect, null);


    }

    public bool TryInteract()
    {

        //if (!CanInteract())
        //    return false;

        Ray ray = new Ray();
        ray.origin = raycastTr.position;
        ray.direction = raycastTr.transform.forward;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.TryGetComponent(out InteractObject detectObject))
            {
                if (detectObject.Interact(this, out InteractObject interactObject))
                {
                    this.interactObject = interactObject;
                    return true;
                }
            }
        }
        return false;

    }
    private void SetupLocalPlayerUI()
    {
        AimUI aimUI = GameManager.UI.ShowSceneUI<AimUI>("UI/PlayerUI/AimUI");
        onDetect += aimUI.ReadInteractInfo;

    }

    public InteractBehavior GetInteractBehavior()
    {
        if (interactData.interactType == InteractType.None)
            return null;


        return interactBehaviors[(int)interactData.interactType];
    }

    public void StopInteract()
    {
        interactObject = null;
        interactData = default;
    }

    public void SearchItemInteract(InteractItemBox interactItemBox)
    {
        if (!itemContainer.SetupSearchData(interactItemBox))
            return;

        itemContainer.ActiveItemContainerUI(true);
    }

    public void StopSearchItemInteract()
    {
        itemContainer.RemoveSerachData();
        itemContainer.ActiveItemContainerUI(false);
    }


}
