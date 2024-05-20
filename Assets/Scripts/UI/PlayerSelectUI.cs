using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSelectUI : MonoBehaviour
{
    [SerializeField] private PlayerPreviewController previewController;
    [SerializeField] private UIImageDrag imageDrag;
    private void Awake()
    {
        imageDrag.onDrag += previewController.RotateModel;
    }
    private void OnEnable()
    {
        previewController.ResetModel();
    }

    public void PressRightButton(int index)
    {
        previewController.ChangePreset((AppearanceType)index, PlayerPreviewController.ModelTransition.Right);
    }
    public void PressLeftButton(int index)
    {
        previewController.ChangePreset((AppearanceType)index, PlayerPreviewController.ModelTransition.Left);
    }


}
